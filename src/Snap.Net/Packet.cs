using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Snap.Net;

namespace SnapNet;

public enum PacketDataType : byte
{
	None,
	Byte,
	Boolean,
	Bytes,
	String,
	Integer,
	Float,
	Double,
	Long,
}

public sealed class Packet : IDisposable
{
	private readonly MemoryStream _stream;
	private readonly BinaryWriter _writer;
	private BinaryReader _reader;
	private int _packetIndex;
	private bool _isReading;
	private readonly List<PacketDataType> _packets = new();
	private byte[] _original, _active;

	public bool IsReading => _isReading;
	public IReadOnlyList<PacketDataType> Packets => _packets;
	public PacketDataType? Peek => _packetIndex < _packets.Count ? _packets[_packetIndex] : (PacketDataType?)null;
	public int RemainingTypes => _packets.Count - _packetIndex;

	public Packet()
	{
		_isReading = false;
		_packetIndex = 0;
		_stream = new MemoryStream();
		_writer = new BinaryWriter(_stream, Encoding.UTF8, leaveOpen: true);
	}

	public Packet(byte[] bytes)
	{
		_isReading = true;
		_packetIndex = 0;
		_original = bytes;
		_active = bytes;

		// fullLength
		var lenBytes = Helpers.TakeFromStart(ref _active, sizeof(int));
		if (lenBytes.Length != 4) throw new InvalidDataException("Bad header: full length.");
		int fullLength = BitConverter.ToInt32(lenBytes, 0);

		// packet count
		var countBytes = Helpers.TakeFromStart(ref _active, sizeof(int));
		if (countBytes.Length != 4) throw new InvalidDataException("Bad header: packet count.");
		int packetSize = BitConverter.ToInt32(countBytes, 0);
		if (packetSize < 0) throw new InvalidDataException("Negative packet count.");

		// packet ids
		var ids = Helpers.TakeFromStart(ref _active, packetSize);
		if (ids.Length != packetSize) throw new InvalidDataException("Truncated packet ids.");

		_packets.Capacity = Math.Max(_packets.Capacity, packetSize);
		for (int i = 0; i < ids.Length; i++)
			_packets.Add((PacketDataType)ids[i]);

		// recompute expected full length
		int computed = sizeof(int) + sizeof(int) + packetSize + _active.Length;
		if (fullLength != computed)
			throw new InvalidDataException("Length field mismatch.");

		// wrap remaining payload without copying
		int dataOffset = bytes.Length - _active.Length;
		_stream = new MemoryStream(bytes, dataOffset, _active.Length, writable: false, publiclyVisible: true);
		_reader = new BinaryReader(_stream, Encoding.UTF8, leaveOpen: true);
		_writer = null; // this instance is read-only
	}

	public void Dispose()
	{
		_reader?.Dispose();
		_writer?.Dispose();
		_stream?.Dispose();
	}

	// ---- Writer helpers (also record types) ----
	public Packet WriteBoolean(bool v) { EnsureWriteMode(); Record(PacketDataType.Boolean); _writer.Write(v); return this; }
	public Packet WriteByte(byte v) { EnsureWriteMode(); Record(PacketDataType.Byte); _writer.Write(v); return this; }
	public Packet WriteBytes(ReadOnlySpan<byte> v)
	{
		EnsureWriteMode();
		Record(PacketDataType.Bytes);
		_writer.Write(v.Length);            // length prefix
		_writer.Write(v);                   // span overload on new .NET

		return this;
	}
	public Packet WriteString(string s) { EnsureWriteMode(); Record(PacketDataType.String); _writer.Write(s ?? string.Empty); return this; }
	public Packet WriteInt(int v) { EnsureWriteMode(); Record(PacketDataType.Integer); _writer.Write(v); return this; }
	public Packet WriteFloat(float v) { EnsureWriteMode(); Record(PacketDataType.Float); _writer.Write(v); return this; }
	public Packet WriteDouble(double v) { EnsureWriteMode(); Record(PacketDataType.Double); _writer.Write(v); return this; }
	public Packet WriteLong(long v) { EnsureWriteMode(); Record(PacketDataType.Long); _writer.Write(v); return this; }


	// ---- Reader helpers (assumes you respect recorded order) ----
	public bool ReadBoolean() { EnsureReadMode(); Expect(PacketDataType.Boolean); return _reader.ReadBoolean(); }
	public byte ReadByte() { EnsureReadMode(); Expect(PacketDataType.Byte); return _reader.ReadByte(); }
	public byte[] ReadBytes()
	{
		EnsureReadMode();
		Expect(PacketDataType.Bytes);
		int len = _reader.ReadInt32();
		return _reader.ReadBytes(len);
	}
	public string ReadString() { EnsureReadMode(); Expect(PacketDataType.String); return _reader.ReadString(); }
	public int ReadInt() { EnsureReadMode(); Expect(PacketDataType.Integer); return _reader.ReadInt32(); }
	public float ReadFloat() { EnsureReadMode(); Expect(PacketDataType.Float); return _reader.ReadSingle(); }
	public double ReadDouble() { EnsureReadMode(); Expect(PacketDataType.Double); return _reader.ReadDouble(); }
	public long ReadLong() { EnsureReadMode(); Expect(PacketDataType.Long); return _reader.ReadInt64(); }


	public byte[] Build()
	{
		if (_writer == null) throw new InvalidOperationException("Packet is in read-only mode.");
		if (_stream.Length == 0) return Array.Empty<byte>();

		_writer.Flush();

		int packetCount = _packets.Count;

		// packet ids
		Span<byte> packetIds = packetCount <= 256 ? stackalloc byte[packetCount] : new byte[packetCount];
		for (int i = 0; i < packetCount; i++) packetIds[i] = (byte)_packets[i];

		// payload buffer view (avoid ToArray)
		if (!_stream.TryGetBuffer(out ArraySegment<byte> seg))
			seg = new ArraySegment<byte>(_stream.ToArray()); // fallback

		const int I32 = sizeof(int);
		int fullLen = I32 + I32 + packetIds.Length + seg.Count;

		byte[] result = new byte[fullLen];
		var dest = result.AsSpan();

		// headers
		BinaryPrimitives.WriteInt32LittleEndian(dest, fullLen);
		dest = dest[I32..];
		BinaryPrimitives.WriteInt32LittleEndian(dest, packetCount);
		dest = dest[I32..];

		// packet ids
		packetIds.CopyTo(dest);
		dest = dest[packetIds.Length..];

		// payload
		seg.AsSpan().CopyTo(dest);

		return result;
	}






	private void EnsureWriteMode()
	{
		if (_isReading) throw new InvalidOperationException("Packet is in read-only mode.");
	}
	private void EnsureReadMode()
	{
		if (!_isReading) throw new InvalidOperationException("Packet is in write-only mode.");
	}

	private void Record(PacketDataType t) => _packets.Add(t);

	private void Expect(PacketDataType expected)
	{
		if (_packetIndex >= _packets.Count)
			throw new InvalidDataException($"Packet underflow at index {_packetIndex}. Expected {expected}.");

		var actual = _packets[_packetIndex++];

		if (actual != expected)
			throw new InvalidDataException($"Type mismatch at index {_packetIndex - 1}. Expected {expected}, got {actual}.");
	}
}