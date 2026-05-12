namespace GameCube.GX.Texture;

/// <summary>
///     A colour block which is directly encoded using a pixel format.
/// </summary>
public record struct DirectBlock
{
    /// <summary>
    ///     The block's width.
    /// </summary>
    public readonly byte Width;

    /// <summary>
    ///     The block's height.
    /// </summary>
    public readonly byte Height;

    /// <summary>
    ///     The block's texture format.
    /// </summary>
    public readonly DirectTextureFormat DirectFormat;

    /// <summary>
    ///     
    /// </summary>
    public readonly DirectEncoding DirectEncoding;

    /// <summary>
    ///     This block's direct colours (pixels).
    /// </summary>
    public TextureColor[] Colors { get; set; }

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
            int index = x + y * Width;
            TextureColor color = Colors[index];
            return color;
        }
        set
        {
            int index = x + y * Width;
            Colors[index] = value;
        }
    }

    /// <summary>
    ///     Construct a new direct colour block.
    /// </summary>
    /// <param name="width">The width of the block.</param>
    /// <param name="height">The height of the block.</param>
    /// <param name="directFormat">The texture format of the block.</param>
    /// <exception cref="System.ArgumentException">
    ///     Thrown if the <paramref name="directFormat"/> is not a direct colour format.
    /// </exception>
    public DirectBlock(byte width, byte height, DirectTextureFormat directFormat)
    {
        directFormat.Validate();

        Width = width;
        Height = height;
        DirectFormat = directFormat;

        int pixelCount = Width * Height;
        Colors = new TextureColor[pixelCount];
    }

    /// <summary>
    ///     Construct a new direct colour block.
    /// </summary>
    /// <param name="directEncoding">The direct encoding to use for this block.</param>
    public DirectBlock(DirectEncoding directEncoding) : this(directEncoding.BlockWidth, directEncoding.BlockHeight, directEncoding.DirectFormat)
    { }

}
