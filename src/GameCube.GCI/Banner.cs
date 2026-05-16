using GameCube.GX.Texture;
using Manifold.IO;
using System;

namespace GameCube.GCI;

/// <summary>
///     GameCube GCI Banner.
/// </summary>
public class Banner
{
    // CONSTANTS
    public const int BannerWidth = 96;
    public const int BannerHeight = 32;
    public const DirectTextureFormat DirectColorFormat = DirectTextureFormat.RGB5A3;
    public const IndirectTextureFormat IndirectIndexFormat = IndirectTextureFormat.CI8;
    public const PaletteColorFormat PaletteColorFormat = PaletteColorFormat.RGB5A3;

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
            texture = Texture.ReadDirectColorTexture(reader, DirectColorFormat, BannerWidth, BannerHeight);
        }
        // INDIRECT COLOR
        else if (format == GciTextureFormat.IndirectColor)
        {
            Palette palette = Palette.Read(reader, PaletteColorFormat, IndirectIndexFormat);
            texture = Texture.ReadIndirectColorTexture(reader, IndirectIndexFormat, palette, BannerWidth, BannerHeight);
        }

        ValidateTexture();
    }

    public void WriteBanner(EndianBinaryWriter writer)
    {
        ValidateTexture();

        // DIRECT COLOR
        if (format == GciTextureFormat.DirectColor)
        {
            Texture.WriteDirectColorTexture(writer, texture, DirectColorFormat);
        }
        // INDIRECT COLOR
        else if (format == GciTextureFormat.IndirectColor)
        {
            Texture.WriteIndirectColorTexture(writer, texture, IndirectIndexFormat, PaletteColorFormat);
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
