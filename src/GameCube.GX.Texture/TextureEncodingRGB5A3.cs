using Manifold.IO;

namespace GameCube.GX.Texture;

/// <summary>
///     Encoding format for '5-bit red, 5-bit green, 5-bit blue, and 0-bit alpha'
///                and also '4-bit red, 4-bit green, 4-bit blue, and 3-bit alpha' colour texture.
/// </summary>
public sealed class TextureEncodingRGB5A3 : DirectEncoding
{
    public override byte BlockWidth => 4;
    public override byte BlockHeight => 4;
    public override byte BitsPerColor => 16;
    public override TextureFormat Format => TextureFormat.RGB5A3;


    public override Block ReadBlock(EndianBinaryReader reader)
    {
        var block = new DirectBlock(BlockWidth, BlockHeight, Format);
        for (int y = 0; y < block.Height; y++)
        {
            for (int x = 0; x < block.Width; x++)
            {
                ushort rgb5a3 = reader.ReadUInt16();
                var color = TextureColor.FromRGB5A3(rgb5a3);
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
                ushort rgb5a3 = TextureColor.ToRGB5A3(color);
                writer.Write(rgb5a3);
            }
        }
    }
}
