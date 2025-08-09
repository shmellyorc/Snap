namespace Snap.Pack.Core;

public sealed class PackOptions
{
	public required string InputDir { get; init; }
	public required string OutFile { get; init; }
	public bool Overwrite { get; init; } = true;

	// compression prefs
	public bool UseBrotli { get; init; } = true;   // prefer brotli
	public bool UseDeflate { get; init; } = false; // optional alternative
	public double MinSavingsRatio { get; init; } = 0.03; // 3% smaller required to keep compressed

	public bool Encrypt { get; init; }
	public byte[]? Key { get; init; } // 32 bytes if Encrpyted
}
