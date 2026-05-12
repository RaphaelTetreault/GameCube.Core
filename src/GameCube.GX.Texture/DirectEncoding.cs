using Manifold.IO;
using System;
using System.Collections.Immutable;

namespace GameCube.GX.Texture;

/// <summary>
///     The base representation of a GameCube direct-colour texture format encoding.
/// </summary>
public record class DirectEncoding 
{
    public delegate DirectBlock ReadDirectBlock(EndianBinaryReader reader);
    public delegate void WriteDirectBlock(EndianBinaryWriter writer, DirectBlock directBlock);


    /// <summary>
    ///     The pixel width of a block for this encoding.
    /// </summary>
    public required byte BlockWidth { get; init; }

    /// <summary>
    ///     The pixel height of a block for this encoding.
    /// </summary>
    public required byte BlockHeight { get; init; }

    /// <summary>
    ///     The texture format used by this encoding.
    /// </summary>
    public required DirectTextureFormat DirectFormat { get; init; }

    /// <summary>
    ///     The number of bits used by this encoding to represent a single colour.
    /// </summary>
    public required byte BitsPerColor { get; init; }

    /// <summary>
    ///     The number of bytes used by this encoding to represent a single colour.
    /// </summary>
    /// <remarks>
    ///     Will return 0 for encodings with less than 8 bits per colour.
    /// </remarks>
    public required byte BytesPerPixel { get; init; } // => BitsPerColor / 8;

    /// <summary>
    ///     The number of bytes used by this encoding per block.
    /// </summary>
    public required byte BytesPerBlock { get; init; } //=> (int)MathF.Ceiling(BitsPerColor / 8f * BlockWidth * BlockHeight);

    /// <summary>
    ///     
    /// </summary>
    public required ReadDirectBlock ReadBlock { get; init; }

    /// <summary>
    ///     
    /// </summary>
    public required WriteDirectBlock WriteBlock { get; init; }

    /// <summary>
    ///     Get <see cref="DirectFormat"/> as <see cref="TextureFormat"/>.
    /// </summary>
    public TextureFormat Format => (TextureFormat)DirectFormat;


    public static DirectBlock ReadI4(EndianBinaryReader reader)
    {
        DirectBlock block = From(DirectBlock.I4);
        for (int y = 0; y < block.Height; y++)
        {
            // Process 2 pixels per pass, high and low nybbles
            for (int x = 0; x < block.Width; x += 2)
            {
                byte nybbles = reader.ReadByte();

                int indexNybbleHigh = x + (y * block.Width);
                int indexNybbleLow = indexNybbleHigh + 1;
                // TODO: move to TextureColor
                byte intentsity0 = (byte)(((nybbles >> 4) & 0b_0000_1111) * ((1 << 4) + 1));
                byte intentsity1 = (byte)(((nybbles >> 0) & 0b_0000_1111) * ((1 << 4) + 1));

                block[indexNybbleHigh] = new TextureColor(intentsity0);
                block[indexNybbleLow] = new TextureColor(intentsity1);
            }
        }
        return block;
    }

    public static void WriteI4(EndianBinaryWriter writer, DirectBlock directBlock)
    {
        for (int y = 0; y < directBlock.Height; y++)
        {
            // Process 2 pixels per pass, set as high and low nybbles
            for (int x = 0; x < directBlock.Width; x += 2)
            {
                int index0 = x + (y * directBlock.Width);
                int index1 = index0 + 1;
                var intensity0 = directBlock[index0].GetIntensity();
                var intensity1 = directBlock[index1].GetIntensity();
                byte intensity01 = (byte)(
                    ((intensity0 >>> 0) & 0b_1111_0000) +
                    ((intensity1 >>> 4) & 0b_0000_1111));
                writer.Write(intensity01);
            }
        }
    }
}



public static class DirectEncodingDB
{
    /// <summary>
    ///     Encoding format for '4-bit intensity' grayscale texture.
    /// </summary>
    public static readonly DirectEncoding I4 = new()
    {
        DirectFormat = TextureFormat.I4,
        BlockWidth = 8,
        BlockHeight = 8,
        BitsPerColor = 4,
        BytesPerBlock = 32, // 8 * 8 * 0.5(4bpp)
        ReadBlock = ,
        WriteBlock = ,
    };

    /// <summary>
    ///     Encoding format for '8-bit intensity' grayscale texture.
    /// </summary>
    public static readonly DirectEncoding I8 = new()
    {
        DirectFormat = TextureFormat.I8,
        BlockWidth = 8,
        BlockHeight = 4,
        BitsPerColor = 8,
        BytesPerBlock = 32, // 8 * 4 * 1(8bpp)
        ReadBlock = ,
        WriteBlock = ,
    };

    /// <summary>
    ///     Encoding format for '4-bit intensity and 4-bit alpha' grayscale texture.
    /// </summary>
    public static readonly DirectEncoding IA4 = new()
    {
        DirectFormat = TextureFormat.IA4,
        BlockWidth = 8,
        BlockHeight = 4,
        BitsPerColor = 8,
        BytesPerBlock = 32, // 8 * 4 * 1(8bpp)
        ReadBlock = ,
        WriteBlock = ,
    };

    /// <summary>
    ///     Encoding format for '8-bit intensity and 8-bit alpha' grayscale texture.
    /// </summary>
    public static readonly DirectEncoding IA8 = new()
    {
        DirectFormat = TextureFormat.IA8,
        BlockWidth = 4,
        BlockHeight = 4,
        BitsPerColor = 16,
        BytesPerBlock = 32, // 4 * 4 * 2(16bpp)
        ReadBlock = ,
        WriteBlock = ,
    };

    /// <summary>
    ///     Encoding format for '5-bit red, 6-bit green, and 5-bit blue' colour texture.
    /// </summary>
    public static readonly DirectEncoding RGB565 = new()
    {
        DirectFormat = TextureFormat.RGB565,
        BlockWidth = 4,
        BlockHeight = 4,
        BitsPerColor = 16,
        BytesPerBlock = 32, // 4 * 4 * 2(16bpp)
        ReadBlock = ,
        WriteBlock = ,
    };

    /// <summary>
    ///     Encoding format for '5-bit red, 5-bit green, 5-bit blue, and 1-bit fixed alpha (1)'
    ///                and also '4-bit red, 4-bit green, 4-bit blue, and 3-bit alpha (4th bit always 0)' colour texture.
    /// </summary>
    public static readonly DirectEncoding RGB5A3 = new()
    {
        DirectFormat = TextureFormat.RGB5A3,
        BlockWidth = 4,
        BlockHeight = 4,
        BitsPerColor = 16,
        BytesPerBlock = 32, // 4 * 4 * 2(16bpp)
        ReadBlock = ,
        WriteBlock = ,
    };

    /// <summary>
    ///     Encoding format for '8-bit red, 8-bit green, 8-bit blue, and 8-bit alpha' colour texture.
    /// </summary>
    public static readonly DirectEncoding RGBA8 = new()
    {
        DirectFormat = TextureFormat.RGBA8,
        BlockWidth = 4,
        BlockHeight = 4,
        BitsPerColor = 32,
        // Big lie: RGBA8 takes 2 4x4 blocks, one is AR then the other GB
        // However, since they are ordered like so, you can treat it like
        // a 4x4 blocks but of size 64 bytes rather than 32 bytes.
        BytesPerBlock = 64, // 4 * 4 * 2(32bpp)
        ReadBlock = ,
        WriteBlock = ,
    };

    public static readonly DirectEncoding CMPR = new()
    {
        DirectFormat = TextureFormat.CMPR,
        BlockWidth = 8,
        BlockHeight = 8,
        BitsPerColor = 4,
        BytesPerBlock = 32, // 8 * 8 * 0.5(4bpp)
        ReadBlock = ,
        WriteBlock = ,
    };

    public static readonly ImmutableDictionary<DirectTextureFormat, DirectEncoding> DirectEncodings =
    [
        new(DirectTextureFormat.I4, I4),
        new(DirectTextureFormat.I8, I8),
        new(DirectTextureFormat.IA4, IA4),
        new(DirectTextureFormat.IA8, IA8),
        new(DirectTextureFormat.RGB565, RGB565),
        new(DirectTextureFormat.RGB5A3, RGB5A3),
        new(DirectTextureFormat.RGBA8, RGBA8),
        new(DirectTextureFormat.CMPR, CMPR),
    ];
}