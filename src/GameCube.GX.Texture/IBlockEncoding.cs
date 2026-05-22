using System.Collections.Immutable;

namespace GameCube.GX.Texture;

public interface IBlockEncoding
{
    public byte BlockWidth { get; }
    public byte BlockHeight { get; }

    public static readonly ImmutableDictionary<TextureFormat, IBlockEncoding> MapTextureFormatToBlockEncoding =
    [
        // DIRECT ENCODINGS
        new(TextureFormat.I4, DirectEncoding.I4),
        new(TextureFormat.I8, DirectEncoding.I8),
        new(TextureFormat.IA4, DirectEncoding.IA4),
        new(TextureFormat.IA8, DirectEncoding.IA8),
        new(TextureFormat.RGB565, DirectEncoding.RGB565),
        new(TextureFormat.RGB5A3, DirectEncoding.RGB5A3),
        new(TextureFormat.RGBA8, DirectEncoding.RGBA8),
        new(TextureFormat.CMPR, DirectEncoding.CMPR),
        // INDIRECT ENCODINGS
        new(TextureFormat.CI4, IndirectEncoding.CI4),
        new(TextureFormat.CI8, IndirectEncoding.CI8),
        new(TextureFormat.CI14X2, IndirectEncoding.CI14X2),
    ];
}
