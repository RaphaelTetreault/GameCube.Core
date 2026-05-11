using Manifold.IO;
using System;

namespace GameCube.GX.Texture;

/// <summary>
///     Encoding format for '8-bit red, 8-bit green, 8-bit blue, and 8-bit alpha' colour texture.
/// </summary>
public sealed class TextureEncodingRGBA8 : DirectEncoding
{
    // Big lie: RGBA8 takes 2 4x4 blocks, one is AR then the other GB
    // However, since they are ordered like so, you can treat it like
    // a 4x4 blocks but of size 64 bytes rather than 32 bytes.
    public override byte BlockWidth => 4;
    public override byte BlockHeight => 4;
    public override byte BitsPerColor => 32;
    public override TextureFormat Format => TextureFormat.RGBA8;


    public override Block ReadBlock(EndianBinaryReader reader)
    {
        var directBlock = new DirectBlock(BlockWidth, BlockHeight, Format);
        int nColors = BlockWidth * BlockHeight;
        int nBytes = nColors * BytesPerPixel;
        var bytes = reader.ReadBytes(nBytes);
        var colors = new TextureColor[nColors];
        var a = ExtractRGBA8Bytes(bytes, 33);
        var r = ExtractRGBA8Bytes(bytes, 32);
        var g = ExtractRGBA8Bytes(bytes, 1);
        var b = ExtractRGBA8Bytes(bytes, 0);
        for (int i = 0; i < colors.Length; i++)
            colors[i] = new TextureColor(r[i], g[i], b[i], a[i]);
        directBlock.Colors = colors;
        return directBlock;
    }

    public override void WriteBlock(EndianBinaryWriter writer, Block block)
    {
        var directBlock = block as DirectBlock;
        int nColors = directBlock!.Colors.Length;
        Assert.IsTrue(nColors == BlockWidth * BlockHeight);
        var a = new byte[nColors];
        var r = new byte[nColors];
        var g = new byte[nColors];
        var b = new byte[nColors];
        for (int i = 0; i < nColors; i++)
        {
            var color = directBlock.Colors[i];
            a[i] = color.a;
            r[i] = color.r;
            g[i] = color.g;
            b[i] = color.b;
        }
        int blockByteCount = BlockWidth * BlockHeight * BytesPerPixel;
        var bytes = new byte[blockByteCount];
        InterleaveRGBA8Bytes(a, 33, ref bytes);
        InterleaveRGBA8Bytes(r, 32, ref bytes);
        InterleaveRGBA8Bytes(g, 01, ref bytes);
        InterleaveRGBA8Bytes(b, 00, ref bytes);
        writer.Write(bytes);
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

}
