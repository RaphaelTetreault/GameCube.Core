using Manifold.IO;

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
        var a = ExtractBytes(bytes, 33, 2, 16);
        var r = ExtractBytes(bytes, 32, 2, 16);
        var g = ExtractBytes(bytes, 01, 2, 16);
        var b = ExtractBytes(bytes, 00, 2, 16);
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
        InterleaveBytes(a, 33, 2, 16, ref bytes);
        InterleaveBytes(r, 32, 2, 16, ref bytes);
        InterleaveBytes(g, 01, 2, 16, ref bytes);
        InterleaveBytes(b, 00, 2, 16, ref bytes);
        writer.Write(bytes);
    }

    // TOOD: make generic on arrays, move elsewhere

    /// <summary>
    /// Iterate over <paramref name="bytes"/> a total of <paramref name="count"/> times, extracting each 
    /// value from position <paramref name="baseIndex"/> with successive separation of <paramref name="stride"/>.
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="baseIndex"></param>
    /// <param name="stride"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static byte[] ExtractBytes(byte[] bytes, int baseIndex, int stride, int count)
    {
        byte[] values = new byte[count];
        int valuesIndex = 0;
        for (int i = baseIndex; i < baseIndex + count * stride; i += stride)
        {
            values[valuesIndex] = bytes[i];
            valuesIndex++;
        }
        return values;
    }

    /// <summary>
    /// Iterate over <paramref name="bytes"/> a total of <paramref name="count"/> times, interleaving
    /// each value to position <paramref name="baseIndex"/> with successive separation of <paramref name="stride"/>.
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="baseIndex"></param>
    /// <param name="stride"></param>
    /// <param name="count"></param>
    /// <param name="destination"></param>
    public static void InterleaveBytes(byte[] bytes, int baseIndex, int stride, int count, ref byte[] destination)
    {
        int valuesIndex = 0;
        for (int i = baseIndex; i < baseIndex + count * stride; i += stride)
        {
            destination[i] = bytes[valuesIndex];
            valuesIndex++;
        }
    }

}
