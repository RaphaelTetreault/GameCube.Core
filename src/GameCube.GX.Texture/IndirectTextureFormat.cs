namespace GameCube.GX.Texture;

public enum IndirectTextureFormat : byte
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

public static class IndirectTextureFormatExtensions
{
    extension(IndirectTextureFormat indirectTextureFormat)
    {
        public byte Byte => (byte)indirectTextureFormat;

        /// <summary>
        ///     
        /// </summary>
        /// <returns>
        ///     
        /// </returns>
        public TextureFormat AsTextureFormat()
        {
            TextureFormat textureFormat = (TextureFormat)indirectTextureFormat;
            textureFormat.Validate();
            return textureFormat;
        }

        /// <summary>
        ///     Ensure this enum has a valid value.
        /// </summary>
        /// <exception cref="System.ArgumentException">
        ///     Thrown if enum value is not defined.
        /// </exception>
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
