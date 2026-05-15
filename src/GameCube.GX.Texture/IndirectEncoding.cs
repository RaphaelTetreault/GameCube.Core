using Manifold.IO;
using System;
using System.Collections.Immutable;

namespace GameCube.GX.Texture;

/// <summary>
///     The base representation of a GameCube indirect-colour texture format encoding.
/// </summary>
public record class IndirectEncoding : IEncoding
{
    public delegate IndirectBlock ReadIndirectBlock(EndianBinaryReader reader);
    public delegate void WriteIndirectBlock(EndianBinaryWriter writer, IndirectBlock indirectBlock);

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
    ///     The number of bytes used by this encoding per block.
    /// </summary>
    public required byte BytesPerBlock { get; init; }

    /// <summary>
    ///     The maximum number of colours that can be represented using this encoding.
    /// </summary>
    public required ushort MaxPaletteSize { get; init; }

    public required ReadIndirectBlock ReadBlock { get; init; }
    public required WriteIndirectBlock WriteBlock { get; init; }


    /// <summary>
    ///     
    /// </summary>
    //public ushort MaxPaletteIndex => (ushort)(MaxPaletteSize - 1);

    public int IndexesPerBlock => BlockWidth * BlockHeight;


    public IndirectBlock[] ReadBlocks(EndianBinaryReader reader, int blocksCount)
    {
        IndirectBlock[] directBlocks = new IndirectBlock[blocksCount];
        for (int i = 0; i < blocksCount; i++)
            directBlocks[i] = ReadBlock.Invoke(reader);
        return directBlocks;
    }

    public IndirectBlock[] ReadBlocks(EndianBinaryReader reader, int pxWidth, int pxHeight)
    {
        BlocksInfo blocks = BlocksInfo.FromPixelDimensions(pxWidth, pxHeight, this);
        IndirectBlock[] indirectBlocks = ReadBlocks(reader, blocks.Count);
        return indirectBlocks;
    }





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
        bool indexTooLarge = index >= CI14X2.MaxPaletteSize;
        if (indexTooLarge)
        {
            string msg = $"Specified index '{index}' is greater than 14 bits.";
            throw new IndexOutOfRangeException(msg);
        }
    }

    internal static IndirectBlock ReadCI4(EndianBinaryReader reader)//, Palette palette)
    {
        IndirectEncoding indirectEncoding = CI8;
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
        IndirectBlock indirectBlock = new(indirectEncoding, indexes);
        return indirectBlock;
    }

    internal static void WriteCI4(EndianBinaryWriter writer, IndirectBlock indirectBlock)
    {
        // TODO: assert sizes
        AssertEncoding(CI4, indirectBlock.IndirectEncoding);
        // Process 2 indexes at a time
        for (int i = 0; i < indirectBlock.ColorIndexes.Length; i += 2)
        {
            byte index0 = checked((byte)i);
            byte index1 = checked((byte)(index0 + 1));
            byte indexes01 = (byte)(index0 << 4 + index1 << 0);
            writer.Write(indexes01);
        }
    }

    internal static IndirectBlock ReadCI8(EndianBinaryReader reader)//, Palette palette)
    {
        IndirectEncoding indirectEncoding = CI4;
        ushort[] indexes = new ushort[indirectEncoding.IndexesPerBlock];
        for (int i = 0; i < indexes.Length; i++)
        {
            indexes[i] = reader.ReadByte();
        }
        IndirectBlock indirectBlock = new(indirectEncoding, indexes);
        return indirectBlock;
    }

    internal static void WriteCI8(EndianBinaryWriter writer, IndirectBlock indirectBlock)
    {
        // TODO: assert sizes
        AssertEncoding(CI8, indirectBlock.IndirectEncoding);
        foreach (ushort index in indirectBlock.ColorIndexes)
        {
            byte index8 = checked((byte)index);
            writer.Write(index8);
        }
    }

    internal static IndirectBlock ReadCI14X2(EndianBinaryReader reader)//, Palette palette)
    {
        IndirectEncoding indirectEncoding = CI8;
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
        IndirectBlock indirectBlock = new(indirectEncoding, indexes);
        return indirectBlock;
    }


    internal static void WriteCI14X2(EndianBinaryWriter writer, IndirectBlock indirectBlock)
    {
        IndirectEncoding indirectEncoding = CI14X2;
        // TODO: assert sizes
        AssertEncoding(indirectEncoding, indirectBlock.IndirectEncoding);
        foreach (var index in indirectBlock.ColorIndexes)
        {
            AssertCI14X2Index(index);
            writer.Write(index);
        }
    }



    /// <summary>
    ///     4-bit colour index.
    /// </summary>
    public static readonly IndirectEncoding CI4 = new()
    {
        IndirectFormat = IndirectTextureFormat.CI4,
        BlockWidth = 8,
        BlockHeight = 8,
        BitsPerIndex = 4,
        BytesPerBlock = 32, // 8 * 8 * 0.5(4bpp)
        MaxPaletteSize = 16,
        ReadBlock = ReadCI4,
        WriteBlock = WriteCI4,
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
        BytesPerBlock = 32, // 8 * 4 * 1(8bpp)
        MaxPaletteSize = 256,
        ReadBlock = ReadCI8,
        WriteBlock = WriteCI8,
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
        BytesPerBlock = 32, // 4 * 4 * 2(14bpp)
        MaxPaletteSize = 16_384,
        ReadBlock = ReadCI14X2,
        WriteBlock = WriteCI14X2,
    };

    public static readonly ImmutableDictionary<IndirectTextureFormat, IndirectEncoding> MapFormatToEncoding =
    [
        new(IndirectTextureFormat.CI4, CI4),
        new(IndirectTextureFormat.CI8, CI8),
        new(IndirectTextureFormat.CI14X2, CI14X2),
    ];

}