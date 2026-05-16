using GameCube.GX.Texture;
using Manifold.IO;
using System;

namespace GameCube.GCI;

/// <summary>
///     GameCube GCI Icon(s).
/// </summary>
public class Icons
{
    // CONSTANTS
    public const int IconWidth = 32;
    public const int IconHeight = 32;
    public const int MaxIcons = 8;
    public const DirectTextureFormat DirectColorFormat = DirectTextureFormat.RGB5A3;
    public const IndirectTextureFormat IndirectIndexFormat = IndirectTextureFormat.CI8;
    public const PaletteColorFormat PaletteColorFormat = PaletteColorFormat.RGB5A3;

    // FIELDS
    private GciTextureFormat iconFormat;
    private GciPalette iconPalette;
    private Texture[] iconTextures = new Texture[MaxIcons];

    // PROPERTIES
    public GciPalette IconPalette { get => iconPalette; set => iconPalette = value; }
    public GciTextureFormat Format { get => iconFormat; set => iconFormat = value; }
    public Texture[] Textures { get => iconTextures; set => iconTextures = value; }


    public void ReadIcons(EndianBinaryReader reader, int iconCount)
    {
        iconFormat.Validate();
        iconPalette.Validate();

        // DIRECT COLOR
        if (iconFormat == GciTextureFormat.DirectColor)
        {
            for (int i = 0; i < iconCount; i++)
                iconTextures[i] = Texture.ReadDirectColorTexture(reader, DirectColorFormat, IconWidth, IconHeight);
        }
        // INDIRECT COLOR
        else if (iconFormat == GciTextureFormat.IndirectColor)
        {
            // SHARED PALETTE
            if (iconPalette == GciPalette.Shared)
            {
                Palette palette = Palette.Read(reader, PaletteColorFormat, IndirectIndexFormat);
                for (int i = 0; i < iconCount; i++)
                    iconTextures[i] = Texture.ReadIndirectColorTexture(reader, IndirectIndexFormat, palette, IconWidth, IconHeight);
            }
            // UNIQUE PALETTES
            else if (iconPalette == GciPalette.Unique)
            {
                Palette[] palettes = new Palette[iconCount];
                for (int i = 0; i < iconCount; i++)
                {
                    palettes[i] = Palette.Read(reader, PaletteColorFormat, IndirectIndexFormat);
                    iconTextures[i] = Texture.ReadIndirectColorTexture(reader, IndirectIndexFormat, palettes[i], IconWidth, IconHeight);
                }
            }
        }

        Validate();
    }

    public void WriteIcons(EndianBinaryWriter writer)
    {
        iconFormat.Validate();
        iconPalette.Validate();
        Validate();

        int iconCount = CountIcons();

        // DIRECT COLOR
        if (iconFormat == GciTextureFormat.DirectColor)
        {
            for (int i = 0; i < iconCount; i++)
            {
                Texture icon = iconTextures[i];
                Texture.WriteDirectColorTexture(writer, icon, DirectColorFormat);
            }
        }
        // INDIRECT COLOR
        else if (iconFormat == GciTextureFormat.IndirectColor)
        {
            // SHARED PALETTE
            if (iconPalette == GciPalette.Shared)
            {
                // Create a combined texture to derive the palette from
                Texture combinedIcons = new(IconWidth, IconHeight * iconCount);
                for (int i = 0; i < iconCount; i++)
                {
                    int originY = i * IconHeight;
                    Texture icon = iconTextures[i];
                    Texture.Copy(icon, combinedIcons, 0, originY);
                }
                Texture.WriteIndirectColorTexture(writer, combinedIcons, IndirectIndexFormat, PaletteColorFormat);
            }
            // UNIQUE PALETTES
            else if (iconPalette == GciPalette.Unique)
            {
                for (int i = 0; i < iconTextures.Length; i++)
                {
                    Texture icon = iconTextures[i];
                    Texture.WriteIndirectColorTexture(writer, icon, IndirectIndexFormat, PaletteColorFormat);
                }
            }
        }
    }

    public int CountIcons()
    {
        // Count until null is found
        for (int count = 0; count < iconTextures.Length; count++)
            if (iconTextures[count] is null)
                return count;
        // All exists we fall through
        return iconTextures.Length;
    }

    public void Validate()
    {
        // Check to see if new texture is not null after a null texture
        for (int i = 1; i < iconTextures.Length; i++)
        {
            bool lastIsNull = iconTextures[i-1] is null;
            bool currNotNull = iconTextures[i] is not null;
            if (currNotNull && lastIsNull)
            {
                string msg = $"Icon index {i-1} (null) was followed by " +
                    $"icon index {i} which is not null. Make sure the " +
                    $"sequence of icons has no null gap.";
                throw new Exception(msg);
            }
        }

        // Constrain icon count to 8 max
        int iconCount = CountIcons();
        if (iconCount < 1 || MaxIcons < iconCount)
        {
            string msg = $"{nameof(iconCount)} must be in range of 1-{MaxIcons}.";
            throw new Exception(msg);
        }

        // Ensure dimensions
        for (int i = 0; i < iconCount; i++)
        {
            Texture icon = iconTextures[i];
            bool hasInvalidWidth = icon.Width != IconWidth;
            bool hasInvalidHeight = icon.Height != IconHeight;
            bool hasInvalidDimensions = hasInvalidWidth || hasInvalidHeight;
            if (hasInvalidDimensions)
            {
                string msg =
                    $"Icon index {i} has invalid dimensions ({icon.Width},{icon.Height}). " +
                    $"Icon must have a dimension of exactly ({IconWidth}, {IconHeight}).";
                throw new Exception(msg);
            }
        }
    }

}
