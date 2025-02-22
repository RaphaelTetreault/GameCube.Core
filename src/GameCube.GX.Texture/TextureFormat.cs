using System.ComponentModel;

namespace GameCube.GX.Texture;

/// <summary>
///     GameCube GX texture formats.
/// </summary>
/// <remarks>
///     Description attribute contains display name of each texture format.
/// </remarks>
public enum TextureFormat : byte
{
    /// <summary>
    ///     4-bit intensity (grayscale).
    /// </summary>
    [Description("4-bit intensity")]
    I4 = 0,

    /// <summary>
    ///     8-bit intensity (grayscale).
    /// </summary>
    [Description("8-bit intensity")]
    I8 = 1,

    /// <summary>
    ///     4-bit intensity (grayscale) with 4-bit alpha (translucent).
    /// </summary>
    [Description("4-bit intensity and 4-bit alpha")]
    IA4 = 2,

    /// <summary>
    ///     8-bit intensity (grayscale) with 8-bit alpha (translucent).
    /// </summary>
    [Description("8-bit intensity and 8-bit alpha")]
    IA8 = 3,

    /// <summary>
    ///     16-bit color. 5-bit red, 6-bit green, and 5-bit blue components.
    /// </summary>
    [Description("16-bit color")]
    RGB565 = 4,

    /// <summary>
    ///     16-bit color with variable alpha. Either opaque with 5-bit red, 5-bit green, and 5-bit blue
    ///     components, or translucid with 4-bit red, 4-bit green, 4-bit blue, and 3-bit alpha components.
    /// </summary>
    [Description("16-bit color with variable alpha")]
    RGB5A3 = 5,

    /// <summary>
    ///     32-bit color. 8-bit red, 8-bit green, 8-bit blue, and 8-bit alpha components.
    /// </summary>
    [Description("24-bit color and 8-bit alpha")]
    RGBA8 = 6,

    /// <summary>
    ///     4-bit colour index. Accompanies with colour paletted.
    /// </summary>
    [Description("4-bit index into 16 color palette")]
    CI4 = 8,

    /// <summary>
    ///     8-bit colour index. Accompanies with colour paletted.
    /// </summary>
    [Description("8-bit index into 256 color palette")]
    CI8 = 9,

    /// <summary>
    ///     14-bit colour index. Accompanies with colour paletted.
    /// </summary>
    [Description("14-bit index into 16384 color palette")]
    CI14X2 = 10,

    /// <summary>
    ///     16-bit color using Block Compression 1 (BC1) / DXT1 compression algorithm.
    /// </summary>
    [Description("Compressed 16-bit color (DXT1)")]
    CMPR = 14
}
