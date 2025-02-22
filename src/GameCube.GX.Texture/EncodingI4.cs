using Manifold.IO;

namespace GameCube.GX.Texture;

/// <summary>
///     Encoding format for '4-bit intensity' grayscale texture.
/// </summary>
public sealed class EncodingI4 : DirectEncoding
{
    public override byte BlockWidth => 8;
    public override byte BlockHeight => 8;
    public override byte BitsPerColor => 4;
    public override TextureFormat Format => TextureFormat.I4;


    public override Block ReadBlock(EndianBinaryReader reader)
    {
        var block = new DirectBlock(BlockWidth, BlockHeight, Format);
        for (int y = 0; y < block.Height; y++)
        {
            // Process 2 pixels per pass, high and low nybbles
            for (int x = 0; x < block.Width; x += 2)
            {
                byte nybbles = reader.ReadByte();

                int indexNybbleHigh = x + (y * block.Width);
                int indexNybbleLow = indexNybbleHigh + 1;
                // TODO: move to TextureColor
                byte intentsity0 = (byte)(((nybbles >> 4) & 0b_0000_1111) * ((1 << 4) + 1));
                byte intentsity1 = (byte)(((nybbles >> 0) & 0b_0000_1111) * ((1 << 4) + 1));

                block[indexNybbleHigh] = new TextureColor(intentsity0);
                block[indexNybbleLow] = new TextureColor(intentsity1);
            }
        }
        return block;
    }

    public override void WriteBlock(EndianBinaryWriter writer, Block block)
    {
        var colorBlock = block as DirectBlock;
        for (int y = 0; y < block.Height; y++)
        {
            // Process 2 pixels per pass, set as high and low nybbles
            for (int x = 0; x < block.Width; x += 2)
            {
                int index0 = x + (y * block.Width);
                int index1 = index0 + 1;
                var intensity0 = colorBlock[index0].GetIntensity();
                var intensity1 = colorBlock[index1].GetIntensity();
                byte intensity01 = (byte)(
                    ((intensity0 >> 0) & 0b_1111_0000) +
                    ((intensity1 >> 4) & 0b_0000_1111));
                writer.Write(intensity01);
            }
        }
    }
}
