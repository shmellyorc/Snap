using Snap.Graphics.Atlas;

// --------------------------------------------------------------------------------
// Inner page wrapper: holds one SkylinePacker and one SFTexture
// --------------------------------------------------------------------------------
public class AtlasPage
{
    public int PageIndex { get; }
    public SkylinePacker Packer { get; }
    public SFTexture Texture { get; }
    public long UsedPixels { get; private set; }

    public AtlasPage(int pageSize, int pageIndex)
    {
        PageIndex = pageIndex;
        Packer = new SkylinePacker(pageSize, pageSize);
        Texture = new SFTexture((uint)pageSize, (uint)pageSize);
        UsedPixels = 0;
    }

    /// <summary>
    /// Try to pack srcRect; if successful, blit the pixels and return an AtlasHandle.
    /// </summary>
    public bool TryPack(SFTexture srcTexture, SFRectI srcRect, out AtlasHandle handle)
    {
        var placement = Packer.Insert(srcRect.Width, srcRect.Height);
        if (!placement.HasValue)
        {
            handle = default;
            return false;
        }

        var dst = placement.Value;

        // Copy only the sub-rect via SFImage
        var img = srcTexture.CopyToImage();
        var region = new SFImage((uint)srcRect.Width, (uint)srcRect.Height);
        region.Copy(
            img,
            0, 0,
            new SFRectI(
                (int)srcRect.Left, (int)srcRect.Top,
                (int)srcRect.Width, (int)srcRect.Height
            ),
            false
        );
        Texture.Update(region, (uint)dst.Left, (uint)dst.Top);

        UsedPixels += (long)srcRect.Width * srcRect.Height;

        handle = new AtlasHandle(PageIndex, dst);
        return true;
    }

    /// <summary>
    /// Mark a region free; actual reclamation happens on next Insert.
    /// </summary>
    // public void RemoveLazy(SFRectI rect)
    // {
    // 	Packer.RemoveLazy(rect);
    // }
    public void RemoveLazy(SFRectI rect)
    {
        // no-op under SkylinePacker

        var area = (long)rect.Width * rect.Height;
        UsedPixels = Math.Max(0, UsedPixels - area);  // avoid going negative
    }
}
