using Manifold.IO;

namespace GameCube.GX.Texture
{
    public sealed class EncodingCI14X2 : IndirectEncoding
    {
        public override byte BlockWidth => 4;
        public override byte BlockHeight => 4;
        public override byte BitsPerIndex => 14;
        public override ushort MaxPaletteSize => 1 << 14;
        public override TextureFormat Format => TextureFormat.CI14X2;


        public override Block ReadBlock(EndianBinaryReader reader)
        {
            var block = new IndirectBlock(BlockWidth, BlockHeight, Format);
            for (int i = 0; i < block.Indexes.Length; i++)
            {
                ushort index16 = reader.ReadUInt16();
                ushort index14 = (ushort)(index16 & 0b_00111111_11111111);
                block.Indexes[i] = index14;
            }
            return block;
        }

        public override void WriteBlock(EndianBinaryWriter writer, Block block)
        {
            var indirectBlock = block as IndirectBlock;
            foreach (var index in indirectBlock.Indexes)
            {
                // Make sure index is 14 bits at most
                Assert.IsTrue(index < (1 << 14));

                writer.Write(index);
            }
        }
    }
}
