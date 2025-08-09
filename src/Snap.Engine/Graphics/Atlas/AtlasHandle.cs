namespace Snap.Engine.Graphics.Atlas;

/// <summary>
/// Represents a handle to a specific region within an atlas page.
/// </summary>
/// <remarks>
/// An atlas handle stores both the page index and the rectangular source
/// area of the texture within that page. It is typically returned by the
/// atlas page manager when an asset is packed into an atlas.
/// </remarks>
public struct AtlasHandle
{
	/// <summary>
	/// The ID of the atlas page where the texture region is stored.
	/// </summary>
	public int PageId;

	/// <summary>
	/// The rectangle defining the texture's position and size within the atlas page.
	/// </summary>
	public SFRectI SourceRect;

	/// <summary>
	/// Initializes a new instance of the <see cref="AtlasHandle"/> struct.
	/// </summary>
	/// <param name="pageId">The ID of the atlas page containing the texture region.</param>
	/// <param name="sourceRect">The rectangle specifying the location and size of the region in the atlas page.</param>
	public AtlasHandle(int pageId, SFRectI sourceRect)
	{
		PageId = pageId;
		SourceRect = sourceRect;
	}
}

