// TODO: implement own quantization
// https://en.wikipedia.org/wiki/Median_cut
// https://en.wikipedia.org/wiki/K-means_clustering
// And consider where dithering fits in?
// https://en.wikipedia.org/wiki/Dither
// https://en.wikipedia.org/wiki/Floyd%E2%80%93Steinberg_dithering
// https://tannerhelland.com/2012/12/28/dithering-eleven-algorithms-source-code.html

using Manifold.IO;
using System;
using SixLabors.ImageSharp.PixelFormats;

namespace GameCube.GX.Texture;

/// <summary>
///     The base representation for a GameCube indexed-colour palette.
/// </summary>
public abstract class Palette
{
    /// <summary>
    ///     The texture format used by this palette.
    /// </summary>
    public abstract TextureFormat ColorFormat { get; }

    /// <summary>
    ///     The colours used by this palette.
    /// </summary>
    public TextureColor[] Colors { get; set; } = Array.Empty<TextureColor>();


    /// <summary>
    ///     Read a palette using the specified <paramref name="indirectEncoding"/> encoding.
    /// </summary>
    /// <param name="reader">The stream to read the palette from.</param>
    /// <param name="indirectEncoding">The indirect encoding format used to deserialize the target palette.</param>
    public abstract void ReadPalette(EndianBinaryReader reader, IndirectEncoding indirectEncoding);

    /// <summary>
    ///     Write a palette using the specified <paramref name="indirectEncoding"/> encoding.
    /// </summary>
    /// <param name="writer">The stream to write the palette to.</param>
    /// <param name="indirectEncoding">The indirect encoding to use (limits number of indexes).</param>
    public abstract void WritePalette(EndianBinaryWriter writer, IndirectEncoding indirectEncoding);

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



    /// <summary>
    ///     Create new palette instance for the provided <paramref name="textureFormat"/> format.
    /// </summary>
    /// <param name="textureFormat">The texture format of the block.</param>
    /// <returns>
    ///     A texture palette of the specified texture format.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    ///     Thrown if the <paramref name="textureFormat"/> is not supported by the GameCube hardware
    ///     for use as a colour-indexed palette.
    /// </exception>
    public static Palette CreatePalette(TextureFormat textureFormat)
    {
        switch (textureFormat)
        {
            case TextureFormat.IA8: return new PaletteIA8();
            case TextureFormat.RGB565: return new PaletteRGB565();
            case TextureFormat.RGB5A3: return new PaletteRGB5A3();
            default:
                throw new System.ArgumentException($"No palette defined for texture format {textureFormat}.");
        }
    }

    public void ReadPaletteColors(EndianBinaryReader reader, TextureFormat indirectFormat)
    {
        IndirectEncoding indirectEncoding = IndirectEncoding.GetEncoding(indirectFormat);
        ReadPalette(reader, indirectEncoding);
    }
    public void WritePalette(EndianBinaryWriter writer, TextureFormat indirectFormat)
    {
        IndirectEncoding indirectEncoding = IndirectEncoding.GetEncoding(indirectFormat);
        WritePalette(writer, indirectEncoding);
    }
}
