using System.Runtime.CompilerServices;

namespace GameCube.GX.Texture;

/// <summary>
///     GameCube GX texture formats.
/// </summary>
public enum TextureFormat : byte
{
    /// <summary>
    ///     4-bit intensity (grayscale).
    /// </summary>
    I4 = 0,

    /// <summary>
    ///     8-bit intensity (grayscale).
    /// </summary>
    I8 = 1,

    /// <summary>
    ///     4-bit intensity (grayscale) with 4-bit alpha (translucent).
    /// </summary>
    IA4 = 2,

    /// <summary>
    ///     8-bit intensity (grayscale) with 8-bit alpha (translucent).
    /// </summary>
    IA8 = 3,

    /// <summary>
    ///     16-bit color. 5-bit red, 6-bit green, and 5-bit blue components.
    /// </summary>
    RGB565 = 4,

    /// <summary>
    ///     16-bit color with variable alpha. Either opaque with 5-bit red, 5-bit green, and 5-bit blue
    ///     components, or translucid with 4-bit red, 4-bit green, 4-bit blue, and 3-bit alpha components.
    /// </summary>
    RGB5A3 = 5,

    /// <summary>
    ///     32-bit color. 8-bit red, 8-bit green, 8-bit blue, and 8-bit alpha components.
    /// </summary>
    RGBA8 = 6,

    /// <summary>
    ///     4-bit colour index. Accompanies with colour paletted.
    /// </summary>
    CI4 = 8,

    /// <summary>
    ///     8-bit colour index. Accompanies with colour paletted.
    /// </summary>
    CI8 = 9,

    /// <summary>
    ///     14-bit colour index. Accompanies with colour paletted.
    /// </summary>
    CI14X2 = 10,

    /// <summary>
    ///     16-bit color using Block Compression 1 (BC1) / DXT1 compression algorithm.
    /// </summary>
    CMPR = 14
}

public static class TextureFormatExtensions
{
    extension(TextureFormat textureFormat)
    {
        public byte Byte => (byte)textureFormat;
        public DirectTextureFormat AsDirectTextureFormat()
        {
            DirectTextureFormat directTextureFormat = (DirectTextureFormat)textureFormat;
            directTextureFormat.Validate();
            return directTextureFormat;
        }
        public IndirectTextureFormat AsIndirectTextureFormat()
        {
            IndirectTextureFormat indirectTextureFormat = (IndirectTextureFormat)textureFormat;
            indirectTextureFormat.Validate();
            return indirectTextureFormat;
        }
        public void Validate()
        {
            if (!System.Enum.IsDefined(textureFormat))
            {
                string msg = $"{nameof(TextureFormat)} '{textureFormat}' is not defined.";
                throw new System.ArgumentException(msg);
            }
        }
    }
}

public static class DirectTextureFormatExtensions
{
    extension(DirectTextureFormat directTextureFormat)
    {
        public byte Byte => (byte)directTextureFormat;
        public TextureFormat AsTextureFormat()
        {
            TextureFormat textureFormat = (TextureFormat)directTextureFormat;
            textureFormat.Validate();
            return textureFormat;
        }
        public void Validate()
        {
            switch (directTextureFormat)
            {
                // Valid direct colour formats
                case DirectTextureFormat.CMPR:
                case DirectTextureFormat.I4:
                case DirectTextureFormat.I8:
                case DirectTextureFormat.IA4:
                case DirectTextureFormat.IA8:
                case DirectTextureFormat.RGB565:
                case DirectTextureFormat.RGB5A3:
                case DirectTextureFormat.RGBA8:
                    break;
                // Everything else is invalid
                default:
                    string msg =
                        $"Invalid {nameof(DirectTextureFormat)} '{directTextureFormat}'. " +
                        $"The format must be a direct colour format.";
                    throw new System.ArgumentException(msg);
            }
        }
    }
}

public static class IndirectTextureFormatExtensions
{
    extension(IndirectTextureFormat indirectTextureFormat)
    {
        public byte Byte => (byte)indirectTextureFormat;
        public TextureFormat AsTextureFormat()
        {
            TextureFormat textureFormat = (TextureFormat)indirectTextureFormat;
            textureFormat.Validate();
            return textureFormat;
        }
        public void Validate()
        {
            switch (indirectTextureFormat)
            {
                // Valid indirect colour formats
                case IndirectTextureFormat.CI4:
                case IndirectTextureFormat.CI8:
                case IndirectTextureFormat.CI14X2:
                    break;
                // Everything else is invalid
                default:
                    string msg =
                        $"Invalid {nameof(IndirectTextureFormat)} '{indirectTextureFormat}'. " +
                        $"The format must be an indirect colour format.";
                    throw new System.ArgumentException(msg);
            }
        }
    }
}

public static class PaletteColorFormatExtensions
{
    extension(PaletteColorFormat paletteColorFormat)
    {
        public byte Byte => (byte)paletteColorFormat;
        public TextureFormat AsTextureFormat()
        {
            TextureFormat textureFormat = (TextureFormat)paletteColorFormat;
            textureFormat.Validate();
            return textureFormat;
        }
        public void Validate()
        {
            switch (paletteColorFormat)
            {
                // Valid indirect colour formats
                case PaletteColorFormat.IA8:
                case PaletteColorFormat.RGB565:
                case PaletteColorFormat.RGB5A3:
                case PaletteColorFormat.RGBA8:
                    break;
                // Everything else is invalid
                default:
                    string msg =
                        $"Invalid {nameof(PaletteColorFormat)} '{paletteColorFormat}'. " +
                        $"The format must be an indirect colour format.";
                    throw new System.ArgumentException(msg);
            }
        }
    }
}



public enum DirectTextureFormat
{
    /// <summary>
    ///     4-bit intensity (grayscale).
    /// </summary>
    I4 = 0,

    /// <summary>
    ///     8-bit intensity (grayscale).
    /// </summary>
    I8 = 1,

    /// <summary>
    ///     4-bit intensity (grayscale) with 4-bit alpha (translucent).
    /// </summary>
    IA4 = 2,

    /// <summary>
    ///     8-bit intensity (grayscale) with 8-bit alpha (translucent).
    /// </summary>
    IA8 = 3,

    /// <summary>
    ///     16-bit color. 5-bit red, 6-bit green, and 5-bit blue components.
    /// </summary>
    RGB565 = 4,

    /// <summary>
    ///     16-bit color with variable alpha. Either opaque with 5-bit red, 5-bit green, and 5-bit blue
    ///     components, or translucid with 4-bit red, 4-bit green, 4-bit blue, and 3-bit alpha components.
    /// </summary>
    RGB5A3 = 5,

    /// <summary>
    ///     32-bit color. 8-bit red, 8-bit green, 8-bit blue, and 8-bit alpha components.
    /// </summary>
    RGBA8 = 6,

    /// <summary>
    ///     16-bit color using Block Compression 1 (BC1) / DXT1 compression algorithm.
    /// </summary>
    CMPR = 14
}

public enum IndirectTextureFormat
{
    /// <summary>
    ///     4-bit colour index. Accompanies with colour paletted.
    /// </summary>
    CI4 = 8,

    /// <summary>
    ///     8-bit colour index. Accompanies with colour paletted.
    /// </summary>
    CI8 = 9,

    /// <summary>
    ///     14-bit colour index. Accompanies with colour paletted.
    /// </summary>
    CI14X2 = 10,
}

public enum PaletteColorFormat
{
    /// <summary>
    ///     8-bit intensity (grayscale) with 8-bit alpha (translucent).
    /// </summary>
    IA8 = 3,

    /// <summary>
    ///     16-bit color. 5-bit red, 6-bit green, and 5-bit blue components.
    /// </summary>
    RGB565 = 4,

    /// <summary>
    ///     16-bit color with variable alpha. Either opaque with 5-bit red, 5-bit green, and 5-bit blue
    ///     components, or translucid with 4-bit red, 4-bit green, 4-bit blue, and 3-bit alpha components.
    /// </summary>
    RGB5A3 = 5,

    /// <summary>
    ///     32-bit color. 8-bit red, 8-bit green, 8-bit blue, and 8-bit alpha components.
    /// </summary>
    RGBA8 = 6,
}