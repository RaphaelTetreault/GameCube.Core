// TODO: implement own quantization
// https://en.wikipedia.org/wiki/Median_cut
// https://en.wikipedia.org/wiki/K-means_clustering
// And consider where dithering fits in?
// https://en.wikipedia.org/wiki/Dither
// https://en.wikipedia.org/wiki/Floyd%E2%80%93Steinberg_dithering
// https://tannerhelland.com/2012/12/28/dithering-eleven-algorithms-source-code.html

// TODO: Implement palettes of all other direct color formats.

using Manifold.IO;
using System;
using SixLabors.ImageSharp.PixelFormats;

namespace GameCube.GX.Texture;

/// <summary>
///     The base representation for a GameCube indexed-colour palette.
/// </summary>
public class Palette
{
    public delegate Palette ReadPalette(EndianBinaryReader reader, IndirectEncoding indirectEncoding);
    public delegate void WritePalette(EndianBinaryWriter writer, IndirectEncoding indirectEncoding, Palette palette);

    /// <summary>
    ///     The texture format used by this palette.
    /// </summary>
    public DirectEncoding DirectEncoding { get; init; }

    /// <summary>
    ///     The colours used by this palette.
    /// </summary>
    public TextureColor[] Colors { get; set; }

    public ReadPalette Read { get; init; }

    public WritePalette Write { get; init; }


    public Palette(DirectEncoding directEncoding, TextureColor[] paletteColors)
    {
        // Validate and assign encoding
        directEncoding.Format.Validate();
        DirectEncoding = directEncoding;

        //// Make sure pixels map to encoding
        //if (paletteColors.Length != directEncoding.PixelsPerBlock)
        //{
        //    //string msg = $"{nameof(Palette)} encoding of {directEncoding.DirectFormat} defines " +
        //    //    $"{paletteColors.PixelsPerBlock} pixels but an array of {paletteColors.Length} " +
        //    //    $"{nameof(TextureColor)} was passed to be assigned.";
        //    string msg = $"";
        //    throw new System.ArgumentException(msg);
        //}
        Colors = paletteColors;
    }


    /// <summary>
    ///     Set the palette's colors to <paramref name="colors"/>.
    /// </summary>
    /// <remarks>
    ///     The 
    /// </remarks>
    /// <param name="colors"></param>
    public void SetColors(Rgba32[] colors)
    {
        Colors = new TextureColor[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            Colors[i] = new TextureColor(colors[i].PackedValue);
        }
    }

    internal static void AssertPalette(Palette palette, IndirectEncoding indirectEncoding)
    {
        if (palette.Colors.Length != indirectEncoding.MaxPaletteSize)
        {
            string msg = $"";
            throw new ArgumentException(msg);
        }
    }

    internal static Palette ReadIA8(EndianBinaryReader reader, IndirectEncoding indirectEncoding)
    {
        TextureColor[] colors = new TextureColor[indirectEncoding.MaxPaletteSize];
        for (int i = 0; i < colors.Length; i++)
        {
            ushort ia8 = reader.ReadUInt16();
            colors[i] = TextureColor.FromIA8(ia8);
        }
        Palette palette = new(DirectEncodingDB.IA8, colors);
        return palette;
    }

    internal static void WriteIA8(EndianBinaryWriter writer, IndirectEncoding indirectEncoding, Palette palette)
    {
        AssertPalette(palette, indirectEncoding);
        for (int i = 0; i < palette.Colors.Length; i++)
        {
            ushort ia8 = TextureColor.ToIA8(palette.Colors[i]);
            writer.Write(ia8);
        }
    }

    internal static Palette ReadRGB565(EndianBinaryReader reader, IndirectEncoding indirectEncoding)
    {
        TextureColor[] colors = new TextureColor[indirectEncoding.MaxPaletteSize];
        for (int i = 0; i < colors.Length; i++)
        {
            ushort rgb565 = reader.ReadUInt16();
            colors[i] = TextureColor.FromRGB565(rgb565);
        }
        Palette palette = new(DirectEncodingDB.RGB565, colors);
        return palette;
    }

    internal static void WriteRGB565(EndianBinaryWriter writer, IndirectEncoding indirectEncoding, Palette palette)
    {
        AssertPalette(palette, indirectEncoding);
        for (int i = 0; i < palette.Colors.Length; i++)
        {
            ushort rgb565 = TextureColor.ToRGB565(palette.Colors[i]);
            writer.Write(rgb565);
        }
    }

    internal static Palette ReadRGB5A3(EndianBinaryReader reader, IndirectEncoding indirectEncoding)
    {
        TextureColor[] colors = new TextureColor[indirectEncoding.MaxPaletteSize];
        for (int i = 0; i < colors.Length; i++)
        {
            ushort rgb5a3 = reader.ReadUInt16();
            colors[i] = TextureColor.FromRGB5A3(rgb5a3);
        }
        Palette palette = new(DirectEncodingDB.RGB5A3, colors);
        return palette;
    }

    internal static void WriteRGB5A3(EndianBinaryWriter writer, IndirectEncoding indirectEncoding, Palette palette)
    {
        AssertPalette(palette, indirectEncoding);
        for (int i = 0; i < palette.Colors.Length; i++)
        {
            ushort rgb5a3 = TextureColor.ToRGB5A3(palette.Colors[i]);
            writer.Write(rgb5a3);
        }
    }

}

public static class PaletteDB
{
    //public static readonly Palette PaletteIA8 = new()
    //{
    //    DirectEncoding = DirectEncodingDB.IA8,
    //    Read = Palette.ReadIA8,
    //    Write = Palette.WriteIA8,
    //};

}