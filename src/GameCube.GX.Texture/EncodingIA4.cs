using Manifold.IO;

namespace GameCube.GX.Texture;

/// <summary>
///     Encoding format for '4-bit intensity and 4-bit alpha' grayscale texture.
/// </summary>
public sealed class EncodingIA4 : DirectEncoding
{
    public override byte BlockWidth => 8;
    public override byte BlockHeight => 4;
    public override byte BitsPerColor => 8;
    public override TextureFormat Format => TextureFormat.IA4;


    public override Block ReadBlock(EndianBinaryReader reader)
    {
        var block = new DirectBlock(BlockWidth, BlockHeight, Format);
        for (int y = 0; y < block.Height; y++)
        {
            for (int x = 0; x < block.Width; x++)
            {
                byte ia4 = reader.ReadByte();
                var color = TextureColor.FromIA4(ia4);
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
                byte ia4 = TextureColor.ToIA4(color);
                writer.Write(ia4);
            }
        }
    }

}
