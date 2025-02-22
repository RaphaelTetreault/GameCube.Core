using Manifold.IO;

namespace GameCube.GX.Texture;

/// <summary>
///     Encoding format for '5-bit red, 6-bit green, and 5-bit blue' colour texture.
/// </summary>
public sealed class EncodingRGB565 : DirectEncoding
{
    public override byte BlockWidth => 4;
    public override byte BlockHeight => 4;
    public override byte BitsPerColor => 16;
    public override TextureFormat Format => TextureFormat.RGB565;


    public override Block ReadBlock(EndianBinaryReader reader)
    {
        var block = new DirectBlock(BlockWidth, BlockHeight, Format);
        for (int y = 0; y < block.Height; y++)
        {
            for (int x = 0; x < block.Width; x++)
            {
                ushort rgb565 = reader.ReadUInt16();
                var color = TextureColor.FromRGB565(rgb565);
                int index = x + (y * block.Width);
                block[index] = color;
            }
        }
        return block;
    }

    public override void WriteBlock(EndianBinaryWriter writer, Block block)
    {
        var colorBlock = block as DirectBlock;
        for (int y = 0; y < block.Height; y++)
        {
            for (int x = 0; x < block.Width; x++)
            {
                int index = x + (y * block.Width);
                var color = colorBlock[index];
                ushort rgb565 = TextureColor.ToRGB565(color);
                writer.Write(rgb565);
            }
        }
    }
}
