using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

using Snap.Content.Abstractions.Interfaces;

namespace Snap.Content.Pak;

public sealed class PakContentProvider : IContentProvider, IDisposable
{
	private readonly FileStream _stream;
	private readonly Dictionary<string, Entry> _index = new(StringComparer.OrdinalIgnoreCase);
	private readonly IKeyProvider? _keyProvider;

	private enum CompType : byte { None = 0, Brotli = 1, Deflate = 2 }

	public bool HasEncryptedEntries { get; private set; }

	private readonly struct Entry
	{
		public readonly long Offset, Stored, Original;
		public readonly CompType Comp;
		public readonly bool Encrypted;
		public readonly byte[]? Nonce; // 12 bytes if encrypted

		public Entry(long o, long s, long r, CompType c, bool enc, byte[]? nonce)
		{
			Offset = o; Stored = s; Original = r; Comp = c;
			Encrypted = enc; Nonce = nonce;
		}
	}

	// keep old ctor for compatibility
	public PakContentProvider(string pakPath) : this(pakPath, keyProvider: null) { }

	public PakContentProvider(string pakPath, IKeyProvider? keyProvider)
	{
		_stream = new FileStream(pakPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		_keyProvider = keyProvider;
		ReadIndex();
	}

	private static byte[] BuildAad(string pathLogical, long originalLen, byte compByte)
	{
		var norm = pathLogical.Replace('\\', '/').TrimStart('/');
		var name = Encoding.UTF8.GetBytes(norm);

		var aad = new byte[name.Length + 8 + 1]; // name || original(LE) || comp
		Buffer.BlockCopy(name, 0, aad, 0, name.Length);
		BinaryPrimitives.WriteInt64LittleEndian(aad.AsSpan(name.Length, 8), originalLen);
		aad[name.Length + 8] = compByte;
		return aad;
	}

	private void ReadIndex()
	{
		using var br = new BinaryReader(_stream, Encoding.UTF8, leaveOpen: true);

		var magicBytes = br.ReadBytes(8);
		var magic = Encoding.ASCII.GetString(magicBytes).TrimEnd('\0');
		if (magic != "SNAPPAK")
			throw new InvalidDataException("Not a SNAP pack");

		long afterMagic = _stream.Position;
		short version = br.ReadInt16();
		short reserved = br.ReadInt16();

		if (version == 3)
		{
			int count = br.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				short pathLen = br.ReadInt16();
				var path = Encoding.UTF8.GetString(br.ReadBytes(pathLen)).Replace('\\', '/');

				long offset = br.ReadInt64();
				long stored = br.ReadInt64();  // stored bytes: ciphertext [+ tag] or plaintext
				long original = br.ReadInt64();  // original plaintext length
				var comp = (CompType)br.ReadByte();

				byte flags = br.ReadByte();
				bool enc = (flags & 0x01) != 0;

				byte[]? nonce = null;
				if (enc)
				{
					HasEncryptedEntries = true;

					// writer must have stored a 12-byte per-file nonce
					// nonce = br.ReadBytes(12);
					// if (nonce.Length != 12)
					// 	throw new InvalidDataException("Invalid nonce length in TOC.");
				}

				_index[path] = new Entry(offset, stored, original, comp, enc, nonce);
			}
			return;
		}

		if (version == 2)
		{
			int count = br.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				short pathLen = br.ReadInt16();
				var path = Encoding.UTF8.GetString(br.ReadBytes(pathLen)).Replace('\\', '/');

				long offset = br.ReadInt64();
				long stored = br.ReadInt64();
				long original = br.ReadInt64();
				var comp = (CompType)br.ReadByte();

				_index[path] = new Entry(offset, stored, original, comp, enc: false, nonce: null);
			}
			return;
		}

		// v1 fallback (no version, no comp/enc)
		_stream.Position = afterMagic;
		int countV1 = br.ReadInt32();
		for (int i = 0; i < countV1; i++)
		{
			short pathLen = br.ReadInt16();
			var path = Encoding.UTF8.GetString(br.ReadBytes(pathLen)).Replace('\\', '/');

			long offset = br.ReadInt64();
			long length = br.ReadInt64();

			_index[path] = new Entry(offset, length, length, CompType.None, enc: false, nonce: null);
		}
	}

	public bool Exists(string path) => _index.ContainsKey(Norm(path));

	public Stream OpenRead(string path)
	{
		path = Norm(path);
		if (!_index.TryGetValue(path, out var e))
			throw new FileNotFoundException(path);

		// Get stored slice for this entry
		var slice = new LimitedStream(_stream, e.Offset, e.Stored);

		// 1) Decrypt (if encrypted)
		Stream payload = e.Encrypted ? DecryptToMemory(slice, e, path) : slice;

		// 2) Decompress (if compressed)
		return e.Comp switch
		{
			CompType.None => payload,
			CompType.Brotli => new BrotliStream(payload, CompressionMode.Decompress, leaveOpen: false),
			CompType.Deflate => new DeflateStream(payload, CompressionMode.Decompress, leaveOpen: false),
			_ => payload
		};
	}

	private Stream DecryptToMemory(Stream slice, Entry e, string path)
	{
		if (_keyProvider is null)
			throw new CryptographicException($"Encrypted entry '{path}' but no key provider set.");
		var key = _keyProvider.GetArchiveKey();
		if (key is null || key.Length is not (16 or 24 or 32))
			throw new CryptographicException("Invalid AES key from key provider.");

		byte[] buf;
		using (var ms = new MemoryStream())
		{
			slice.CopyTo(ms);
			buf = ms.ToArray();
		}

		const int NONCE_LEN = 12;
		const int TAG_LEN = 16;
		if (buf.Length < NONCE_LEN + TAG_LEN)
			throw new CryptographicException("Ciphertext too short.");

		ReadOnlySpan<byte> nonce;
		ReadOnlySpan<byte> ciphertext;
		ReadOnlySpan<byte> tag;

		if (e.Nonce is { Length: NONCE_LEN })
		{
			// (Not used in Option B, but supported)
			nonce = e.Nonce;
			ciphertext = new ReadOnlySpan<byte>(buf, 0, buf.Length - TAG_LEN);
			tag = new ReadOnlySpan<byte>(buf, buf.Length - TAG_LEN, TAG_LEN);
		}
		else
		{
			// Option B: [nonce | ciphertext | tag]
			nonce = new ReadOnlySpan<byte>(buf, 0, NONCE_LEN);
			ciphertext = new ReadOnlySpan<byte>(buf, NONCE_LEN, buf.Length - NONCE_LEN - TAG_LEN);
			tag = new ReadOnlySpan<byte>(buf, buf.Length - TAG_LEN, TAG_LEN);
		}

		var plaintext = new byte[e.Original];
		var aad = BuildAad(path, e.Original, (byte)e.Comp);

		using var gcm = new AesGcm(key, TAG_LEN);
		try
		{
			gcm.Decrypt(nonce, ciphertext, tag, plaintext, aad);
		}
		catch (CryptographicException ex)
		{
			throw new CryptographicException(
				$"Decryption failed for '{path}'. Wrong key or tampered metadata.", ex);
		}

		return new MemoryStream(plaintext, writable: false);
	}

	public IEnumerable<string> List(string folder)
	{
		folder = Norm(folder);
		if (folder.Length > 0 && !folder.EndsWith('/'))
			folder += "/";

		foreach (var k in _index.Keys)
		{
			if (k.StartsWith(folder, StringComparison.OrdinalIgnoreCase))
				yield return k;
		}
	}

	private static string Norm(string p) => p.Replace('\\', '/').TrimStart('/');

	public void Dispose() => _stream.Dispose();
}
