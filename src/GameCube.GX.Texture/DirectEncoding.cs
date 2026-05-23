using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using CommunityToolkit.HighPerformance;
using Manifold.IO;
using System;
using System.Collections.Immutable;

namespace GameCube.GX.Texture;

/// <summary>
///     Representation of a GameCube direct color texture format encoding.
/// </summary>
public record class DirectEncoding : IBlockEncoding
{
    public delegate DirectBlock ReadBlock(EndianBinaryReader reader);
    public delegate void WriteBlock(EndianBinaryWriter writer, DirectBlock directBlock);

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
    ///     The direct texture format used by this encoding.
    /// </summary>
    public required DirectTextureFormat DirectFormat { get; init; }

    /// <summary>
    ///     The pixel width of a block for this encoding.
    /// </summary>
    public required byte BlockPixelWidth { get; init; }

    /// <summary>
    ///     The pixel height of a block for this encoding.
    /// </summary>
    public required byte BlockPixelHeight { get; init; }

    /// <summary>
    ///     The number of bits used by this encoding to represent a single color.
    /// </summary>
    public required byte BitsPerPixel { get; init; }

    /// <summary>
    ///     The number of bytes used by this encoding per block.
    /// </summary>
    public required byte BytesPerBlock { get; init; }

    /// <summary>
    ///     Function to read a direct block for this encoding.
    /// </summary>
    public required DirectEncoding.ReadBlock ReadDirectBlock { get; init; }

    /// <summary>
    ///     Function to write a direct block for this encoding.
    /// </summary>
    public required DirectEncoding.WriteBlock WriteDirectBlock { get; init; }


    /// <summary>
    ///     Get <see cref="DirectFormat"/> as <see cref="TextureFormat"/>.
    /// </summary>
    public TextureFormat Format => (TextureFormat)DirectFormat;

    /// <summary>
    ///     The pixel width of a block for this encoding.
    /// </summary>
    public int PixelsPerBlock => BlockPixelWidth * BlockPixelHeight;


    /// <summary>
    ///     Read <paramref name="blocksCount"/> number of direct blocks from <paramref name="reader"/>
    ///     using this encoding.
    /// </summary>
    /// <param name="reader">The binary reader to read from.</param>
    /// <param name="blocksCount">The number of direct blocks to read.</param>
    /// <returns>
    ///     Array of <paramref name="blocksCount"/> number of direct blocks read from <paramref name="reader"/>.
    /// </returns>
    public DirectBlock[] ReadBlocks(EndianBinaryReader reader, int blocksCount)
    {
        DirectBlock[] directBlocks = new DirectBlock[blocksCount];
        for (int i = 0; i < blocksCount; i++)
            directBlocks[i] = ReadDirectBlock.Invoke(reader);
        return directBlocks;
    }

    /// <summary>
    ///     Read number of direct blocks required to construct <paramref name="pxWidth"/> by 
    ///     <paramref name="pxHeight"/> from <paramref name="reader"/> using this encoding.
    /// </summary>
    /// <param name="reader">The binary reader to read from.</param>
    /// <param name="pxWidth">Pixel width of texture.</param>
    /// <param name="pxHeight">Pixel height of texture.</param>
    /// <returns>
    ///     Array of direct blocks read from <paramref name="reader"/> construct a
    ///     <paramref name="pxWidth"/> by <paramref name="pxHeight"/> texture.
    /// </returns>
    public DirectBlock[] ReadBlocks(EndianBinaryReader reader, int pxWidth, int pxHeight)
    {
        TextureBlocksInfo blocks = TextureBlocksInfo.FromPixelDimensions(pxWidth, pxHeight, this);
        DirectBlock[] directBlocks = ReadBlocks(reader, blocks.BlockCount);
        return directBlocks;
    }


    [System.Diagnostics.Conditional("DEBUG")]
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
        DirectEncoding directEncoding = I4;
        TexturePixel[] pixels = new TexturePixel[directEncoding.PixelsPerBlock];
        for (int y = 0; y < directEncoding.BlockPixelHeight; y++)
        {
            // Process 2 pixels per pass, high and low nybbles
            for (int x = 0; x < directEncoding.BlockPixelWidth; x += 2)
            {
                // Get 2 colors at a time from 1 byte
                byte nybbles = reader.ReadByte();
                (TexturePixel color0, TexturePixel color1) = TexturePixel.FromI4(nybbles);
                // Assign colors
                int index0 = x + (y * directEncoding.BlockPixelWidth);
                int index1 = index0 + 1;
                pixels[index0] = color0;
                pixels[index1] = color1;
            }
        }
        DirectBlock directBlock = new(directEncoding, pixels);
        return directBlock;
    }

    internal static void WriteI4(EndianBinaryWriter writer, DirectBlock directBlock)
    {
        DirectEncoding directEncoding = I4;
        AssertEncoding(directEncoding, directBlock.DirectEncoding);
        for (int y = 0; y < directEncoding.BlockPixelHeight; y++)
        {
            // Process 2 pixels per pass, set as high and low nybbles
            for (int x = 0; x < directEncoding.BlockPixelWidth; x += 2)
            {
                int index0 = x + (y * directEncoding.BlockPixelWidth);
                int index1 = index0 + 1;
                TexturePixel intensity0 = directBlock[index0];
                TexturePixel intensity1 = directBlock[index1];
                byte intensity01 = TexturePixel.ToI4(intensity0, intensity1);
                writer.Write(intensity01);
            }
        }
    }

    internal static DirectBlock ReadI8(EndianBinaryReader reader)
    {
        DirectEncoding directEncoding = I8;
        TexturePixel[] pixels = new TexturePixel[directEncoding.PixelsPerBlock];
        for (int y = 0; y < directEncoding.BlockPixelHeight; y++)
        {
            for (int x = 0; x < directEncoding.BlockPixelWidth; x++)
            {
                byte i8 = reader.ReadByte();
                var color = new TexturePixel(i8);
                int index = x + (y * directEncoding.BlockPixelWidth);
                pixels[index] = color;
            }
        }
        DirectBlock directBlock = new(directEncoding, pixels);
        return directBlock;
    }

    internal static void WriteI8(EndianBinaryWriter writer, DirectBlock directBlock)
    {
        DirectEncoding directEncoding = I8;
        AssertEncoding(directEncoding, directBlock.DirectEncoding);
        for (int y = 0; y < directEncoding.BlockPixelHeight; y++)
        {
            for (int x = 0; x < directEncoding.BlockPixelWidth; x++)
            {
                int index = x + (y * directEncoding.BlockPixelWidth);
                var color = directBlock[index];
                byte i8 = color.GetIntensity();
                writer.Write(i8);
            }
        }
    }

    internal static DirectBlock ReadIA4(EndianBinaryReader reader)
    {
        DirectEncoding directEncoding = IA4;
        TexturePixel[] pixels = new TexturePixel[directEncoding.PixelsPerBlock];
        for (int y = 0; y < directEncoding.BlockPixelHeight; y++)
        {
            for (int x = 0; x < directEncoding.BlockPixelWidth; x++)
            {
                byte ia4 = reader.ReadByte();
                var color = TexturePixel.FromIA4(ia4);
                int index = x + (y * directEncoding.BlockPixelWidth);
                pixels[index] = color;
            }
        }
        DirectBlock directBlock = new(directEncoding, pixels);
        return directBlock;
    }

    internal static void WriteIA4(EndianBinaryWriter writer, DirectBlock directBlock)
    {
        DirectEncoding directEncoding = IA4;
        AssertEncoding(directEncoding, directBlock.DirectEncoding);
        for (int y = 0; y < directEncoding.BlockPixelHeight; y++)
        {
            for (int x = 0; x < directEncoding.BlockPixelWidth; x++)
            {
                int index = x + (y * directEncoding.BlockPixelWidth);
                var color = directBlock[index];
                byte ia4 = TexturePixel.ToIA4(color);
                writer.Write(ia4);
            }
        }
    }

    internal static DirectBlock ReadIA8(EndianBinaryReader reader)
    {
        DirectEncoding directEncoding = IA8;
        TexturePixel[] pixels = new TexturePixel[directEncoding.PixelsPerBlock];
        for (int y = 0; y < directEncoding.BlockPixelHeight; y++)
        {
            for (int x = 0; x < directEncoding.BlockPixelWidth; x++)
            {
                ushort ia8 = reader.ReadUInt16();
                var color = TexturePixel.FromIA8(ia8);
                int index = x + (y * directEncoding.BlockPixelWidth);
                pixels[index] = color;
            }
        }
        DirectBlock directBlock = new(directEncoding, pixels);
        return directBlock;
    }

    internal static void WriteIA8(EndianBinaryWriter writer, DirectBlock directBlock)
    {
        DirectEncoding directEncoding = IA8;
        AssertEncoding(directEncoding, directBlock.DirectEncoding);
        for (int y = 0; y < directEncoding.BlockPixelHeight; y++)
        {
            for (int x = 0; x < directEncoding.BlockPixelWidth; x++)
            {
                int index = x + (y * directEncoding.BlockPixelWidth);
                var color = directBlock[index];
                ushort ia8 = TexturePixel.ToIA8(color);
                writer.Write(ia8);
            }
        }
    }

    internal static DirectBlock ReadRGB565(EndianBinaryReader reader)
    {
        DirectEncoding directEncoding = RGB565;
        TexturePixel[] pixels = new TexturePixel[directEncoding.PixelsPerBlock];
        for (int y = 0; y < directEncoding.BlockPixelHeight; y++)
        {
            for (int x = 0; x < directEncoding.BlockPixelWidth; x++)
            {
                ushort rgb565 = reader.ReadUInt16();
                var color = TexturePixel.FromRGB565(rgb565);
                int index = x + (y * directEncoding.BlockPixelWidth);
                pixels[index] = color;
            }
        }
        DirectBlock directBlock = new(directEncoding, pixels);
        return directBlock;
    }

    internal static void WriteRGB565(EndianBinaryWriter writer, DirectBlock directBlock)
    {
        DirectEncoding directEncoding = RGB565;
        AssertEncoding(directEncoding, directBlock.DirectEncoding);
        for (int y = 0; y < directEncoding.BlockPixelHeight; y++)
        {
            for (int x = 0; x < directEncoding.BlockPixelWidth; x++)
            {
                int index = x + (y * directEncoding.BlockPixelWidth);
                var color = directBlock[index];
                ushort rgb565 = TexturePixel.ToRGB565(color);
                writer.Write(rgb565);
            }
        }
    }

    internal static DirectBlock ReadRGB5A3(EndianBinaryReader reader)
    {
        DirectEncoding directEncoding = RGB5A3;
        TexturePixel[] pixels = new TexturePixel[directEncoding.PixelsPerBlock];
        for (int y = 0; y < directEncoding.BlockPixelHeight; y++)
        {
            for (int x = 0; x < directEncoding.BlockPixelWidth; x++)
            {
                ushort rgb5a3 = reader.ReadUInt16();
                var color = TexturePixel.FromRGB5A3(rgb5a3);
                int index = x + (y * directEncoding.BlockPixelWidth);
                pixels[index] = color;
            }
        }
        DirectBlock directBlock = new(directEncoding, pixels);
        return directBlock;
    }

    internal static void WriteRGB5A3(EndianBinaryWriter writer, DirectBlock directBlock)
    {
        DirectEncoding directEncoding = RGB5A3;
        AssertEncoding(directEncoding, directBlock.DirectEncoding);
        for (int y = 0; y < directEncoding.BlockPixelHeight; y++)
        {
            for (int x = 0; x < directEncoding.BlockPixelWidth; x++)
            {
                int index = x + (y * directEncoding.BlockPixelWidth);
                var color = directBlock[index];
                ushort rgb5a3 = TexturePixel.ToRGB5A3(color);
                writer.Write(rgb5a3);
            }
        }
    }

    internal static DirectBlock ReadRGBA8(EndianBinaryReader reader)
    {
        DirectEncoding directEncoding = RGBA8;
        TexturePixel[] pixels = new TexturePixel[directEncoding.PixelsPerBlock];
        var bytes = reader.ReadBytes(directEncoding.BytesPerBlock);
        var a = Rgba8ExtractBytes(bytes, 33);
        var r = Rgba8ExtractBytes(bytes, 32);
        var g = Rgba8ExtractBytes(bytes, 1);
        var b = Rgba8ExtractBytes(bytes, 0);
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new TexturePixel(r[i], g[i], b[i], a[i]);
        DirectBlock directBlock = new(directEncoding, pixels);
        return directBlock;

        static byte[] Rgba8ExtractBytes(ReadOnlySpan<byte> bytes, int baseIndex)
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
    }

    internal static void WriteRGBA8(EndianBinaryWriter writer, DirectBlock directBlock)
    {
        DirectEncoding directEncoding = RGBA8;
        AssertEncoding(directEncoding, directBlock.DirectEncoding);
        var a = new byte[directEncoding.PixelsPerBlock];
        var r = new byte[directEncoding.PixelsPerBlock];
        var g = new byte[directEncoding.PixelsPerBlock];
        var b = new byte[directEncoding.PixelsPerBlock];
        for (int i = 0; i < directEncoding.PixelsPerBlock; i++)
        {
            var color = directBlock.Pixels[i];
            a[i] = color.a;
            r[i] = color.r;
            g[i] = color.g;
            b[i] = color.b;
        }
        var swizzledBytes = new byte[directEncoding.BytesPerBlock];
        Rgba8InterleaveBytes(a, 33, ref swizzledBytes);
        Rgba8InterleaveBytes(r, 32, ref swizzledBytes);
        Rgba8InterleaveBytes(g, 01, ref swizzledBytes);
        Rgba8InterleaveBytes(b, 00, ref swizzledBytes);
        writer.Write(swizzledBytes);

        static void Rgba8InterleaveBytes(ReadOnlySpan<byte> bytes, int baseIndex, ref byte[] destination)
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
    }

    internal static DirectBlock ReadCMPR(EndianBinaryReader reader)
    {
        DirectEncoding DirectEncoding = CMPR;
        TexturePixel[] pixels = new TexturePixel[DirectEncoding.PixelsPerBlock];
        // CMPR 8x8 is split into 2x2, ie quadrants of 4x4
        for (int qy = 0; qy < 2; qy++)
        {
            for (int qx = 0; qx < 2; qx++)
            {
                // Each subdivision is 4x4 with 2 leading RGB565 colors
                ushort c0 = reader.ReadUInt16();
                ushort c1 = reader.ReadUInt16();
                TexturePixel[] palette = GetCmprPalette(c0, c1);
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

        // Reconstruct CMPR palette from 2 color endpoints.
        static TexturePixel[] GetCmprPalette(ushort c0, ushort c1)
        {
            var colors = new TexturePixel[4];
            colors[0] = TexturePixel.FromRGB565(c0);
            colors[1] = TexturePixel.FromRGB565(c1);
            if (c0 > c1)
            {
                colors[2] = TexturePixel.Lerp(colors[0], colors[1], 1f / 3f);
                colors[3] = TexturePixel.Lerp(colors[0], colors[1], 2f / 3f);
            }
            else
            {
                colors[2] = TexturePixel.Lerp(colors[0], colors[1], 1f / 2f);
                colors[3] = new TexturePixel(0x00000000);
            }
            return colors;
        }

        // Unpack CMPR indexes from packed indexes.
        // Indexes are 16 x 2-bit CMPR indexes packed into uint32.</param>
        static byte[] UnpackIndexes(uint indexesPacked)
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
                var colors = Get4x4SubBlockColors(directBlock.Pixels.AsSpan(), qx, qy);
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

        static TexturePixel[] Get4x4SubBlockColors(ReadOnlySpan<TexturePixel> colors, int qx, int qy)
        {
            // DXT1 sub-block is 4x4 inside the larger 8x8
            TexturePixel[] subBlock = new TexturePixel[16];

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

        //// Pack CMPR indexes.
        //// Indexes are 16 x 2-bit CMPR indexes to pack into uint32.</param>
        //static uint PackIndexes(byte[] indexes)
        //{
        //    bool isValidQuantity = indexes.Length == 4 * 4; // 16
        //    if (!isValidQuantity)
        //    {
        //        string msg = $"Argument {nameof(indexes)}.Length is not exatly 16.";
        //        throw new ArgumentException(msg);
        //    }
        //    uint packedIndexes = 0;
        //    for (int i = 0; i < indexes.Length; i++)
        //    {
        //        // left-most bits are index0, right-most bits are index15
        //        int leftShift = (15 - i) * 2;
        //        uint bits = (uint)((indexes[i] & 0b11) << leftShift);
        //        packedIndexes |= bits;
        //    }
        //    return packedIndexes;
        //}
    }




    /// <summary>
    ///     Encoding format for '4-bit intensity' grayscale texture.
    /// </summary>
    public static readonly DirectEncoding I4 = new()
    {
        DirectFormat = DirectTextureFormat.I4,
        BlockPixelWidth = 8,
        BlockPixelHeight = 8,
        BitsPerPixel = 4,
        BytesPerBlock = 32, // 8 * 8 * 0.5(4bpp)
        ReadDirectBlock = ReadI4,
        WriteDirectBlock = WriteI4,
    };

    /// <summary>
    ///     Encoding format for '8-bit intensity' grayscale texture.
    /// </summary>
    public static readonly DirectEncoding I8 = new()
    {
        DirectFormat = DirectTextureFormat.I8,
        BlockPixelWidth = 8,
        BlockPixelHeight = 4,
        BitsPerPixel = 8,
        BytesPerBlock = 32, // 8 * 4 * 1(8bpp)
        ReadDirectBlock = ReadI8,
        WriteDirectBlock = WriteI8,
    };

    /// <summary>
    ///     Encoding format for '4-bit intensity and 4-bit alpha' grayscale texture.
    /// </summary>
    public static readonly DirectEncoding IA4 = new()
    {
        DirectFormat = DirectTextureFormat.IA4,
        BlockPixelWidth = 8,
        BlockPixelHeight = 4,
        BitsPerPixel = 8,
        BytesPerBlock = 32, // 8 * 4 * 1(8bpp)
        ReadDirectBlock = ReadIA4,
        WriteDirectBlock = WriteIA4,
    };

    /// <summary>
    ///     Encoding format for '8-bit intensity and 8-bit alpha' grayscale texture.
    /// </summary>
    public static readonly DirectEncoding IA8 = new()
    {
        DirectFormat = DirectTextureFormat.IA8,
        BlockPixelWidth = 4,
        BlockPixelHeight = 4,
        BitsPerPixel = 16,
        BytesPerBlock = 32, // 4 * 4 * 2(16bpp)
        ReadDirectBlock = ReadIA8,
        WriteDirectBlock = WriteIA8,
    };

    /// <summary>
    ///     Encoding format for '5-bit red, 6-bit green, and 5-bit blue' color texture.
    /// </summary>
    public static readonly DirectEncoding RGB565 = new()
    {
        DirectFormat = DirectTextureFormat.RGB565,
        BlockPixelWidth = 4,
        BlockPixelHeight = 4,
        BitsPerPixel = 16,
        BytesPerBlock = 32, // 4 * 4 * 2(16bpp)
        ReadDirectBlock = ReadRGB565,
        WriteDirectBlock = WriteRGB565,
    };

    /// <summary>
    ///     Encoding format for '5-bit red, 5-bit green, 5-bit blue, and 1-bit fixed alpha (1)'
    ///                and also '4-bit red, 4-bit green, 4-bit blue, and 3-bit alpha (4th bit always 0)' color texture.
    /// </summary>
    public static readonly DirectEncoding RGB5A3 = new()
    {
        DirectFormat = DirectTextureFormat.RGB5A3,
        BlockPixelWidth = 4,
        BlockPixelHeight = 4,
        BitsPerPixel = 16,
        BytesPerBlock = 32, // 4 * 4 * 2(16bpp)
        ReadDirectBlock = ReadRGB5A3,
        WriteDirectBlock = WriteRGB5A3,
    };

    /// <summary>
    ///     Encoding format for '8-bit red, 8-bit green, 8-bit blue, and 8-bit alpha' color texture.
    /// </summary>
    public static readonly DirectEncoding RGBA8 = new()
    {
        DirectFormat = DirectTextureFormat.RGBA8,
        BlockPixelWidth = 4,
        BlockPixelHeight = 4,
        BitsPerPixel = 32,
        // Big lie: RGBA8 takes 2 4x4 blocks, one is AR then the other GB
        // However, since they are ordered like so, you can treat it like
        // a 4x4 blocks but of size 64 bytes rather than 32 bytes.
        BytesPerBlock = 64, // 4 * 4 * 2(32bpp)
        ReadDirectBlock = ReadRGBA8,
        WriteDirectBlock = WriteRGBA8,
    };

    /// <summary>
    ///     16-bit color using Block Compression 1 (BC1) / DXT1 compression algorithm.
    /// </summary>
    public static readonly DirectEncoding CMPR = new()
    {
        DirectFormat = DirectTextureFormat.CMPR,
        BlockPixelWidth = 8,
        BlockPixelHeight = 8,
        BitsPerPixel = 4,
        BytesPerBlock = 32, // 8 * 8 * 0.5(4bpp)
        ReadDirectBlock = ReadCMPR,
        WriteDirectBlock = WriteCMPR,
    };

    /// <summary>
    ///     Map of <see cref="DirectTextureFormat"/> to <see cref="DirectEncoding"/>.
    /// </summary>
    public static readonly ImmutableDictionary<DirectTextureFormat, DirectEncoding> MapDirectFormatToEncoding =
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
