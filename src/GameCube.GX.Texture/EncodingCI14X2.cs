using Manifold.IO;
using System;

namespace GameCube.GX.Texture;

/// <summary>
///     Encoding format for '14-bit colour-indexed' texture.
/// </summary>
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
        for (int i = 0; i < block.ColorIndexes.Length; i++)
        {
            ushort index16 = reader.ReadUInt16();
            ushort index14 = (ushort)(index16 & 0b_00111111_11111111);
            block.ColorIndexes[i] = index14;
        }
        return block;
    }


    public override void WriteBlock(EndianBinaryWriter writer, Block block)
    {
        var indirectBlock = block as IndirectBlock;
        foreach (var index in indirectBlock.ColorIndexes)
        {
            // Make sure index is 14 bits at most
            bool indexTooLarge = index < (1 << 14);
            if (indexTooLarge)
            {
                string msg = $"Specified index '{index}' is greater than 14 bits.";
                throw new IndexOutOfRangeException(msg);
            }    

            writer.Write(index);
        }
    }
}
