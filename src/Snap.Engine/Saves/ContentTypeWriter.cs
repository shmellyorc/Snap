namespace Snap.Engine.Saves;

public sealed class ContentTypeWriter : BinaryWriter
{
	internal ContentTypeWriter(Stream stream) : base(stream) { }

	public void Write(Vect2 value)
	{
		Write(value.X);
		Write(value.Y);
	}

	public void Write(Rect2 value)
	{
		Write(value.X);
		Write(value.Y);
		Write(value.Width);
		Write(value.Height);
	}

	public void Write(Color value)
	{
		Write(value.R);
		Write(value.G);
		Write(value.B);
		Write(value.A);
	}

	public void WriteObject<T>(T value)
	{
		using var ms = new MemoryStream();
		var serializer = new XmlSerializer(typeof(T));
		serializer.Serialize(ms, value);

		var bytes = ms.ToArray();

		Write(bytes.Length);
		Write(bytes);
	}
}
