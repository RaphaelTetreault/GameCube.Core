using System;
using System.Collections.Immutable;

namespace GameCube.GX.Texture;

/// <summary>
///     A colour block which is indirectly encoded using a colour-indexing format.
/// </summary>
public record class IndirectBlock
{
    /// <summary>
    ///     The indirect coding used for this indirect block.
    /// </summary>
    public IndirectEncoding IndirectEncoding { get; init; }

    /// <summary>
    ///     This block's indirect colour indexes.
    /// </summary>
    public ImmutableArray<ushort> ColorIndexes { get; init; }


    /// <summary>
    ///     Indexer to get indirect colour index.
    /// </summary>
    /// <param name="i">The pixel's indirect colour index.</param>
    /// <returns>
    ///     Indirect colour at the specified index within this block.
    /// </returns>
    public ushort this[int i]
    {
        get => ColorIndexes[i];
    }

    /// <summary>
    ///     Indexer to get indirect colour index.
    /// </summary>
    /// <param name="x">The horizontal coordinate of the pixel in this block.</param>
    /// <param name="y">The vertical coordinate of the pixel in this block.</param>
    /// <returns>
    ///     Indirect colour at the specified index within this block.
    /// </returns>
    public ushort this[int x, int y]
    {
        get
        {
            int index = x + y * IndirectEncoding.BlockWidth;
            ushort colorIndex = ColorIndexes[index];
            return colorIndex;
        }
    }

    /// <summary>
    ///     
    /// </summary>
    /// <param name="indirectEncoding"></param>
    /// <param name="colorIndexes"></param>
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

    /// <summary>
    ///     
    /// </summary>
    /// <param name="indirectEncoding"></param>
    /// <param name="indexesLength"></param>
    /// <exception cref="ArgumentException">
    ///     
    /// </exception>
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

    /// <summary>
    ///     
    /// </summary>
    /// <param name="encoding"></param>
    /// <param name="indirectBlock"></param>
    /// <exception cref="OverflowException">
    ///     
    /// </exception>
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
