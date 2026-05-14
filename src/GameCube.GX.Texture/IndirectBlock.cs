namespace GameCube.GX.Texture;

/// <summary>
///     A colour block which is indirectly encoded using a colour-indexing format.
/// </summary>
public record class IndirectBlock
{
    /// <summary>
    ///     
    /// </summary>
    public IndirectEncoding IndirectEncoding { get; init; }

    /// <summary>
    ///     
    /// </summary>
    public Palette Palette { get; init; }

    /// <summary>
    ///     This block's indirect colour indexes.
    /// </summary>
    public ushort[] ColorIndexes { get; init; }


    /// <summary>
    ///     Indexer to get/set indirect colour index.
    /// </summary>
    /// <param name="i">The pixel's indirect colour index.</param>
    /// <returns>
    ///     Indirect colour at the specified index within this block.
    /// </returns>
    public ushort this[int i] { get => ColorIndexes[i]; set => ColorIndexes[i] = value; }


    public IndirectBlock(IndirectEncoding indirectEncoding, Palette palette)
    {
        IndirectEncoding = indirectEncoding;
        Palette = palette;
        ColorIndexes = new ushort[indirectEncoding.MaxPaletteSize];
    }
}
