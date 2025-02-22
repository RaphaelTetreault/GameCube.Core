using System;

namespace GameCube.GX.Texture;

/// <summary>
///     The base representation of a GameCube indirect-colour texture format encoding.
/// </summary>
public abstract class IndirectEncoding : Encoding
{
    /// <summary>
    ///     The number of bits used by this encoding to represent a single colour index.
    /// </summary>
    public abstract byte BitsPerIndex { get; }

    /// <summary>
    ///     The number of bytes used by this encoding to represent a single colour index.
    /// </summary>
    /// <remarks>
    ///     Will return 0 for encodings with less than 8 bits per index.
    /// </remarks>
    public int BytesPerIndex => BitsPerIndex / 8;

    /// <summary>
    ///     The maximum number of colours that can be represented using this encoding.
    /// </summary>
    public abstract ushort MaxPaletteSize { get; }

    public override bool IsDirect => false;
    public override bool IsIndirect => true;
    public override int BytesPerBlock => (int)MathF.Ceiling(BitsPerIndex / 8f * BlockWidth * BlockHeight);

    new public static IndirectEncoding GetEncoding(TextureFormat textureFormat)
    {
        switch (textureFormat)
        {
            case TextureFormat.CI4: return EncodingCI4;
            case TextureFormat.CI8: return EncodingCI8;
            case TextureFormat.CI14X2: return EncodingCI14X2;

            default:
                string msg = $"Requested texture format {textureFormat} is not a {nameof(IndirectEncoding)} type.";
                throw new System.Exception(msg);
        }
    }
}
