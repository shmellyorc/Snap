namespace Snap.Content.Abstractions.Interfaces;

public interface IContentProvider
{
	bool Exists(string path);
	Stream OpenRead(string path);
	IEnumerable<string> List(string folder);
}
