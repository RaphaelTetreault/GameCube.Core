using Manifold.IO;
using System;

namespace GameCube.GX.Texture;

/// <summary>
///     GameCube GCI Banner.
/// </summary>
public class Banner
{
    // CONSTANTS
    const int BannerWidth = 96;
    const int BannerHeight = 32;
    const TextureFormat DirectFormat = TextureFormat.RGB5A3;
    const TextureFormat IndirectFormat = TextureFormat.CI8;

    // FIELDS
    private GciTextureFormat format;
    private Texture texture = new();

    // PROPERTIES
    public GciTextureFormat Format { get => format; set => format = value; }
    public Texture Texture { get => texture; set => texture = value; }

    public void ReadBanner(EndianBinaryReader reader)
    {
        // DIRECT COLOR
        if (format == GciTextureFormat.DirectColor)
        {
            texture = Texture.ReadDirectColorTexture(reader, DirectFormat, BannerWidth, BannerHeight);
        }
        // INDIRECT COLOR
        else if (format == GciTextureFormat.IndirectColor)
        {
            Palette palette = Palette.CreatePalette(DirectFormat);
            palette.ReadPaletteColors(reader, IndirectFormat);
            texture = Texture.ReadIndirectColorTexture(reader, palette, IndirectFormat, BannerWidth, BannerHeight);
        }

        ValidateTexture();
    }

    public void WriteBanner(EndianBinaryWriter writer)
    {
        ValidateTexture();

        // DIRECT COLOR
        if (format == GciTextureFormat.DirectColor)
        {
            Texture.WriteDirectColorTexture(writer, texture, DirectFormat);
        }
        // INDIRECT COLOR
        else if (format == GciTextureFormat.IndirectColor)
        {
            Texture.WriteIndirectColorTexture(writer, texture, IndirectFormat, DirectFormat);
        }
    }

    public void ValidateTexture()
    {
        // Ensure dimensions
        bool hasInvalidWidth = texture.Width != BannerWidth;
        bool hasInvalidHeight = texture.Height != BannerHeight;
        bool hasInvalidDimensions = hasInvalidWidth || hasInvalidHeight;
        if (hasInvalidDimensions)
        {
            string msg =
                $"{nameof(Banner)} has invalid dimensions ({texture.Width},{texture.Height}). " +
                $"{nameof(Banner)} must have a dimension of exactly ({BannerWidth}, {BannerHeight}).";
            throw new Exception(msg);
        }
    }
}
