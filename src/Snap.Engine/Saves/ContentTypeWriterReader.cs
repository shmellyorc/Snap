namespace Snap.Engine.Saves;

public sealed class ContentTypeWriterReaderMetadata
{
	public string Checksum { get; internal set; }
	public bool IsCompressed { get; internal set; }
	public DateTime Timestamp { get; internal set; }
	public int Version { get; internal set; }

	internal ContentTypeWriterReaderMetadata() { }

	public static ContentTypeWriterReaderMetadata Create(byte[] data, bool isCompressed, int version = 1)
	{
		return new ContentTypeWriterReaderMetadata
		{
			Checksum = CompueteChecksumHex(data),
			IsCompressed = isCompressed,
			Timestamp = DateTime.UtcNow,
			Version = version
		};
	}

	public static string CompueteChecksumHex(byte[] data)
	{
		byte[] hash = MD5.HashData(data);
		return $"0x{Convert.ToHexString(hash).ToUpper()}";
	}
}

public abstract class ContentTypeWriterReader<T>
{
	private readonly byte[] _magicHaeder = [0x53, 0x4E, 0x41, 0x50]; // SNAP
	private readonly string _encryptionKey;

	public ContentTypeWriterReaderMetadata Metadata { get; internal set; }

	protected ContentTypeWriterReader(string encryptionKey = null)
	{
		_encryptionKey = encryptionKey;
	}

	public abstract void Write(T value, ContentTypeWriter writer);
	public abstract T Read(ContentTypeReader reader);

	public void Save(string filename, T saveFile)
	{
		using MemoryStream ms = new();
		using ContentTypeWriter writer = new(ms);

		// Serialize all data:
		Write(saveFile, writer);
		byte[] rawData = ms.ToArray();

		// Encrpytion
		byte[] encryptionData = EncryptData(rawData);

		// Compresison:
		byte[] compressedData = CompressData(encryptionData);
		bool useCompression = compressedData.Length < encryptionData.Length;
		byte[] finalData = useCompression ? compressedData : encryptionData;

		ContentTypeWriterReaderMetadata metadata =
			ContentTypeWriterReaderMetadata.Create(finalData, useCompression, version: 1);

		using MemoryStream headerStream = new();
		using BinaryWriter headerWriter = new(headerStream);

		headerWriter.Write(_magicHaeder);
		headerWriter.Write(metadata.Checksum);
		headerWriter.Write(metadata.IsCompressed);
		headerWriter.Write(metadata.Timestamp.Ticks);
		headerWriter.Write(metadata.Version);
		headerWriter.Write(finalData);

		File.WriteAllBytes(CreateFinalPath(filename), headerStream.ToArray());
	}

	private string CreateFinalPath(string filename)
	{
		if (Path.IsPathRooted(filename))
			throw new UnauthorizedAccessException("Save filename must be relative.");

		var safeName = filename
			.Replace("../", "")
			.Replace("..\\", "");

		var invalid = Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars()).ToArray();
		safeName = new string(safeName.Where(c => !invalid.Contains(c)).ToArray());

		var baseDir = Game.Instance.ApplicationSaveFolder;
		var combined = Path.Combine(baseDir, safeName);
		var fullPath = Path.GetFullPath(combined);
		var normalizedBase = Path.GetFullPath(baseDir)
								 .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
								 + Path.DirectorySeparatorChar;

		if (!fullPath.StartsWith(normalizedBase, StringComparison.OrdinalIgnoreCase))
		{
			throw new UnauthorizedAccessException(
				$"Invalid save path: '{filename}'. Cannot escape save directory.");
		}

		// Make 100% sure the save folder exists.
		Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

		return fullPath;
	}

	public T Load(string filename)
	{
		byte[] fileData = File.ReadAllBytes(CreateFinalPath(filename));

		using MemoryStream memoryStream = new(fileData);
		using BinaryReader reader = new(memoryStream);

		byte[] magic = reader.ReadBytes(4);
		if (!magic.SequenceEqual(_magicHaeder))
			throw new Exception("Invlid save file");

		string checksum = reader.ReadString();
		bool isCompressed = reader.ReadBoolean();
		long timespamp = reader.ReadInt64();
		int version = reader.ReadInt32();
		byte[] rawData = reader.ReadBytes((int)(memoryStream.Length - memoryStream.Position));

		// Verify Intergity
		string computedChecksum = ContentTypeWriterReaderMetadata.CompueteChecksumHex(rawData);
		if (checksum != computedChecksum)
			throw new Exception("Save file corrupted!");

		// Compress if necessary
		byte[] finalData = isCompressed ? DecompressData(rawData) : rawData;

		// decrypt:
		byte[] decryptedData = DecryptData(finalData);

		using MemoryStream ms = new(decryptedData);
		using ContentTypeReader readerContent = new(ms);

		Metadata = new ContentTypeWriterReaderMetadata
		{
			Checksum = checksum,
			IsCompressed = isCompressed,
			Timestamp = new DateTime(timespamp),
			Version = version
		};

		return Read(readerContent);
	}

	private byte[] EncryptData(byte[] rawData)
	{
		if (string.IsNullOrEmpty(_encryptionKey))
			return rawData;

		using Aes aes = Aes.Create();

		aes.Key = GenerateKey(_encryptionKey);
		aes.GenerateIV();

		using ICryptoTransform encryptor = aes.CreateEncryptor();
		using MemoryStream ms = new();

		ms.Write(aes.IV, 0, aes.IV.Length);

		using var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);

		cryptoStream.Write(rawData, 0, rawData.Length);
		cryptoStream.FlushFinalBlock();

		return ms.ToArray();
	}

	private byte[] DecryptData(byte[] encyptedData)
	{
		if (string.IsNullOrEmpty(_encryptionKey)) return encyptedData;

		using Aes aes = Aes.Create();
		using MemoryStream ms = new(encyptedData);

		byte[] iv = new byte[16];

		ms.Read(iv, 0, iv.Length);

		aes.Key = GenerateKey(_encryptionKey);
		aes.IV = iv;

		using ICryptoTransform decryptor = aes.CreateDecryptor();
		using CryptoStream cryptoStream = new(ms, decryptor, CryptoStreamMode.Read);
		using MemoryStream outputStream = new();

		cryptoStream.CopyTo(outputStream);

		return outputStream.ToArray();
	}

	private byte[] CompressData(byte[] data)
	{
		using MemoryStream output = new();
		using DeflateStream compressionStream = new(output, CompressionLevel.Optimal);

		compressionStream.Write(data, 0, data.Length);
		compressionStream.Flush();

		return output.ToArray();
	}

	private byte[] DecompressData(byte[] data)
	{
		using MemoryStream input = new(data);
		using MemoryStream output = new();
		using DeflateStream decompressStream = new(input, CompressionMode.Decompress);

		decompressStream.CopyTo(output);

		return output.ToArray();
	}

	private static byte[] GenerateKey(string passPhrase) =>
		SHA256.HashData(Encoding.UTF8.GetBytes(passPhrase));
}
