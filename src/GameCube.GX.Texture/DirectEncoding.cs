using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using CommunityToolkit.HighPerformance;
using Manifold.IO;
using System;
using System.Collections.Immutable;

namespace GameCube.GX.Texture;

/// <summary>
///     The base representation of a GameCube direct-colour texture format encoding.
/// </summary>
public record class DirectEncoding : IEncoding
{
    public delegate DirectBlock ReadDirectBlock(EndianBinaryReader reader);
    public delegate void WriteDirectBlock(EndianBinaryWriter writer, DirectBlock directBlock);

    /// <summary>
    ///     BC1 encoder for CMPR format.
    /// </summary>
    /// <remarks>
    ///     BC1, CMPR, and DXT1 are effectively all the same.
    /// </remarks>
    private static readonly BcEncoder BC1Encoder = new(CompressionFormat.Bc1);

    /// <summary>
    ///     BC1 encoder quality.
    /// </summary>
    public static CompressionQuality BC1CompressionQuality
    {
        get => field;
        set
        {
            field = value;
            BC1Encoder.OutputOptions.Quality = value;
        }
    } = CompressionQuality.BestQuality;


    /// <summary>
    ///     The texture format used by this encoding.
    /// </summary>
    public required DirectTextureFormat DirectFormat { get; init; }

    /// <summary>
    ///     The pixel width of a block for this encoding.
    /// </summary>
    public required byte BlockWidth { get; init; }

    /// <summary>
    ///     The pixel height of a block for this encoding.
    /// </summary>
    public required byte BlockHeight { get; init; }

    /// <summary>
    ///     The number of bits used by this encoding to represent a single colour.
    /// </summary>
    public required byte BitsPerPixel { get; init; }

    /// <summary>
    ///     The number of bytes used by this encoding per block.
    /// </summary>
    public required byte BytesPerBlock { get; init; }

    /// <summary>
    ///     
    /// </summary>
    public required ReadDirectBlock ReadBlock { get; init; }

    /// <summary>
    ///     
    /// </summary>
    public required WriteDirectBlock WriteBlock { get; init; }


    /// <summary>
    ///     Get <see cref="DirectFormat"/> as <see cref="TextureFormat"/>.
    /// </summary>
    public TextureFormat Format => (TextureFormat)DirectFormat;

    /// <summary>
    ///     The pixel width of a block for this encoding.
    /// </summary>
    public int PixelsPerBlock => BlockWidth * BlockHeight;



    public DirectBlock[] ReadBlocks(EndianBinaryReader reader, int blocksCount)
    {
        DirectBlock[] directBlocks = new DirectBlock[blocksCount];
        for (int i = 0; i < blocksCount; i++)
            directBlocks[i] = ReadBlock.Invoke(reader);
        return directBlocks;
    }
    public DirectBlock[] ReadBlocks(EndianBinaryReader reader, int pxWidth, int pxHeight)
    {
        BlocksInfo blocks = BlocksInfo.FromPixelDimensions(pxWidth, pxHeight, this);
        DirectBlock[] directBlocks = ReadBlocks(reader, blocks.BlockCount);
        return directBlocks;
    }



    internal static void AssertEncoding(DirectEncoding expected, DirectEncoding value)
    {
        // Assert types match
        if (value.DirectFormat != expected.DirectFormat)
        {
            string msg = $"Expected a {nameof(DirectBlock)} with encoding type of " +
                $"{expected.DirectFormat} but received a block of type {value.DirectFormat}.";
            throw new ArgumentException(msg);
        }
    }

    internal static DirectBlock ReadI4(EndianBinaryReader reader)
    {
        DirectEncoding DirectEncoding = I4;
        TextureColor[] pixels = new TextureColor[DirectEncoding.PixelsPerBlock];
        for (int y = 0; y < DirectEncoding.BlockHeight; y++)
        {
            // Process 2 pixels per pass, high and low nybbles
            for (int x = 0; x < DirectEncoding.BlockWidth; x += 2)
            {
                // Get 2 colors at a time from 1 byte
                byte nybbles = reader.ReadByte();
                (TextureColor color0, TextureColor color1) = TextureColor.FromI4(nybbles);
                // Assign colors
                int index0 = x + (y * DirectEncoding.BlockWidth);
                int index1 = index0 + 1;
                pixels[index0] = color0;
                pixels[index1] = color1;
            }
        }
        DirectBlock directBlock = new(DirectEncoding, pixels);
        return directBlock;
    }

    internal static void WriteI4(EndianBinaryWriter writer, DirectBlock directBlock)
    {
        DirectEncoding DirectEncoding = I4;
        AssertEncoding(DirectEncoding, directBlock.DirectEncoding);
        for (int y = 0; y < DirectEncoding.BlockHeight; y++)
        {
            // Process 2 pixels per pass, set as high and low nybbles
            for (int x = 0; x < DirectEncoding.BlockWidth; x += 2)
            {
                int index0 = x + (y * DirectEncoding.BlockWidth);
                int index1 = index0 + 1;
                TextureColor intensity0 = directBlock[index0];
                TextureColor intensity1 = directBlock[index1];
                byte intensity01 = TextureColor.ToI4(intensity0, intensity1);
                writer.Write(intensity01);
            }
        }
    }

    internal static DirectBlock ReadI8(EndianBinaryReader reader)
    {
        DirectEncoding DirectEncoding = I8;
        TextureColor[] pixels = new TextureColor[DirectEncoding.PixelsPerBlock];
        for (int y = 0; y < DirectEncoding.BlockHeight; y++)
        {
            for (int x = 0; x < DirectEncoding.BlockWidth; x++)
            {
                byte i8 = reader.ReadByte();
                var color = new TextureColor(i8);
                int index = x + (y * DirectEncoding.BlockWidth);
                pixels[index] = color;
            }
        }
        DirectBlock directBlock = new(DirectEncoding, pixels);
        return directBlock;
    }

    internal static void WriteI8(EndianBinaryWriter writer, DirectBlock directBlock)
    {
        DirectEncoding DirectEncoding = I8;
        AssertEncoding(DirectEncoding, directBlock.DirectEncoding);
        for (int y = 0; y < DirectEncoding.BlockHeight; y++)
        {
            for (int x = 0; x < DirectEncoding.BlockWidth; x++)
            {
                int index = x + (y * DirectEncoding.BlockWidth);
                var color = directBlock[index];
                byte i8 = color.GetIntensity();
                writer.Write(i8);
            }
        }
    }

    internal static DirectBlock ReadIA4(EndianBinaryReader reader)
    {
        DirectEncoding DirectEncoding = IA4;
        TextureColor[] pixels = new TextureColor[DirectEncoding.PixelsPerBlock];
        for (int y = 0; y < DirectEncoding.BlockHeight; y++)
        {
            for (int x = 0; x < DirectEncoding.BlockWidth; x++)
            {
                byte ia4 = reader.ReadByte();
                var color = TextureColor.FromIA4(ia4);
                int index = x + (y * DirectEncoding.BlockWidth);
                pixels[index] = color;
            }
        }
        DirectBlock directBlock = new(DirectEncoding, pixels);
        return directBlock;
    }

    internal static void WriteIA4(EndianBinaryWriter writer, DirectBlock directBlock)
    {
        DirectEncoding DirectEncoding = IA4;
        AssertEncoding(DirectEncoding, directBlock.DirectEncoding);
        for (int y = 0; y < DirectEncoding.BlockHeight; y++)
        {
            for (int x = 0; x < DirectEncoding.BlockWidth; x++)
            {
                int index = x + (y * DirectEncoding.BlockWidth);
                var color = directBlock[index];
                byte ia4 = TextureColor.ToIA4(color);
                writer.Write(ia4);
            }
        }
    }

    internal static DirectBlock ReadIA8(EndianBinaryReader reader)
    {
        DirectEncoding DirectEncoding = IA8;
        TextureColor[] pixels = new TextureColor[DirectEncoding.PixelsPerBlock];
        for (int y = 0; y < DirectEncoding.BlockHeight; y++)
        {
            for (int x = 0; x < DirectEncoding.BlockWidth; x++)
            {
                ushort ia8 = reader.ReadUInt16();
                var color = TextureColor.FromIA8(ia8);
                int index = x + (y * DirectEncoding.BlockWidth);
                pixels[index] = color;
            }
        }
        DirectBlock directBlock = new(DirectEncoding, pixels);
        return directBlock;
    }

    internal static void WriteIA8(EndianBinaryWriter writer, DirectBlock directBlock)
    {
        DirectEncoding DirectEncoding = IA8;
        AssertEncoding(DirectEncoding, directBlock.DirectEncoding);
        for (int y = 0; y < DirectEncoding.BlockHeight; y++)
        {
            for (int x = 0; x < DirectEncoding.BlockWidth; x++)
            {
                int index = x + (y * DirectEncoding.BlockWidth);
                var color = directBlock[index];
                ushort ia8 = TextureColor.ToIA8(color);
                writer.Write(ia8);
            }
        }
    }

    internal static DirectBlock ReadRGB565(EndianBinaryReader reader)
    {
        DirectEncoding DirectEncoding = RGB565;
        TextureColor[] pixels = new TextureColor[DirectEncoding.PixelsPerBlock];
        for (int y = 0; y < DirectEncoding.BlockHeight; y++)
        {
            for (int x = 0; x < DirectEncoding.BlockWidth; x++)
            {
                ushort rgb565 = reader.ReadUInt16();
                var color = TextureColor.FromRGB565(rgb565);
                int index = x + (y * DirectEncoding.BlockWidth);
                pixels[index] = color;
            }
        }
        DirectBlock directBlock = new(DirectEncoding, pixels);
        return directBlock;
    }

    internal static void WriteRGB565(EndianBinaryWriter writer, DirectBlock directBlock)
    {
        DirectEncoding DirectEncoding = RGB565;
        AssertEncoding(DirectEncoding, directBlock.DirectEncoding);
        for (int y = 0; y < DirectEncoding.BlockHeight; y++)
        {
            for (int x = 0; x < DirectEncoding.BlockWidth; x++)
            {
                int index = x + (y * DirectEncoding.BlockWidth);
                var color = directBlock[index];
                ushort rgb565 = TextureColor.ToRGB565(color);
                writer.Write(rgb565);
            }
        }
    }

    internal static DirectBlock ReadRGB5A3(EndianBinaryReader reader)
    {
        DirectEncoding DirectEncoding = RGB5A3;
        TextureColor[] pixels = new TextureColor[DirectEncoding.PixelsPerBlock];
        for (int y = 0; y < DirectEncoding.BlockHeight; y++)
        {
            for (int x = 0; x < DirectEncoding.BlockWidth; x++)
            {
                ushort rgb5a3 = reader.ReadUInt16();
                var color = TextureColor.FromRGB5A3(rgb5a3);
                int index = x + (y * DirectEncoding.BlockWidth);
                pixels[index] = color;
            }
        }
        DirectBlock directBlock = new(DirectEncoding, pixels);
        return directBlock;
    }

    internal static void WriteRGB5A3(EndianBinaryWriter writer, DirectBlock directBlock)
    {
        DirectEncoding DirectEncoding = RGB5A3;
        AssertEncoding(DirectEncoding, directBlock.DirectEncoding);
        for (int y = 0; y < DirectEncoding.BlockHeight; y++)
        {
            for (int x = 0; x < DirectEncoding.BlockWidth; x++)
            {
                int index = x + (y * DirectEncoding.BlockWidth);
                var color = directBlock[index];
                ushort rgb5a3 = TextureColor.ToRGB5A3(color);
                writer.Write(rgb5a3);
            }
        }
    }

    internal static DirectBlock ReadRGBA8(EndianBinaryReader reader)
    {
        DirectEncoding DirectEncoding = RGBA8;
        TextureColor[] pixels = new TextureColor[DirectEncoding.PixelsPerBlock];
        var bytes = reader.ReadBytes(DirectEncoding.BytesPerBlock);
        var a = ExtractRGBA8Bytes(bytes, 33);
        var r = ExtractRGBA8Bytes(bytes, 32);
        var g = ExtractRGBA8Bytes(bytes, 1);
        var b = ExtractRGBA8Bytes(bytes, 0);
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new TextureColor(r[i], g[i], b[i], a[i]);
        DirectBlock directBlock = new(DirectEncoding, pixels);
        return directBlock;
    }

    internal static void WriteRGBA8(EndianBinaryWriter writer, DirectBlock directBlock)
    {
        DirectEncoding DirectEncoding = RGBA8;
        AssertEncoding(DirectEncoding, directBlock.DirectEncoding);
        var a = new byte[DirectEncoding.PixelsPerBlock];
        var r = new byte[DirectEncoding.PixelsPerBlock];
        var g = new byte[DirectEncoding.PixelsPerBlock];
        var b = new byte[DirectEncoding.PixelsPerBlock];
        for (int i = 0; i < DirectEncoding.PixelsPerBlock; i++)
        {
            var color = directBlock.Colors[i];
            a[i] = color.a;
            r[i] = color.r;
            g[i] = color.g;
            b[i] = color.b;
        }
        var swizzledBytes = new byte[DirectEncoding.BytesPerBlock];
        InterleaveRGBA8Bytes(a, 33, ref swizzledBytes);
        InterleaveRGBA8Bytes(r, 32, ref swizzledBytes);
        InterleaveRGBA8Bytes(g, 01, ref swizzledBytes);
        InterleaveRGBA8Bytes(b, 00, ref swizzledBytes);
        writer.Write(swizzledBytes);
    }

    // TOOD: make generic on arrays, move elsewhere

    /// <summary>
    ///     Iterate over <paramref name="bytes"/> a total of <paramref name="count"/> times, extracting each 
    ///     value from position <paramref name="baseIndex"/> with successive separation of <paramref name="stride"/>.
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="baseIndex"></param>
    /// <returns>
    ///     
    /// </returns>
    public static byte[] ExtractRGBA8Bytes(ReadOnlySpan<byte> bytes, int baseIndex)
    {
        const int stride = 2;
        const int count = 16;
        byte[] values = new byte[count];

        int dstIndex = 0;
        int srcIndex = baseIndex;
        while (srcIndex < baseIndex + count * stride)
        {
            // Copy
            values[dstIndex] = bytes[srcIndex];
            // Increment
            srcIndex += stride;
            dstIndex++;
        }
        return values;
    }

    /// <summary>
    ///     Iterate over <paramref name="bytes"/> a total of <paramref name="count"/> times, interleaving
    ///     each value to position <paramref name="baseIndex"/> with successive separation of <paramref name="stride"/>.
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="baseIndex"></param>
    /// <param name="destination"></param>
    public static void InterleaveRGBA8Bytes(ReadOnlySpan<byte> bytes, int baseIndex, ref byte[] destination)
    {
        const int stride = 2;
        const int count = 16;

        int dstIndex = 0;
        int srcIndex = baseIndex;
        while (srcIndex < baseIndex + count * stride)
        {
            // Copy
            destination[srcIndex] = bytes[dstIndex];
            // Increment
            srcIndex += stride;
            dstIndex++;
        }
    }


    internal static DirectBlock ReadCMPR(EndianBinaryReader reader)
    {
        DirectEncoding DirectEncoding = CMPR;
        TextureColor[] pixels = new TextureColor[DirectEncoding.PixelsPerBlock];
        // CMPR 8x8 is split into 2x2, ie quadrants of 4x4
        for (int qy = 0; qy < 2; qy++)
        {
            for (int qx = 0; qx < 2; qx++)
            {
                // Each subdivision is 4x4 with 2 leading RGB565 colors
                ushort c0 = reader.ReadUInt16();
                ushort c1 = reader.ReadUInt16();
                TextureColor[] palette = GetCmprPalette(c0, c1);
                // Then followed by 2-bit indexes packed into 32-bits
                uint indexesPacked = reader.ReadUInt32();
                byte[] indexes = UnpackIndexes(indexesPacked);

                // Now that we have the data, get colors for each index
                // and place it in the direct color block

                // We must get the first index of the 2x2 grid for the pixel in the larger 8x8 grid.
                // The following math gets the first pixel index for the quadrant.
                int quadrantIndex2x2 = qx * 4 + qy * 32;
                // Then we can iterate over the 4x4 subset.
                for (int y = 0; y < 4; y++)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        // Get color
                        int blockIndex4x4 = x + (y * 4); // sub 4x4 index
                        byte paletteIndex = indexes[blockIndex4x4];
                        var color = palette[paletteIndex];

                        // Store color
                        int blockIndex8x8 = x + (y * 8) + quadrantIndex2x2; // true 8x8 index
                        pixels[blockIndex8x8] = color;
                    }
                }
            }
        }
        DirectBlock directBlock = new(DirectEncoding, pixels);
        return directBlock;
    }

    internal static void WriteCMPR(EndianBinaryWriter writer, DirectBlock directBlock)
    {
        DirectEncoding DirectEncoding = CMPR;
        AssertEncoding(DirectEncoding, directBlock.DirectEncoding);
        // Split this 8x8 block into 4 quadrants (2x2), each 16 pixels (4x4).
        for (int qy = 0; qy < 2; qy++)
        {
            for (int qx = 0; qx < 2; qx++)
            {
                // Get the 4x4 pixel subset
                var colors = Get4x4SubBlockColors(directBlock.Colors, qx, qy);
                // Convert from own format into that used by BCnEncoder
                ColorRgba32[] colors32 = new ColorRgba32[16];
                for (int i = 0; i < 16; i++)
                {
                    colors32[i].r = colors[i].r;
                    colors32[i].g = colors[i].g;
                    colors32[i].b = colors[i].b;
                    colors32[i].a = colors[i].a;
                }

                // Get bytes to write
                byte[] rawBytes = BC1Encoder.EncodeToRawBytes(new ReadOnlyMemory2D<ColorRgba32>(colors32, 4, 4))[0];
                // Colors byte ordering is OK
                ushort c0 = BitConverter.ToUInt16([rawBytes[0], rawBytes[1]]);
                ushort c1 = BitConverter.ToUInt16([rawBytes[2], rawBytes[3]]);
                // But indexes are flipped X and Y
                // Flip Y axis
                uint indexesByteSwapped = BitConverter.ToUInt32([rawBytes[7], rawBytes[6], rawBytes[5], rawBytes[4]]);
                // Flip X axis
                uint indexesBitSwapped =
                    (indexesByteSwapped >> 6) & (0b_00000011_00000011_00000011_00000011) |
                    (indexesByteSwapped >> 2) & (0b_00001100_00001100_00001100_00001100) |
                    (indexesByteSwapped << 2) & (0b_00110000_00110000_00110000_00110000) |
                    (indexesByteSwapped << 6) & (0b_11000000_11000000_11000000_11000000);
                // Finally, write out DXT1 block
                writer.Write(c0);
                writer.Write(c1);
                writer.Write(indexesBitSwapped);
            }
        }
    }

    /// <summary>
    ///     Reconstruct a CMPR block's palette based on the 2 color endpoints <paramref name="c0"/>
    ///     and <paramref name="c1"/>.
    /// </summary>
    /// <param name="c0">Color 0.</param>
    /// <param name="c1">Color 1.</param>
    /// <returns>
    ///     An array of <cref>TextureColor</cref> with exactly 4 colors in it for 2-bit CMPR index.
    /// </returns>
    public static TextureColor[] GetCmprPalette(ushort c0, ushort c1)
    {
        var colors = new TextureColor[4];
        colors[0] = TextureColor.FromRGB565(c0);
        colors[1] = TextureColor.FromRGB565(c1);
        if (c0 > c1)
        {
            colors[2] = TextureColor.Lerp(colors[0], colors[1], 1f / 3f);
            colors[3] = TextureColor.Lerp(colors[0], colors[1], 2f / 3f);
        }
        else
        {
            colors[2] = TextureColor.Lerp(colors[0], colors[1], 1f / 2f);
            colors[3] = new TextureColor(0x00000000);
        }
        return colors;
    }

    /// <summary>
    ///     Unpack CMPR indexes from <paramref name="indexesPacked"/>.
    /// </summary>
    /// <param name="indexesPacked">The 16 2-bit CMPR indexes packed into uint32.</param>
    /// <returns>
    ///     A byte array of exactly 16 entries, one for each of the CMPR indexes in a 4x4 block. 
    /// </returns>
    public static byte[] UnpackIndexes(uint indexesPacked)
    {
        byte[] indexes = new byte[4 * 4];
        for (int i = 0; i < indexes.Length; i++)
        {
            // left-most bits are index0, right-most bits are index15
            int rightShift = (15 - i) * 2;
            indexes[i] = (byte)((indexesPacked >> rightShift) & 0b_11);
        }
        return indexes;
    }

    /// <summary>
    ///     Pack CMPR <paramref name="indexes"/>.
    /// </summary>
    /// <param name="indexes">The 16 2-bit CMPR indexes to pack into uint32.</param>
    /// <returns>
    ///     A uint32 where each 2 bits represent a CMPR color index.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown if length of indexes array is not exatly 16.
    /// </exception>
    public static uint PackIndexes(byte[] indexes)
    {
        bool isValidQuantity = indexes.Length == 4 * 4; // 16
        if (!isValidQuantity)
        {
            string msg = $"Argument {nameof(indexes)}.Length is not exatly 16.";
            throw new ArgumentException(msg);
        }

        uint packedIndexes = 0;
        for (int i = 0; i < indexes.Length; i++)
        {
            // left-most bits are index0, right-most bits are index15
            int leftShift = (15 - i) * 2;
            uint bits = (uint)((indexes[i] & 0b11) << leftShift);
            packedIndexes |= bits;
        }

        return packedIndexes;
    }

    /// <summary>
    ///     
    /// </summary>
    /// <param name="colors"></param>
    /// <param name="qx"></param>
    /// <param name="qy"></param>
    /// <returns>
    ///     
    /// </returns>
    public static TextureColor[] Get4x4SubBlockColors(TextureColor[] colors, int qx, int qy)
    {
        // DXT1 sub-block is 4x4 inside the larger 8x8
        TextureColor[] subBlock = new TextureColor[16];

        // Compute starts and ends
        int yStart = qy * 4;
        int yEnd = yStart + 4;
        int xStart = qx * 4;
        int xEnd = xStart + 4;

        // Iterate over 8x8 array, copying out 4x4 pixel quadrant
        int blockIndex4x4 = 0;
        for (int y = yStart; y < yEnd; y++)
        {
            for (int x = xStart; x < xEnd; x++)
            {
                int blockIndex8x8 = x + (y * 8);
                subBlock[blockIndex4x4++] = colors[blockIndex8x8];
            }
        }

        return subBlock;
    }







    /// <summary>
    ///     Encoding format for '4-bit intensity' grayscale texture.
    /// </summary>
    public static readonly DirectEncoding I4 = new()
    {
        DirectFormat = DirectTextureFormat.I4,
        BlockWidth = 8,
        BlockHeight = 8,
        BitsPerPixel = 4,
        BytesPerBlock = 32, // 8 * 8 * 0.5(4bpp)
        ReadBlock = ReadI4,
        WriteBlock = WriteI4,
    };

    /// <summary>
    ///     Encoding format for '8-bit intensity' grayscale texture.
    /// </summary>
    public static readonly DirectEncoding I8 = new()
    {
        DirectFormat = DirectTextureFormat.I8,
        BlockWidth = 8,
        BlockHeight = 4,
        BitsPerPixel = 8,
        BytesPerBlock = 32, // 8 * 4 * 1(8bpp)
        ReadBlock = ReadI8,
        WriteBlock = WriteI8,
    };

    /// <summary>
    ///     Encoding format for '4-bit intensity and 4-bit alpha' grayscale texture.
    /// </summary>
    public static readonly DirectEncoding IA4 = new()
    {
        DirectFormat = DirectTextureFormat.IA4,
        BlockWidth = 8,
        BlockHeight = 4,
        BitsPerPixel = 8,
        BytesPerBlock = 32, // 8 * 4 * 1(8bpp)
        ReadBlock = ReadIA4,
        WriteBlock = WriteIA4,
    };

    /// <summary>
    ///     Encoding format for '8-bit intensity and 8-bit alpha' grayscale texture.
    /// </summary>
    public static readonly DirectEncoding IA8 = new()
    {
        DirectFormat = DirectTextureFormat.IA8,
        BlockWidth = 4,
        BlockHeight = 4,
        BitsPerPixel = 16,
        BytesPerBlock = 32, // 4 * 4 * 2(16bpp)
        ReadBlock = ReadIA8,
        WriteBlock = WriteIA8,
    };

    /// <summary>
    ///     Encoding format for '5-bit red, 6-bit green, and 5-bit blue' colour texture.
    /// </summary>
    public static readonly DirectEncoding RGB565 = new()
    {
        DirectFormat = DirectTextureFormat.RGB565,
        BlockWidth = 4,
        BlockHeight = 4,
        BitsPerPixel = 16,
        BytesPerBlock = 32, // 4 * 4 * 2(16bpp)
        ReadBlock = ReadRGB565,
        WriteBlock = WriteRGB565,
    };

    /// <summary>
    ///     Encoding format for '5-bit red, 5-bit green, 5-bit blue, and 1-bit fixed alpha (1)'
    ///                and also '4-bit red, 4-bit green, 4-bit blue, and 3-bit alpha (4th bit always 0)' colour texture.
    /// </summary>
    public static readonly DirectEncoding RGB5A3 = new()
    {
        DirectFormat = DirectTextureFormat.RGB5A3,
        BlockWidth = 4,
        BlockHeight = 4,
        BitsPerPixel = 16,
        BytesPerBlock = 32, // 4 * 4 * 2(16bpp)
        ReadBlock = ReadRGB5A3,
        WriteBlock = WriteRGB5A3,
    };

    /// <summary>
    ///     Encoding format for '8-bit red, 8-bit green, 8-bit blue, and 8-bit alpha' colour texture.
    /// </summary>
    public static readonly DirectEncoding RGBA8 = new()
    {
        DirectFormat = DirectTextureFormat.RGBA8,
        BlockWidth = 4,
        BlockHeight = 4,
        BitsPerPixel = 32,
        // Big lie: RGBA8 takes 2 4x4 blocks, one is AR then the other GB
        // However, since they are ordered like so, you can treat it like
        // a 4x4 blocks but of size 64 bytes rather than 32 bytes.
        BytesPerBlock = 64, // 4 * 4 * 2(32bpp)
        ReadBlock = ReadRGBA8,
        WriteBlock = WriteRGBA8,
    };

    public static readonly DirectEncoding CMPR = new()
    {
        DirectFormat = DirectTextureFormat.CMPR,
        BlockWidth = 8,
        BlockHeight = 8,
        BitsPerPixel = 4,
        BytesPerBlock = 32, // 8 * 8 * 0.5(4bpp)
        ReadBlock = ReadCMPR,
        WriteBlock = WriteCMPR,
    };

    public static readonly ImmutableDictionary<DirectTextureFormat, DirectEncoding> MapFormatToEncoding =
    [
        new(DirectTextureFormat.I4, I4),
        new(DirectTextureFormat.I8, I8),
        new(DirectTextureFormat.IA4, IA4),
        new(DirectTextureFormat.IA8, IA8),
        new(DirectTextureFormat.RGB565, RGB565),
        new(DirectTextureFormat.RGB5A3, RGB5A3),
        new(DirectTextureFormat.RGBA8, RGBA8),
        new(DirectTextureFormat.CMPR, CMPR),
    ];

}
