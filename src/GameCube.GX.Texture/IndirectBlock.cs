using System;
using System.Collections.Immutable;

namespace GameCube.GX.Texture;

/// <summary>
///     An indirect color block which indirectly encodes pixel colors through index
///     lookup into a <see cref="Palette"/>.
/// </summary>
public record class IndirectBlock
{
    /// <summary>
    ///     The indirect coding used for this indirect block.
    /// </summary>
    public IndirectEncoding IndirectEncoding { get; init; }

    /// <summary>
    ///     This block's indirect color indexes.
    /// </summary>
    public ImmutableArray<ushort> ColorIndexes { get; init; }


    /// <summary>
    ///     Indexer to get indirect color index.
    /// </summary>
    /// <param name="i">The pixel's indirect color index.</param>
    /// <returns>
    ///     Indirect color index at the specified index within this block.
    /// </returns>
    public ushort this[int i]
    {
        get => ColorIndexes[i];
    }

    /// <summary>
    ///     Indexer to get indirect color index.
    /// </summary>
    /// <param name="x">The horizontal coordinate of the pixel in this block.</param>
    /// <param name="y">The vertical coordinate of the pixel in this block.</param>
    /// <returns>
    ///     Indirect color index at the specified index within this block.
    /// </returns>
    public ushort this[int x, int y]
    {
        get
        {
            int index = x + y * IndirectEncoding.BlockPixelWidth;
            ushort colorIndex = ColorIndexes[index];
            return colorIndex;
        }
    }

    /// <param name="indirectEncoding">The <see cref="GX.Texture.IndirectEncoding"/> to use for this block.</param>
    /// <param name="colorIndexes">The u16 color indexes to use for this block.</param>
    public IndirectBlock(IndirectEncoding indirectEncoding, ushort[] colorIndexes)
    {
        // Validate format
        indirectEncoding.IndirectFormat.Validate();
        IndirectEncoding = indirectEncoding;
        // Validate indexes
        AssertIndexCount(indirectEncoding, colorIndexes.Length);
        AssertIndexValues(indirectEncoding, colorIndexes);
        ColorIndexes = ImmutableArray.Create(colorIndexes);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    internal static void AssertIndexCount(IndirectEncoding indirectEncoding, int indexesLength)
    {
        // Assert index count
        if (indexesLength != indirectEncoding.IndexesPerBlock)
        {
            string msg = $"{nameof(IndirectBlock)} encoding of {indirectEncoding.IndirectFormat} " +
                $"defines {indirectEncoding.IndexesPerBlock} indexes but an array of {indexesLength}.";
            throw new ArgumentException(msg);
        }
    }

    [System.Diagnostics.Conditional("DEBUG")]
    internal static void AssertIndexValues(IndirectEncoding encoding, ReadOnlySpan<ushort> colorIndexes)
    {
        // Assert all indexes in block are within acceatable range
        ushort maxPaletteIndex = encoding.MaxPaletteIndex;
        for (int i = 0; i < colorIndexes.Length; i++)
        {
            ushort index = colorIndexes[i];
            if (index > maxPaletteIndex)
            {
                string msg = $"Index {i}/{colorIndexes.Length} value {index} " +
                    $"is greater than {encoding.IndirectFormat} max index {maxPaletteIndex}.";
                throw new OverflowException(msg);
            }
        }
    }

}
