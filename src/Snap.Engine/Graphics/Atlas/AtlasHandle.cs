namespace Snap.Engine.Graphics.Atlas;

public struct AtlasHandle
{
	public int PageId;
	public SFRectI SourceRect;
	public AtlasHandle(int pageId, SFRectI sourceRect)
	{
		PageId = pageId;
		SourceRect = sourceRect;
	}
}
