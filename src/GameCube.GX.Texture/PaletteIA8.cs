using Manifold.IO;

namespace GameCube.GX.Texture;

/// <summary>
///     Palette for '8-bit intensity and 8-bit alpha' indirect colour textures.
/// </summary>
public sealed class PaletteIA8 : Palette
{
    public override TextureFormat ColorFormat => TextureFormat.IA8;


    public override void ReadPalette(EndianBinaryReader reader, IndirectEncoding indirectEncoding)
    {
        Colors = new TextureColor[indirectEncoding.MaxPaletteSize];
        for (int i = 0; i < Colors.Length; i++)
        {
            ushort ia8 = reader.ReadUInt16();
            Colors[i] = TextureColor.FromIA8(ia8);
        }
    }

    public override void WritePalette(EndianBinaryWriter writer, IndirectEncoding indirectEncoding)
    {
        Assert.IsTrue(Colors.Length == indirectEncoding.MaxPaletteSize);

        for (int i = 0; i < Colors.Length; i++)
        {
            ushort ia8 = TextureColor.ToIA8(Colors[i]);
            writer.Write(ia8);
        }
    }
}
