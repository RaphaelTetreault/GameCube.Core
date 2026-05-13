namespace GameCube.GX.Texture;

/// <summary>
///     A colour block which is directly encoded using a pixel format.
/// </summary>
public record class DirectBlock
{
    /// <summary>
    ///     The block's encoding.
    /// </summary>
    public readonly DirectEncoding DirectEncoding;

    /// <summary>
    ///     This block's direct colours (pixels).
    /// </summary>
    public readonly TextureColor[] Colors;

    /// <summary>
    ///     Indexer to get/set direct colour (pixel).
    /// </summary>
    /// <param name="i">The pixel's direct colour index in this block.</param>
    /// <returns>
    ///     Direct colour (pixel) at the specified index within this block.
    /// </returns>
    public TextureColor this[int i] 
    { 
        get => Colors[i];
        set => Colors[i] = value; 
    }

    /// <summary>
    ///     Indexer to get/set direct colour (pixel).
    /// </summary>
    /// <param name="x">The horizontal coordinate of the pixel in this block.</param>
    /// <param name="y">The vertical coordinate of the pixel in this block.</param>
    /// <returns>
    ///     Direct colour (pixel) at the specified coordinate within this block.
    /// </returns>
    public TextureColor this[int x, int y]
    { 
        get
        {
            int index = x + y * DirectEncoding.Width;
            TextureColor color = Colors[index];
            return color;
        }
        set
        {
            int index = x + y * DirectEncoding.Width;
            Colors[index] = value;
        }
    }

    /// <summary>
    ///     Construct a new direct colour block.
    /// </summary>
    /// <param name="directEncoding">The direct encoding to use for this block.</param>
    public DirectBlock(DirectEncoding directEncoding, TextureColor[] pixels)
    {
        // Validate and assign encoding
        directEncoding.DirectFormat.Validate();
        DirectEncoding = directEncoding;

        // Make sure pixels map to encoding
        if (pixels.Length != directEncoding.PixelsPerBlock)
        {
            string msg = $"{nameof(DirectBlock)} encoding of {directEncoding.DirectFormat} " +
                $"defines {directEncoding.PixelsPerBlock} pixels but an array of {pixels.Length} " +
                $"{nameof(TextureColor)} was passed to be assigned.";
            throw new System.ArgumentException(msg);
        }
        Colors = pixels;
    }
}
