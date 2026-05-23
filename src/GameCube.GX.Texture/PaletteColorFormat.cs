namespace GameCube.GX.Texture;

public enum PaletteColorFormat : byte
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

public static class PaletteColorFormatExtensions
{
    extension(PaletteColorFormat paletteColorFormat)
    {
        public byte Byte => (byte)paletteColorFormat;

        /// <summary>
        ///     
        /// </summary>
        /// <returns>
        ///     
        /// </returns>
        public TextureFormat AsTextureFormat()
        {
            TextureFormat textureFormat = (TextureFormat)paletteColorFormat;
            textureFormat.Validate();
            return textureFormat;
        }

        /// <summary>
        ///     Ensure this enum has a valid value.
        /// </summary>
        /// <exception cref="System.ArgumentException">
        ///     Thrown if enum value is not defined.
        /// </exception>
        [System.Diagnostics.Conditional("DEBUG")]
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
