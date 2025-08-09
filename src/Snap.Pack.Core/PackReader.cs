using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snap.Pack.Core;

public sealed record PakEntry(string Path, long Offset, long Stored, long Original, CompType Comp, bool Encrypted);

public static class PackReader
{
	public static (short Version, IReadOnlyList<PakEntry> Entries) ReadIndex(string pakPath)
	{
		using var fs = File.OpenRead(pakPath);
		using var br = new BinaryReader(fs, Encoding.UTF8, leaveOpen: true);

		var magic = Encoding.ASCII.GetString(br.ReadBytes(8)).TrimEnd('\0');
		if (magic != "SNAPPAK") throw new InvalidDataException("Not a SNAP pack");

		long afterMagic = fs.Position;

		// Try v3/v2 (both have version + reserved)
		short ver = br.ReadInt16();
		br.ReadInt16(); // reserved

		if (ver == 3)
		{
			int count = br.ReadInt32();
			var list = new List<PakEntry>(count);
			for (int i = 0; i < count; i++)
			{
				short pl = br.ReadInt16();
				var path = Encoding.UTF8.GetString(br.ReadBytes(pl)).Replace('\\', '/');
				long off = br.ReadInt64();
				long stored = br.ReadInt64();
				long orig = br.ReadInt64();
				var comp = (CompType)br.ReadByte();
				bool enc = br.ReadByte() != 0;
				list.Add(new PakEntry(path, off, stored, orig, comp, enc));
			}
			return (3, list);
		}
		if (ver == 2)
		{
			int count = br.ReadInt32();
			var list = new List<PakEntry>(count);
			for (int i = 0; i < count; i++)
			{
				short pl = br.ReadInt16();
				var path = Encoding.UTF8.GetString(br.ReadBytes(pl)).Replace('\\', '/');
				long off = br.ReadInt64();
				long stored = br.ReadInt64();
				long orig = br.ReadInt64();
				var comp = (CompType)br.ReadByte();
				list.Add(new PakEntry(path, off, stored, orig, comp, false));
			}
			return (2, list);
		}

		// Fallback: v1 (no version field)
		fs.Position = afterMagic;
		int countV1 = br.ReadInt32();
		var listV1 = new List<PakEntry>(countV1);
		for (int i = 0; i < countV1; i++)
		{
			short pl = br.ReadInt16();
			var path = Encoding.UTF8.GetString(br.ReadBytes(pl)).Replace('\\', '/');
			long off = br.ReadInt64();
			long len = br.ReadInt64();
			listV1.Add(new PakEntry(path, off, len, len, CompType.None, false));
		}
		return (1, listV1);
	}

	public static void Verify(string pakPath, TextWriter output, byte[]? key = null)
	{
		using var fs = File.OpenRead(pakPath);
		var (ver, entries) = ReadIndex(pakPath);

		byte[] buf = new byte[1 << 20];
		output.WriteLine($"Verifying {pakPath} (v{ver}) with {entries.Count} entries...");

		foreach (var e in entries)
		{
			fs.Position = e.Offset;

			Stream s = new LimitedStream(fs, e.Offset, e.Stored);

			// If encrypted, decrypt into memory first
			if (e.Encrypted)
			{
				if (key is null || key.Length != 32)
					throw new InvalidOperationException($"Encrypted entry but no 32-byte key provided: {e.Path}");

				var enc = new byte[e.Stored];
				int read = fs.Read(enc, 0, enc.Length);
				if (read != enc.Length) throw new EndOfStreamException();

				const int NONCE_LEN = 12;
				const int TAG_LEN = 16;

				var nonce = enc.AsSpan(0, NONCE_LEN).ToArray();
				var tag = enc.AsSpan(enc.Length - TAG_LEN, TAG_LEN).ToArray();
				var ct = enc.AsSpan(NONCE_LEN, enc.Length - NONCE_LEN - TAG_LEN).ToArray();

				var pt = new byte[ct.Length];
				var aad = BuildAad(e.Path, e.Original, (byte)e.Comp); // ← MATCHES WRITER

				using (var aes = new System.Security.Cryptography.AesGcm(key, TAG_LEN))
					aes.Decrypt(nonce, ct, tag, pt, aad);

				s = new MemoryStream(pt, writable: false);
			}

			// Decompress if needed
			if (e.Comp == CompType.Brotli)
				s = new System.IO.Compression.BrotliStream(s, System.IO.Compression.CompressionMode.Decompress, leaveOpen: false);
			else if (e.Comp == CompType.Deflate)
				s = new System.IO.Compression.DeflateStream(s, System.IO.Compression.CompressionMode.Decompress, leaveOpen: false);

			long total = 0;
			int n;
			while ((n = s.Read(buf, 0, buf.Length)) > 0) total += n;

			if ((e.Comp != CompType.None || e.Encrypted) && total != e.Original)
				throw new InvalidDataException($"Size mismatch {e.Path}: got {total}, expected {e.Original}");
		}

		output.WriteLine("OK");
	}

	// MUST be identical to the writer’s AAD function
	private static byte[] BuildAad(string pathLogical, long originalLen, byte compByte)
	{
		var norm = pathLogical.Replace('\\', '/').TrimStart('/');
		var name = Encoding.UTF8.GetBytes(norm);

		var aad = new byte[name.Length + 8 + 1]; // name || original(LE) || comp
		Buffer.BlockCopy(name, 0, aad, 0, name.Length);
		System.Buffers.Binary.BinaryPrimitives.WriteInt64LittleEndian(aad.AsSpan(name.Length, 8), originalLen);
		aad[name.Length + 8] = compByte;
		return aad;
	}



	// Small slice wrapper for stored bytes
	public sealed class LimitedStream : Stream
	{
		private readonly Stream _base;
		private readonly long _start, _length;
		private long _pos;
		public LimitedStream(Stream b, long start, long length) { _base = b; _start = start; _length = length; _pos = 0; _base.Position = _start; }
		public override bool CanRead => true; public override bool CanSeek => true; public override bool CanWrite => false;
		public override long Length => _length; public override long Position { get => _pos; set => Seek(value, SeekOrigin.Begin); }
		public override int Read(byte[] buffer, int offset, int count) { if (_pos >= _length) return 0; int to = (int)Math.Min(count, _length - _pos); int r = _base.Read(buffer, offset, to); _pos += r; return r; }
		public override long Seek(long offset, SeekOrigin origin) { long t = origin switch { SeekOrigin.Begin => offset, SeekOrigin.Current => _pos + offset, SeekOrigin.End => _length + offset, _ => throw new ArgumentOutOfRangeException() }; if (t < 0 || t > _length) throw new IOException(); _pos = t; _base.Position = _start + _pos; return _pos; }
		public override void Flush() { }
		public override void SetLength(long value) => throw new NotSupportedException(); public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
	}
}