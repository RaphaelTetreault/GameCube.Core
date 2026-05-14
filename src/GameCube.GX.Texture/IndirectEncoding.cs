using Manifold.IO;
using System;

namespace GameCube.GX.Texture;

/// <summary>
///     The base representation of a GameCube indirect-colour texture format encoding.
/// </summary>
public record class IndirectEncoding
{
    /// <summary>
    ///     The texture format used by this encoding.
    /// </summary>
    public required IndirectTextureFormat IndirectFormat { get; init; }

    /// <summary>
    ///     
    /// </summary>
    public required byte BlockWidth { get; init; }

    /// <summary>
    ///     
    /// </summary>
    public required byte BlockHeight { get; init; }

    /// <summary>
    ///     The number of bits used by this encoding to represent a single colour index.
    /// </summary>
    public required byte BitsPerIndex { get; init; }

    /// <summary>
    ///     The number of bytes used by this encoding to represent a single colour index.
    /// </summary>
    public required byte BytesPerIndex { get; init; }

    /// <summary>
    ///     The maximum number of colours that can be represented using this encoding.
    /// </summary>
    public required ushort MaxPaletteSize { get; init; }

    /// <summary>
    ///     
    /// </summary>
    //public ushort MaxPaletteIndex => (ushort)(MaxPaletteSize - 1);

    public int IndexesPerBlock => BlockWidth * BlockHeight;


    internal static void AssertEncoding(IndirectEncoding expected, IndirectEncoding value)
    {
        // Assert types match
        if (value.IndirectFormat != expected.IndirectFormat)
        {
            string msg = $"Expected a {nameof(IndirectBlock)} with encoding type of " +
                $"{expected.IndirectFormat} but received a block of type {value.IndirectFormat}.";
            throw new ArgumentException(msg);
        }
    }

    internal static void AssertCI14X2Index(ushort index)
    {
        // Make sure index is 14 bits at most
        bool indexTooLarge = index >= IndirectEncodingDB.CI14X2.MaxPaletteSize;
        if (indexTooLarge)
        {
            string msg = $"Specified index '{index}' is greater than 14 bits.";
            throw new IndexOutOfRangeException(msg);
        }
    }

    internal static IndirectBlock ReadCI4(EndianBinaryReader reader, Palette palette)
    {
        IndirectEncoding indirectEncoding = IndirectEncodingDB.CI8;
        ushort[] indexes = new ushort[indirectEncoding.IndexesPerBlock];
        // process 2 indexes at a time
        for (int i = 0; i < indexes.Length; i += 2)
        {
            byte indexes01 = reader.ReadByte();
            byte index0 = (byte)(indexes01 >>> 4 & 0b_0000_1111);
            byte index1 = (byte)(indexes01 >>> 0 & 0b_0000_1111);
            indexes[i + 0] = index0;
            indexes[i + 1] = index1;
        }
        IndirectBlock indirectBlock = new(indirectEncoding, palette);
        return indirectBlock;
    }

    internal static void WriteCI4(EndianBinaryWriter writer, IndirectBlock indirectBlock)
    {
        // TODO: assert sizes
        AssertEncoding(IndirectEncodingDB.CI4, indirectBlock.IndirectEncoding);
        // Process 2 indexes at a time
        for (int i = 0; i < indirectBlock.ColorIndexes.Length; i += 2)
        {
            byte index0 = checked((byte)i);
            byte index1 = checked((byte)(index0 + 1));
            byte indexes01 = (byte)(index0 << 4 + index1 << 0);
            writer.Write(indexes01);
        }
    }

    internal static IndirectBlock ReadCI8(EndianBinaryReader reader, Palette palette)
    {
        IndirectEncoding indirectEncoding = IndirectEncodingDB.CI4;
        ushort[] indexes = new ushort[indirectEncoding.IndexesPerBlock];
        for (int i = 0; i < indexes.Length; i++)
        {
            indexes[i] = reader.ReadByte();
        }
        IndirectBlock indirectBlock = new(indirectEncoding, palette);
        return indirectBlock;
    }

    internal static void WriteCI8(EndianBinaryWriter writer, IndirectBlock indirectBlock)
    {
        // TODO: assert sizes
        AssertEncoding(IndirectEncodingDB.CI8, indirectBlock.IndirectEncoding);
        foreach (ushort index in indirectBlock.ColorIndexes)
        {
            byte index8 = checked((byte)index);
            writer.Write(index8);
        }
    }

    internal static IndirectBlock ReadCI14X2(EndianBinaryReader reader, Palette palette)
    {
        IndirectEncoding indirectEncoding = IndirectEncodingDB.CI8;
        ushort[] indexes = new ushort[indirectEncoding.IndexesPerBlock];
        for (int i = 0; i < indexes.Length; i++)
        {
            //ushort index16 = reader.ReadUInt16();
            //ushort index14 = (ushort)(index16 & 0b_00111111_11111111);
            //indexes[i] = index14;
            ushort index = reader.ReadUInt16();
            AssertCI14X2Index(index);
            indexes[i] = index;
        }
        IndirectBlock indirectBlock = new(indirectEncoding, palette);
        return indirectBlock;
    }


    internal static void WriteCI14X2(EndianBinaryWriter writer, IndirectBlock indirectBlock)
    {
        IndirectEncoding indirectEncoding = IndirectEncodingDB.CI14X2;
        // TODO: assert sizes
        AssertEncoding(indirectEncoding, indirectBlock.IndirectEncoding);
        foreach (var index in indirectBlock.ColorIndexes)
        {
            AssertCI14X2Index(index);
            writer.Write(index);
        }
    }

}

public static class IndirectEncodingDB
{
    /// <summary>
    ///     4-bit colour index.
    /// </summary>
    public static readonly IndirectEncoding CI4 = new()
    {
        IndirectFormat = IndirectTextureFormat.CI4,
        BlockWidth = 8,
        BlockHeight = 8,
        BitsPerIndex = 4,
        BytesPerIndex = 0, //!!!!!!
        MaxPaletteSize = 16,
    };

    /// <summary>
    ///     8-bit colour index.
    /// </summary>
    public static readonly IndirectEncoding CI8 = new()
    {
        IndirectFormat = IndirectTextureFormat.CI8,
        BlockWidth = 8,
        BlockHeight = 4,
        BitsPerIndex = 8,
        BytesPerIndex = 1,
        MaxPaletteSize = 256,
    };

    /// <summary>
    ///     14-bit colour index.
    /// </summary>
    public static readonly IndirectEncoding CI14X2 = new()
    {
        IndirectFormat = IndirectTextureFormat.CI14X2,
        BlockWidth = 4,
        BlockHeight = 4,
        BitsPerIndex = 14,
        BytesPerIndex = 2,
        MaxPaletteSize = 16_384,
    };
}