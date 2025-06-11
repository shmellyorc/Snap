namespace Snap.Assets.Loaders;

public interface IAsset
{
	uint Id { get; }
	string Tag { get; }
	bool IsValid { get; }
	public uint Handle { get; }
	ulong Load();
	void Unload();
	void Dispose();
}
