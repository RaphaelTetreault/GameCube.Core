// TODO: implement own quantization
// https://en.wikipedia.org/wiki/Median_cut
// https://en.wikipedia.org/wiki/K-means_clustering
// And consider where dithering fits in?
// https://en.wikipedia.org/wiki/Dither
// https://en.wikipedia.org/wiki/Floyd%E2%80%93Steinberg_dithering
// https://tannerhelland.com/2012/12/28/dithering-eleven-algorithms-source-code.html

using Manifold.IO;
using System;
using System.Collections.Immutable;

namespace GameCube.GX.Texture;

/// <summary>
///     The base representation for a GameCube indexed-colour palette.
/// </summary>
public record class Palette
{
    public delegate Palette ReadPalette(EndianBinaryReader reader, IndirectEncoding indirectEncoding);
    public delegate void WritePalette(EndianBinaryWriter writer, IndirectEncoding indirectEncoding, Palette palette);

    /// <summary>
    ///     The color format used by this palette.
    /// </summary>
    public required PaletteColorFormat ColorFormat { get; init; }

    /// <summary>
    ///     The texture format used by this palette.
    /// </summary>
    public required IndirectTextureFormat IndexFormat { get; init; }

    /// <summary>
    ///     The colours used by this palette.
    /// </summary>
    public required TextureColor[] Colors { get; init; }

    /// <summary>
    ///     
    /// </summary>
    public required WritePalette Write { get; init; }


    /// <summary>
    ///     
    /// </summary>
    /// <param name="i"></param>
    /// <returns>
    ///     
    /// </returns>
    public TextureColor this[int i] { get => Colors[i]; set => Colors[i] = value; }


    internal Palette() { }

    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public Palette(PaletteColorFormat colorFormat, IndirectEncoding indirectEncoding, TextureColor[] paletteValues)
    {
        // Assign values
        ColorFormat = colorFormat;
        IndexFormat = indirectEncoding.IndirectFormat;
        Colors = paletteValues;
        // Assert everything
        AssertPalette(this, indirectEncoding);
        // Assign write function if we are valid
        Write = MapColorFormatToWrite[colorFormat];
    }

    internal static void AssertPalette(Palette palette, IndirectEncoding indirectEncoding)
    {
        // Ensure palette is in a valid color format
        palette.ColorFormat.Validate();
        indirectEncoding.IndirectFormat.Validate();
        // Ensure palette size and encoding match.
        if (palette.Colors.Length != indirectEncoding.MaxPaletteSize)
        {
            string msg = $"{nameof(Palette)}.{nameof(Colors)} size is {palette.Colors.Length} " +
                $"but should be exactly {indirectEncoding.MaxPaletteSize} to match " +
                $"{nameof(IndirectEncoding)} format {indirectEncoding.IndirectFormat}.";
            throw new ArgumentException(msg);
        }
    }

    internal static Palette ReadIA8(EndianBinaryReader reader, IndirectEncoding indirectEncoding)
    {
        // Read palette colors
        TextureColor[] colors = new TextureColor[indirectEncoding.MaxPaletteSize];
        for (int i = 0; i < colors.Length; i++)
        {
            ushort ia8 = reader.ReadUInt16();
            colors[i] = TextureColor.FromIA8(ia8);
        }
        // Construct palette
        Palette palette = new()
        {
            ColorFormat = PaletteColorFormat.IA8,
            IndexFormat = indirectEncoding.IndirectFormat,
            Colors = colors,
            Write = WriteIA8,
        };
        AssertPalette(palette, indirectEncoding);
        return palette;
    }

    internal static void WriteIA8(EndianBinaryWriter writer, IndirectEncoding indirectEncoding, Palette palette)
    {
        AssertPalette(palette, indirectEncoding);
        // Write palette
        for (int i = 0; i < palette.Colors.Length; i++)
        {
            ushort ia8 = TextureColor.ToIA8(palette.Colors[i]);
            writer.Write(ia8);
        }
    }

    internal static Palette ReadRGB565(EndianBinaryReader reader, IndirectEncoding indirectEncoding)
    {
        // Read palette colors
        TextureColor[] colors = new TextureColor[indirectEncoding.MaxPaletteSize];
        for (int i = 0; i < colors.Length; i++)
        {
            ushort rgb565 = reader.ReadUInt16();
            colors[i] = TextureColor.FromRGB565(rgb565);
        }
        // Construct palette
        Palette palette = new()
        {
            ColorFormat = PaletteColorFormat.RGB565,
            IndexFormat = indirectEncoding.IndirectFormat,
            Colors = colors,
            Write = WriteRGB565,
        };
        AssertPalette(palette, indirectEncoding);
        return palette;
    }

    internal static void WriteRGB565(EndianBinaryWriter writer, IndirectEncoding indirectEncoding, Palette palette)
    {
        AssertPalette(palette, indirectEncoding);
        // Write palette
        for (int i = 0; i < palette.Colors.Length; i++)
        {
            ushort rgb565 = TextureColor.ToRGB565(palette.Colors[i]);
            writer.Write(rgb565);
        }
    }

    internal static Palette ReadRGB5A3(EndianBinaryReader reader, IndirectEncoding indirectEncoding)
    {
        // Read palette colors
        TextureColor[] colors = new TextureColor[indirectEncoding.MaxPaletteSize];
        for (int i = 0; i < colors.Length; i++)
        {
            ushort rgb5a3 = reader.ReadUInt16();
            colors[i] = TextureColor.FromRGB5A3(rgb5a3);
        }
        // Construct palette
        Palette palette = new()
        {
            ColorFormat = PaletteColorFormat.RGB5A3,
            IndexFormat = indirectEncoding.IndirectFormat,
            Colors = colors,
            Write = WriteRGB5A3,
        };
        AssertPalette(palette, indirectEncoding);
        return palette;
    }

    internal static void WriteRGB5A3(EndianBinaryWriter writer, IndirectEncoding indirectEncoding, Palette palette)
    {
        AssertPalette(palette, indirectEncoding);
        // Write palette
        for (int i = 0; i < palette.Colors.Length; i++)
        {
            ushort rgb5a3 = TextureColor.ToRGB5A3(palette.Colors[i]);
            writer.Write(rgb5a3);
        }
    }

    internal static Palette ReadRGBA8(EndianBinaryReader reader, IndirectEncoding indirectEncoding)
    {
        // Read palette colors
        TextureColor[] colors = new TextureColor[indirectEncoding.MaxPaletteSize];
        for (int i = 0; i < colors.Length; i++)
        {
            uint rgba8 = reader.ReadUInt32();
            colors[i] = new TextureColor(rgba8);
        }
        // Construct palette
        Palette palette = new()
        {
            ColorFormat = PaletteColorFormat.RGBA8,
            IndexFormat = indirectEncoding.IndirectFormat,
            Colors = colors,
            Write = WriteRGBA8,
        };
        AssertPalette(palette, indirectEncoding);
        return palette;
    }

    internal static void WriteRGBA8(EndianBinaryWriter writer, IndirectEncoding indirectEncoding, Palette palette)
    {
        AssertPalette(palette, indirectEncoding);
        // Write palette
        for (int i = 0; i < palette.Colors.Length; i++)
        {
            uint rgba8 = palette.Colors[i].raw;
            writer.Write(rgba8);
        }
    }

    public static readonly ImmutableDictionary<PaletteColorFormat, ReadPalette> MapColorFormatToRead =
    [
        new(PaletteColorFormat.IA8, ReadIA8),
        new(PaletteColorFormat.RGB565, ReadRGB565),
        new(PaletteColorFormat.RGB5A3, ReadRGB5A3),
        new(PaletteColorFormat.RGBA8, ReadRGBA8),
    ];

    public static readonly ImmutableDictionary<PaletteColorFormat, WritePalette> MapColorFormatToWrite =
    [
        new(PaletteColorFormat.IA8, WriteIA8),
        new(PaletteColorFormat.RGB565, WriteRGB565),
        new(PaletteColorFormat.RGB5A3, WriteRGB5A3),
        new(PaletteColorFormat.RGBA8, WriteRGBA8),
    ];
}