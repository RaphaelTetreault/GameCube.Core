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

        /// <summary>
        ///     
        /// </summary>
        /// <returns>
        ///     
        /// </returns>
        public DirectTextureFormat AsDirectTextureFormat()
        {
            DirectTextureFormat directTextureFormat = (DirectTextureFormat)textureFormat;
            directTextureFormat.Validate();
            return directTextureFormat;
        }

        /// <summary>
        ///     
        /// </summary>
        /// <returns>
        ///     
        /// </returns>
        public IndirectTextureFormat AsIndirectTextureFormat()
        {
            IndirectTextureFormat indirectTextureFormat = (IndirectTextureFormat)textureFormat;
            indirectTextureFormat.Validate();
            return indirectTextureFormat;
        }

        /// <summary>
        ///     
        /// </summary>
        /// <returns>
        ///     
        /// </returns>
        public PaletteColorFormat AsPaletteColorFormat()
        {
            PaletteColorFormat paletteColorFormat = (PaletteColorFormat)textureFormat;
            paletteColorFormat.Validate();
            return paletteColorFormat;
        }

        /// <summary>
        ///     Ensure this enum has a valid value.
        /// </summary>
        /// <exception cref="System.ArgumentException">
        ///     Thrown if enum value is not defined.
        /// </exception>
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
