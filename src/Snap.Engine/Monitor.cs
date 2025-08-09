namespace Snap.Engine;

public readonly struct Monitor
{
	public int Width { get; }
	public int Height { get; }
	public float Ratio { get; }

	public Monitor(int width, int height)
	{
		Width = width;
		Height = height;
		Ratio = (float)width / height;
	}
}