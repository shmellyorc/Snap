using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Snap.Pack.Core;

public static class PackBuilder
{
	// [Magic: 8 bytes] "SNAPPAK"
	// [Version: int16] = 2
	// [Reserved: int16] = 0
	// [FileCount: int32]

	// repeat for each file:
	//     [PathLen: int16]
	//     [Path: utf8]
	//     [Offset: int64]     // data start
	//     [Stored: int64]     // bytes stored in archive (compressed or raw)
	//     [Original: int64]   // original file size before compression
	//     [Comp: byte]        // 0=None, 1=Brotli, 2=Deflate
	public static void Build(PackOptions opts)
	{
		var root = Path.GetFullPath(opts.InputDir);
		if (!Directory.Exists(root)) throw new DirectoryNotFoundException(root);

		// FULL path to the output file we are writing
		var outFull = Path.GetFullPath(opts.OutFile);

		// Collect input files BUT skip any .spack files and skip the output file itself
		var files = Directory.GetFiles(root, "*", SearchOption.AllDirectories)
			.Where(p =>
			{
				var full = Path.GetFullPath(p);
				return !full.EndsWith(".spack", StringComparison.OrdinalIgnoreCase)
					   && !string.Equals(full, outFull, StringComparison.OrdinalIgnoreCase);
			})
			.OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
			.ToArray();

		Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(opts.OutFile))!);
		var mode = opts.Overwrite ? FileMode.Create : FileMode.CreateNew;

		using var fs = new FileStream(opts.OutFile, mode, FileAccess.ReadWrite, FileShare.None, 1 << 20);
		using var bw = new BinaryWriter(fs, Encoding.UTF8, leaveOpen: true);

		// HEADER (v3)
		bw.Write(Encoding.ASCII.GetBytes("SNAPPAK"));
		if ("SNAPPAK".Length < 8) bw.Write(new byte[8 - "SNAPPAK".Length]);
		bw.Write((short)3); // version
		bw.Write((short)0); // reserved
		bw.Write(files.Length);

		// Precompute path bytes + TOC size
		var rels = new string[files.Length];
		var pathBytes = new byte[files.Length][];
		long tocSize = 0;
		for (int i = 0; i < files.Length; i++)
		{
			var rel = Path.GetRelativePath(root, files[i]).Replace('\\', '/');
			rels[i] = rel;
			var pb = Encoding.UTF8.GetBytes(rel);
			pathBytes[i] = pb;
			tocSize += 2 + pb.Length + 8 + 8 + 8 + 1 + 1; // pathLen + path + offset + stored + original + comp + enc
		}

		long dataOffset = 8 + 2 + 2 + 4 + tocSize;

		// Prepare payloads (compress, then optionally encrypt)
		var payloads = new byte[files.Length][];
		var entries = new (long stored, long original, CompType comp, bool enc)[files.Length];

		for (int i = 0; i < files.Length; i++)
		{
			var raw = File.ReadAllBytes(files[i]);
			long original = raw.LongLength;

			// choose compression
			CompType comp = CompType.None;
			byte[] payload = raw;

			if (opts.UseBrotli || opts.UseDeflate)
			{
				// try preferred
				if (opts.UseBrotli)
				{
					var p1 = TryCompress(raw, CompType.Brotli);
					bool keep = p1.LongLength < original && (original - p1.LongLength) >= original * opts.MinSavingsRatio;
					if (keep) { payload = p1; comp = CompType.Brotli; }
					else if (opts.UseDeflate)
					{
						var p2 = TryCompress(raw, CompType.Deflate);
						keep = p2.LongLength < original && (original - p2.LongLength) >= original * opts.MinSavingsRatio;
						if (keep) { payload = p2; comp = CompType.Deflate; }
					}
				}
				else // deflate preferred
				{
					var p1 = TryCompress(raw, CompType.Deflate);
					bool keep = p1.LongLength < original && (original - p1.LongLength) >= original * opts.MinSavingsRatio;
					if (keep) { payload = p1; comp = CompType.Deflate; }
					else if (opts.UseBrotli)
					{
						var p2 = TryCompress(raw, CompType.Brotli);
						keep = p2.LongLength < original && (original - p2.LongLength) >= original * opts.MinSavingsRatio;
						if (keep) { payload = p2; comp = CompType.Brotli; }
					}
				}
			}

			bool enc = false;
			if (opts.Encrypt)
			{
				if (opts.Key is null || opts.Key.Length != 32)
					throw new ArgumentException("Encrypt=true requires 32-byte Key (AES-256).");

				// NONCE(12) + CIPHERTEXT + TAG(16)  ← we keep this exact layout
				const int NONCE_LEN = 12;
				const int TAG_LEN = 16;

				var nonce = RandomNumberGenerator.GetBytes(NONCE_LEN);
				var cipher = new byte[payload.Length];
				var tag = new byte[TAG_LEN];

				// AAD must match the reader: path (logical, UTF8), original length (LE), comp byte
				var aad = BuildAad(rels[i], original, (byte)comp);

				using (var aes = new AesGcm(opts.Key, TAG_LEN))
					aes.Encrypt(nonce, payload, cipher, tag, aad);

				var outBuf = new byte[NONCE_LEN + cipher.Length + TAG_LEN];
				Buffer.BlockCopy(nonce, 0, outBuf, 0, NONCE_LEN);
				Buffer.BlockCopy(cipher, 0, outBuf, NONCE_LEN, cipher.Length);
				Buffer.BlockCopy(tag, 0, outBuf, NONCE_LEN + cipher.Length, TAG_LEN);

				payload = outBuf;
				enc = true;
			}

			payloads[i] = payload;
			entries[i] = (payload.LongLength, original, comp, enc);
		}

		// Write TOC with computed offsets
		long cur = dataOffset;
		for (int i = 0; i < files.Length; i++)
		{
			bw.Write((short)pathBytes[i].Length);
			bw.Write(pathBytes[i]);

			bw.Write(cur);                        // Offset
			bw.Write(entries[i].stored);          // Stored
			bw.Write(entries[i].original);        // Original
			bw.Write((byte)entries[i].comp);      // Comp
			bw.Write((byte)(entries[i].enc ? 1 : 0)); // Enc

			cur += entries[i].stored;
		}

		// Write payloads
		for (int i = 0; i < files.Length; i++)
			bw.Write(payloads[i]);
	}

	// helper
	private static byte[] TryCompress(byte[] data, CompType which)
	{
		using var ms = new MemoryStream();
		Stream z = which == CompType.Brotli
			? new BrotliStream(ms, CompressionLevel.Optimal, leaveOpen: true)
			: new DeflateStream(ms, CompressionLevel.Optimal, leaveOpen: true);
		z.Write(data, 0, data.Length);
		z.Dispose();
		return ms.ToArray();
	}


	public static void List(string pakPath, TextWriter output)
	{
		var (ver, entries) = PackReader.ReadIndex(pakPath);
		output.WriteLine($"{pakPath} : {entries.Count} files (v{ver})");
		foreach (var e in entries)
			output.WriteLine($"{e.Original,10}  {e.Path}  [{e.Comp}]");
	}

	public static void ExtractAll(string pakPath, string outDir)
	{
		var (ver, entries) = PackReader.ReadIndex(pakPath);
		Directory.CreateDirectory(outDir);

		using var fs = File.OpenRead(pakPath);
		byte[] buf = new byte[1 << 20];

		foreach (var e in entries)
		{
			var dest = Path.Combine(outDir, e.Path.Replace('/', Path.DirectorySeparatorChar));
			Directory.CreateDirectory(Path.GetDirectoryName(dest)!);

			fs.Position = e.Offset;
			using var slice = new Snap.Pack.Core.PackReader.LimitedStream(fs, e.Offset, e.Stored);

			Stream s = e.Comp switch
			{
				CompType.Brotli => new System.IO.Compression.BrotliStream(slice, System.IO.Compression.CompressionMode.Decompress, leaveOpen: false),
				CompType.Deflate => new System.IO.Compression.DeflateStream(slice, System.IO.Compression.CompressionMode.Decompress, leaveOpen: false),
				_ => slice
			};

			using var fw = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.None, buf.Length);
			int read;
			while ((read = s.Read(buf, 0, buf.Length)) > 0)
				fw.Write(buf, 0, read);
		}
	}

	private static byte[] BuildAad(string pathLogical, long originalLen, byte compByte)
	{
		// Must match how the TOC stores the path
		var norm = pathLogical.Replace('\\', '/').TrimStart('/');
		var name = Encoding.UTF8.GetBytes(norm);

		var aad = new byte[name.Length + 8 + 1]; // name || originalLen(LE) || comp
		Buffer.BlockCopy(name, 0, aad, 0, name.Length);
		BinaryPrimitives.WriteInt64LittleEndian(aad.AsSpan(name.Length, 8), originalLen);
		aad[name.Length + 8] = compByte;
		return aad;
	}
}
