using Manifold.IO;

namespace GameCube.GX.Texture;

/// <summary>
///     Encoding format for '4-bit colour-indexed' texture.
/// </summary>
public sealed class TextureEncodingCI4 : IndirectEncoding
{
    public override byte BlockWidth => 8;
    public override byte BlockHeight => 8;
    public override byte BitsPerIndex => 4;
    public override ushort MaxPaletteSize => 1 << 4;
    public override TextureFormat Format => TextureFormat.CI4;


    public override Block ReadBlock(EndianBinaryReader reader)
    {
        var block = new IndirectBlock(BlockWidth, BlockHeight, Format);
        // process 2 indexes at a time
        for (int i = 0; i < block.ColorIndexes.Length; i += 2)
        {
            byte indexes01 = reader.ReadByte();
            byte index0 = (byte)((indexes01 >> 4) & 0b_0000_1111);
            byte index1 = (byte)((indexes01 >> 0) & 0b_0000_1111);
            block.ColorIndexes[i + 0] = index0;
            block.ColorIndexes[i + 1] = index1;
        }
        return block;
    }

    public override void WriteBlock(EndianBinaryWriter writer, Block block)
    {
        var indirectBlock = block as IndirectBlock;
        // Process 2 indexes at a time
        for (int i = 0; i < indirectBlock.ColorIndexes.Length; i += 2)
        {
            byte index0 = checked((byte)(i));
            byte index1 = checked((byte)(index0 + 1));
            byte indexes01 = (byte)(index0 << 4 + index1 << 0);
            writer.Write(indexes01);
        }
    }
}
