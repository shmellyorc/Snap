namespace Snap.Engine.Inputs;

internal class SdlControllerEntry
{
	public string Guid { get; }
	public string Name { get; }
	public Dictionary<char, int> ButtonMap { get; }

	public SdlControllerEntry(string guid, string name, Dictionary<char, int> map)
	{
		Guid = guid;
		Name = name;
		ButtonMap = map;
	}
}
