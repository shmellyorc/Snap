using System.IO.Compression;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;

namespace Snap.Saves;

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
		using var md5 = MD5.Create();
		byte[] hash = md5.ComputeHash(data);
		return $"0x{BitConverter.ToString(hash).Replace("-", "").ToUpper()}";
	}
}

public abstract class ContentTypeWriterReader<T>
{
	private readonly byte[] _magicHaeder = { 0x53, 0x4E, 0x41, 0x50 }; // SNAP
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
		using var ms = new MemoryStream();
		using var writer = new ContentTypeWriter(ms);

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

		using var headerStream = new MemoryStream();
		using var headerWriter = new BinaryWriter(headerStream);
		headerWriter.Write(_magicHaeder);
		headerWriter.Write(metadata.Checksum);
		headerWriter.Write(metadata.IsCompressed);
		headerWriter.Write(metadata.Timestamp.Ticks);
		headerWriter.Write(metadata.Version);
		headerWriter.Write(finalData);

		File.WriteAllBytes(filename, headerStream.ToArray());
	}

	public T Load(string filename)
	{
		byte[] fileData = File.ReadAllBytes(filename);

		using var memoryStream = new MemoryStream(fileData);
		using var reader = new BinaryReader(memoryStream);

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
		if (checksum != computedChecksum) throw new Exception("Save file corrupted!");

		// Compress if necessary
		byte[] finalData = isCompressed ? DecompressData(rawData) : rawData;

		// decrypt:
		byte[] decryptedData = DecryptData(finalData);

		using var ms = new MemoryStream(decryptedData);
		using var readerContent = new ContentTypeReader(ms);

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
		if (string.IsNullOrEmpty(_encryptionKey)) return rawData;

		using var aes = Aes.Create();
		aes.Key = GenerateKey(_encryptionKey);
		aes.GenerateIV();

		using var encryptor = aes.CreateEncryptor();
		using var ms = new MemoryStream();
		ms.Write(aes.IV, 0, aes.IV.Length);
		using var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
		cryptoStream.Write(rawData, 0, rawData.Length);
		cryptoStream.FlushFinalBlock();

		return ms.ToArray();
	}

	private byte[] DecryptData(byte[] encyptedData)
	{
		if (string.IsNullOrEmpty(_encryptionKey)) return encyptedData;

		using var aes = Aes.Create();
		using var ms = new MemoryStream(encyptedData);
		byte[] iv = new byte[16];
		ms.Read(iv, 0, iv.Length);
		aes.Key = GenerateKey(_encryptionKey);
		aes.IV = iv;

		using var decryptor = aes.CreateDecryptor();
		using var cryptoStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
		using var outputStream = new MemoryStream();
		cryptoStream.CopyTo(outputStream);

		return outputStream.ToArray();
	}

	private byte[] CompressData(byte[] data)
	{
		using var output = new MemoryStream();
		using var compressionStream = new DeflateStream(output, CompressionLevel.Optimal);
		compressionStream.Write(data, 0, data.Length);
		compressionStream.Flush();
		return output.ToArray();
	}

	private byte[] DecompressData(byte[] data)
	{
		using var input = new MemoryStream(data);
		using var output = new MemoryStream();
		using var decompressStream = new DeflateStream(input, CompressionMode.Decompress);
		decompressStream.CopyTo(output);
		return output.ToArray();
	}

	private static byte[] GenerateKey(string passPhrase)
	{
		using var sha256 = SHA256.Create();
		return sha256.ComputeHash(Encoding.UTF8.GetBytes(passPhrase));
	}
}
