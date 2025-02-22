using Manifold.IO;
using System;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using Microsoft.Toolkit.HighPerformance;


namespace GameCube.GX.Texture;

/// <summary>
///     Encoding format for 'compressed' (BC1/DXT1) texture.
/// </summary>
/// 
/// TODO: better compression
/// https://github.com/Nominom/BCnEncoder.NET/blob/master/BCnEnc.Net/Encoder/Bc1BlockEncoder.cs
public sealed class EncodingCMPR : DirectEncoding
{
    public override byte BlockWidth => 8;
    public override byte BlockHeight => 8;
    public override byte BitsPerColor => 4;
    public override TextureFormat Format => TextureFormat.CMPR;
    private BcEncoder BC1Encoder => new BcEncoder();

    public EncodingCMPR(CompressionQuality quality = CompressionQuality.Balanced) : base()
    {
        BC1Encoder.OutputOptions.Quality = quality;
        BC1Encoder.OutputOptions.Format = CompressionFormat.Bc1;
    }


    public override Block ReadBlock(EndianBinaryReader reader)
    {
        var block = new DirectBlock(BlockWidth, BlockHeight, Format);

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
                        block.Colors[blockIndex8x8] = color;
                    }
                }
            }
        }
        return block;
    }

    public override void WriteBlock(EndianBinaryWriter writer, Block block)
    {
        // Get the 8x8 color block
        var colorBlock = block as DirectBlock;

        // Split this 8x8 block into 4 quadrants (2x2), each 16 pixels (4x4).
        for (int qy = 0; qy < 2; qy++)
        {
            for (int qx = 0; qx < 2; qx++)
            {
                // Get the 4x4 pixel subset
                var colors = Get4x4SubBlockColors(colorBlock!.Colors, qx, qy);
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

    public void OldWriteBlock(EndianBinaryWriter writer, Block block)
    {
        var colorBlock = block as DirectBlock;

        // CMPR 8x8 is split into 2x2 quadrants of 4x4 pixels
        for (int qy = 0; qy < 2; qy++)
        {
            for (int qx = 0; qx < 2; qx++)
            {
                // Get colors
                var colors4x4 = new TextureColor[4 * 4];
                // The following math gets the first pixel index for the quadrant.
                int quadrantBaseIndex = qx * 4 + qy * 32;
                for (int y = 0; y < 4; y++)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        int blockIndex4x4 = x + (y * 4); // 4x4
                        int blockIndex8x8 = x + (y * 8) + quadrantBaseIndex; // true 8x8 index
                        colors4x4[blockIndex4x4] = colorBlock.Colors[blockIndex8x8];
                    }
                }

                // Get color palette and indexes from compressor
                GetCmprColorAndIndexes(colors4x4, out ushort c0, out ushort c1, out uint indexesPacked);
                // write DXT1 block
                writer.Write(c0);
                writer.Write(c1);
                writer.Write(indexesPacked);
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
    ///     Get CMPR compressed colors and indexes.
    /// </summary>
    /// <param name="colors4x4"></param>
    /// <param name="c0">Color 0 of CMPR palette.</param>
    /// <param name="c1">Color 1 of CMPR palette.</param>
    /// <param name="indexesPacked">Packed CMPR color indexes.</param>
    public static void GetCmprColorAndIndexes(TextureColor[] colors4x4, out ushort c0, out ushort c1, out uint indexesPacked)
    {
        // For now use naive range-fit for CMPR
        DXT1.MinMaxFitColors(colors4x4, out c0, out c1, out indexesPacked);
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
    ///     Pack CMPR <paramref name="indexes"/>
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

    public TextureColor[] Get4x4SubBlockColors(TextureColor[] colors, int qx, int qy)
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

}
