using Snap.Systems;

namespace Snap.Assets.LDTKImporter;

public class MapInstance
{
	public Vect2 Location { get; }
	public Vect2 Position { get; }

	internal MapInstance(Vect2 location, Vect2 position)
	{
		Location = location;
		Position = position;
	}
}
