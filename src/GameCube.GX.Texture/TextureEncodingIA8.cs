//using Manifold.IO;

//namespace GameCube.GX.Texture;

///// <summary>
/////     Encoding format for '8-bit intensity and 8-bit alpha' grayscale texture.
///// </summary>
//public sealed class TextureEncodingIA8 : DirectEncoding
//{
//    public override byte BlockWidth => 4;
//    public override byte BlockHeight => 4;
//    public override byte BitsPerColor => 16;
//    public override TextureFormat Format => TextureFormat.IA8;


//    public override Block ReadBlock(EndianBinaryReader reader)
//    {
//        var block = new DirectBlock(BlockWidth, BlockHeight, Format);
//        for (int y = 0; y < block.Height; y++)
//        {
//            for (int x = 0; x < block.Width; x++)
//            {
//                ushort ia8 = reader.ReadUInt16();
//                var color = TextureColor.FromIA8(ia8);
//                int index = x + (y * block.Width);
//                block[index] = color;
//            }
//        }
//        return block;
//    }

//    public override void WriteBlock(EndianBinaryWriter writer, Block block)
//    {
//        var colorBlock = block as DirectBlock;
//        for (int y = 0; y < block.Height; y++)
//        {
//            for (int x = 0; x < block.Width; x++)
//            {
//                int index = x + (y * block.Width);
//                var color = colorBlock[index];
//                ushort ia8 = TextureColor.ToIA8(color);
//                writer.Write(ia8);
//            }
//        }
//    }

//}
