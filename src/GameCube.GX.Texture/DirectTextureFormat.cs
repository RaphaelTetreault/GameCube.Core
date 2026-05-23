namespace GameCube.GX.Texture;

/// <summary>
///     Subset of <see cref="TextureFormat"/> for direct color formats only.
/// </summary>
public enum DirectTextureFormat : byte
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

/// <summary>
///     Extensions for <see cref="DirectTextureFormat"/>.
/// </summary>
public static class DirectTextureFormatExtensions
{
    extension(DirectTextureFormat directTextureFormat)
    {
        public byte Byte => (byte)directTextureFormat;

        /// <summary>
        ///     
        /// </summary>
        /// <returns>
        ///     
        /// </returns>
        public TextureFormat AsTextureFormat()
        {
            TextureFormat textureFormat = (TextureFormat)directTextureFormat;
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
            switch (directTextureFormat)
            {
                // Valid direct color formats
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
                        $"The format must be a direct color format.";
                    throw new System.ArgumentException(msg);
            }
        }
    }
}
