namespace GameCube.GX.Texture;

/// <summary>
///     A colour block which is indirectly encoded using a colour-indexing format.
/// </summary>
public record class IndirectBlock
{
    /// <summary>
    ///     
    /// </summary>
    public readonly IndirectEncoding IndirectEncoding;

    /// <summary>
    ///     
    /// </summary>
    public readonly Palette Palette;

    /// <summary>
    ///     This block's indirect colour indexes.
    /// </summary>
    public readonly ushort[] ColorIndexes;


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
