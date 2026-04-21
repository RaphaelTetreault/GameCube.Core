using System;

namespace GameCube.GX.Texture;

/// <summary>
///     The base representation of a GameCube direct-colour texture format encoding.
/// </summary>
public abstract class DirectEncoding : TextureEncoding
{
    /// <summary>
    ///     The number of bits used by this encoding to represent a single colour.
    /// </summary>
    public abstract byte BitsPerColor { get; }

    /// <summary>
    ///     The number of bytes used by this encoding to represent a single colour.
    /// </summary>
    /// <remarks>
    ///     Will return 0 for encodings with less than 8 bits per colour.
    /// </remarks>
    public int BytesPerPixel { get; private init; }

    public override bool IsDirect => true;
    public override bool IsIndirect => false;
    public override int BytesPerBlock => (int)MathF.Ceiling(BitsPerColor / 8f * BlockWidth * BlockHeight);

    public DirectEncoding()
    {
        // Initialize count
        BytesPerPixel = BitsPerColor / 8;
    }

    new public static DirectEncoding GetEncoding(TextureFormat textureFormat)
    {
        switch (textureFormat)
        {
            case TextureFormat.I4: return EncodingI4;
            case TextureFormat.I8: return EncodingI8;
            case TextureFormat.IA4: return EncodingIA4;
            case TextureFormat.IA8: return EncodingIA8;
            case TextureFormat.RGB565: return EncodingRGB565;
            case TextureFormat.RGB5A3: return EncodingRGB5A3;
            case TextureFormat.RGBA8: return EncodingRGBA8;
            case TextureFormat.CMPR: return EncodingCMPR;

            default:
                string msg = $"Requested texture format {textureFormat} is not a {nameof(DirectEncoding)} type.";
                throw new Exception(msg);
        }
    }
}
