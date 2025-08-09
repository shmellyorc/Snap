namespace Snap.Engine.Saves;

public sealed class ContentTypeReader : BinaryReader
{
	internal ContentTypeReader(Stream stream) : base(stream) { }

	public Vect2 ReadVect2()
	{
		float x = ReadSingle();
		float y = ReadSingle();
		return new Vect2(x, y);
	}

	public Rect2 ReadRect2()
	{
		float x = ReadSingle();
		float y = ReadSingle();
		float w = ReadSingle();
		float h = ReadSingle();
		return new Rect2(x, y, w, h);
	}

	public Color ReadColor()
	{
		byte r = ReadByte();
		byte g = ReadByte();
		byte b = ReadByte();
		byte a = ReadByte();
		return new Color(r, g, b, a);
	}

	public T ReadObject<T>()
	{
		var length = ReadInt32();
		var bytes = ReadBytes(length);

		using var ms = new MemoryStream(bytes);
		var serializer = new XmlSerializer(typeof(T));
		return (T)serializer.Deserialize(ms);
	}
}
