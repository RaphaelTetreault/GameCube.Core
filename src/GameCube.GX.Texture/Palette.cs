using Manifold.IO;
using System;
using System.Collections.Immutable;

namespace GameCube.GX.Texture;

/// <summary>
///     Representation for a GameCube indexed-color palette.
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
    ///     The colors used by this palette.
    /// </summary>
    public required ImmutableArray<TexturePixel> Colors { get; init; }

    /// <summary>
    ///     Indexer to get palette color.
    /// </summary>
    /// <param name="i">Index into palette's colors.</param>
    public TexturePixel this[int i]
    {
        get => Colors[i];
    }


    internal Palette() { }

    /// <param name="colorFormat">The color format for this palette's colors.</param>
    /// <param name="indirectEncoding">The indirect encoding used to index into this palette.</param>
    /// <param name="paletteValues">The palette's colors.</param>
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public Palette(PaletteColorFormat colorFormat, IndirectEncoding indirectEncoding, ReadOnlySpan<TexturePixel> paletteValues)
    {
        // Assign values
        ColorFormat = colorFormat;
        IndexFormat = indirectEncoding.IndirectFormat;
        Colors = ImmutableArray.Create(paletteValues);
        // Assert everything
        AssertPalette(this, indirectEncoding);
    }


    /// <summary>
    ///     Read a new palette from <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The binary reader to read from.</param>
    /// <param name="colorFormat">Format of palette colors.</param>
    /// <param name="indexFormat">Format of indexes into palette.</param>
    /// <returns>
    ///     New palette constrcuted from <paramref name="reader"/> using the specified
    ///     <paramref name="colorFormat"/> and <paramref name="indexFormat"/>.
    /// </returns>
    public static Palette Read(EndianBinaryReader reader, PaletteColorFormat colorFormat, IndirectTextureFormat indexFormat)
    {
        IndirectEncoding indirectEncoding = IndirectEncoding.MapIndirectFormatToEncoding[indexFormat];
        ReadPalette readPalette = MapColorFormatToRead[colorFormat];
        Palette palette = readPalette.Invoke(reader, indirectEncoding);
        return palette;
    }

    /// <summary>
    ///     Write an existing <paramref name="palette"/> to <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer">The binary writer to write to.</param>
    /// <param name="palette">The palette to write.</param>
    public static void Write(EndianBinaryWriter writer, Palette palette)
    {
        IndirectEncoding indirectEncoding = IndirectEncoding.MapIndirectFormatToEncoding[palette.IndexFormat];
        WritePalette writePalette = MapColorFormatToWrite[palette.ColorFormat];
        writePalette.Invoke(writer, indirectEncoding, palette);
    }

    [System.Diagnostics.Conditional("DEBUG")]
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
        TexturePixel[] colors = new TexturePixel[indirectEncoding.MaxPaletteSize];
        for (int i = 0; i < colors.Length; i++)
        {
            ushort ia8 = reader.ReadUInt16();
            colors[i] = TexturePixel.FromIA8(ia8);
        }
        // Construct palette
        Palette palette = new()
        {
            ColorFormat = PaletteColorFormat.IA8,
            IndexFormat = indirectEncoding.IndirectFormat,
            Colors = ImmutableArray.Create(colors),
        };
        AssertPalette(palette, indirectEncoding);
        return palette;
    }

    internal static void WriteIA8(EndianBinaryWriter writer, IndirectEncoding indirectEncoding, Palette palette)
    {
        // Write palette
        for (int i = 0; i < palette.Colors.Length; i++)
        {
            ushort ia8 = TexturePixel.ToIA8(palette.Colors[i]);
            writer.Write(ia8);
        }
    }

    internal static Palette ReadRGB565(EndianBinaryReader reader, IndirectEncoding indirectEncoding)
    {
        // Read palette colors
        TexturePixel[] colors = new TexturePixel[indirectEncoding.MaxPaletteSize];
        for (int i = 0; i < colors.Length; i++)
        {
            ushort rgb565 = reader.ReadUInt16();
            colors[i] = TexturePixel.FromRGB565(rgb565);
        }
        // Construct palette
        Palette palette = new()
        {
            ColorFormat = PaletteColorFormat.RGB565,
            IndexFormat = indirectEncoding.IndirectFormat,
            Colors = ImmutableArray.Create(colors),
        };
        AssertPalette(palette, indirectEncoding);
        return palette;
    }

    internal static void WriteRGB565(EndianBinaryWriter writer, IndirectEncoding indirectEncoding, Palette palette)
    {
        // Write palette
        for (int i = 0; i < palette.Colors.Length; i++)
        {
            ushort rgb565 = TexturePixel.ToRGB565(palette.Colors[i]);
            writer.Write(rgb565);
        }
    }

    internal static Palette ReadRGB5A3(EndianBinaryReader reader, IndirectEncoding indirectEncoding)
    {
        // Read palette colors
        TexturePixel[] colors = new TexturePixel[indirectEncoding.MaxPaletteSize];
        for (int i = 0; i < colors.Length; i++)
        {
            ushort rgb5a3 = reader.ReadUInt16();
            colors[i] = TexturePixel.FromRGB5A3(rgb5a3);
        }
        // Construct palette
        Palette palette = new()
        {
            ColorFormat = PaletteColorFormat.RGB5A3,
            IndexFormat = indirectEncoding.IndirectFormat,
            Colors = ImmutableArray.Create(colors),
        };
        AssertPalette(palette, indirectEncoding);
        return palette;
    }

    internal static void WriteRGB5A3(EndianBinaryWriter writer, IndirectEncoding indirectEncoding, Palette palette)
    {
        // Write palette
        for (int i = 0; i < palette.Colors.Length; i++)
        {
            ushort rgb5a3 = TexturePixel.ToRGB5A3(palette.Colors[i]);
            writer.Write(rgb5a3);
        }
    }

    internal static Palette ReadRGBA8(EndianBinaryReader reader, IndirectEncoding indirectEncoding)
    {
        // Read palette colors
        TexturePixel[] colors = new TexturePixel[indirectEncoding.MaxPaletteSize];
        for (int i = 0; i < colors.Length; i++)
        {
            uint rgba8 = reader.ReadUInt32();
            colors[i] = new TexturePixel(rgba8);
        }
        // Construct palette
        Palette palette = new()
        {
            ColorFormat = PaletteColorFormat.RGBA8,
            IndexFormat = indirectEncoding.IndirectFormat,
            Colors = ImmutableArray.Create(colors),
        };
        AssertPalette(palette, indirectEncoding);
        return palette;
    }

    internal static void WriteRGBA8(EndianBinaryWriter writer, IndirectEncoding indirectEncoding, Palette palette)
    {
        // Write palette
        for (int i = 0; i < palette.Colors.Length; i++)
        {
            uint rgba8 = palette.Colors[i].raw;
            writer.Write(rgba8);
        }
    }

    /// <summary>
    ///     Map of <see cref="PaletteColorFormat"/> to <see cref="ReadPalette"/> function.
    /// </summary>
    public static readonly ImmutableDictionary<PaletteColorFormat, ReadPalette> MapColorFormatToRead =
    [
        new(PaletteColorFormat.IA8, ReadIA8),
        new(PaletteColorFormat.RGB565, ReadRGB565),
        new(PaletteColorFormat.RGB5A3, ReadRGB5A3),
        new(PaletteColorFormat.RGBA8, ReadRGBA8),
    ];

    /// <summary>
    ///     Map of <see cref="PaletteColorFormat"/> to <see cref="WritePalette"/> function.
    /// </summary>
    public static readonly ImmutableDictionary<PaletteColorFormat, WritePalette> MapColorFormatToWrite =
    [
        new(PaletteColorFormat.IA8, WriteIA8),
        new(PaletteColorFormat.RGB565, WriteRGB565),
        new(PaletteColorFormat.RGB5A3, WriteRGB5A3),
        new(PaletteColorFormat.RGBA8, WriteRGBA8),
    ];
}