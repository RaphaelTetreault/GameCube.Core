using Manifold.IO;

namespace GameCube.GX.Texture;

/// <summary>
///     Palette for '5-bit red, 5-bit green, 5-bit blue, and 0-bit alpha'
///     and also '4-bit red, 4-bit green, 4-bit blue, and 3-bit alpha' indirect colour textures.
/// </summary>
public sealed class PaletteRGB5A3 : Palette
{
    public override TextureFormat ColorFormat => TextureFormat.RGB5A3;


    public override void ReadPalette(EndianBinaryReader reader, IndirectEncoding indirectEncoding)
    {
        Colors = new TextureColor[indirectEncoding.MaxPaletteSize];
        for (int i = 0; i < Colors.Length; i++)
        {
            ushort rgb5a3 = reader.ReadUInt16();
            Colors[i] = TextureColor.FromRGB5A3(rgb5a3);
        }
    }

    public override void WritePalette(EndianBinaryWriter writer, IndirectEncoding indirectEncoding)
    {
        Assert.IsTrue(Colors.Length == indirectEncoding.MaxPaletteSize);

        for (int i = 0; i < Colors.Length; i++)
        {
            ushort rgb5a3 = TextureColor.ToRGB5A3(Colors[i]);
            writer.Write(rgb5a3);
        }
    }
}
