namespace GameCube.GX.Texture;

/// <summary>
///     A colour block which is indirectly encoded using a colour-indexing format.
/// </summary>
public sealed class IndirectBlock : Block
{
    /// <summary>
    ///     This block's indirect colour indexes.
    /// </summary>
    public ushort[] ColorIndexes { get; private set; }

    /// <summary>
    ///     Indexer to get/set indirect colour index.
    /// </summary>
    /// <param name="i">The pixel's indirect colour index.</param>
    /// <returns>
    ///     Indirect colour at the specified index within this block.
    /// </returns>
    public ushort this[int i] { get => ColorIndexes[i]; set => ColorIndexes[i] = value; }

    /// <summary>
    ///     Indexer to get/set indirect colour index.
    /// </summary>
    /// <param name="x">The horizontal coordinate of the indirect colour index in this block.</param>
    /// <param name="y">The vertical coordinate of the indirect colour index in this block.</param>
    /// <returns>
    ///     Indirect colour index at the specified coordinate within this block.
    /// </returns>
    public ushort this[int x, int y]
    {
        get
        {
            int index = x + y * Width;
            ushort colorIndex = ColorIndexes[index];
            return colorIndex;
        }
        set
        {
            int coordinate = x + y * Width;
            ColorIndexes[coordinate] = value;
        }
    }

    /// <summary>
    ///     Construct a new indirect colour block.
    /// </summary>
    /// <param name="width">The width of the block.</param>
    /// <param name="height">The height of the block.</param>
    /// <param name="indirectFormat">The texture format of the block.</param>
    /// <exception cref="System.ArgumentException">
    ///     Thrown if the <paramref name="indirectFormat"/> is not an indirect colour format.
    /// </exception>
    public IndirectBlock(byte width, byte height, TextureFormat indirectFormat) : base(width, height, indirectFormat)
    {
        int pixelCount = Width * Height;
        ColorIndexes = new ushort[pixelCount];

        switch (indirectFormat)
        {
            // Valid indirect colour formats
            case TextureFormat.CI14X2:
            case TextureFormat.CI4:
            case TextureFormat.CI8:
                break;

            // Everything else is invalid
            default:
                string msg =
                    $"Invalid {nameof(TextureFormat)} '{indirectFormat}'. " +
                    $"The format must be an indirect colour format.";
                throw new System.ArgumentException(msg);
        }
    }

    /// <summary>
    ///     Construct a new direct colour block.
    /// </summary>
    /// <param name="indirectEncoding">The direct encoding to use for this block.</param>
    public IndirectBlock(IndirectEncoding indirectEncoding) : this(indirectEncoding.BlockWidth, indirectEncoding.BlockHeight, indirectEncoding.Format)
    { }
}
