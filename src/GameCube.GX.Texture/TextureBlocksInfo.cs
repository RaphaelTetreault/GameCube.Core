using System;

namespace GameCube.GX.Texture;

/// <summary>
///     Texture information when stored as encoded blocks for GameCube GX GPU.
/// </summary>
public readonly record struct TextureBlocksInfo
{
    /// <summary>
    ///     Number of blocks along X-axis (width, columns).
    /// </summary>
    public int BlockCountX { get; init; }

    /// <summary>
    ///     Number of blocks along Y-axis (height, rows).
    /// </summary>
    public int BlockCountY { get; init; }

    /// <summary>
    ///     Total number of blocks (X by Y).
    /// </summary>
    public int BlockCount { get; init; }

    /// <summary>
    ///     Number of pixels inside block along X-axis.
    /// </summary>
    public int BlockPixelWidth { get; init; }

    /// <summary>
    ///     Number of pixels inside block along Y-axis.
    /// </summary>
    public int BlockPixelHeight { get; init; }

    /// <summary>
    ///     Total number of pixels inside block (X by Y).
    /// </summary>
    public int BlockPixelCount { get; init; }

    /// <summary>
    ///     Number of texture pixels along X-axis.
    ///     Can be less than <see cref="BlockPixelWidth"/> for small textures.
    /// </summary>
    public int TexturePixelWidth { get; init; }

    /// <summary>
    ///     Number of texture pixels along Y-axis.
    ///     Can be less than <see cref="BlockPixelHeight"/> for small textures.
    /// </summary>
    public int TexturePixelHeight { get; init; }

    /// <summary>
    ///     Total number of texture pixels (X by Y).
    ///     Can be less than <see cref="BlockPixelCount"/> for small textures.
    /// </summary>
    public int TexturePixelCount { get; init; }

    /// <summary>
    ///     Create block information from texture size.
    /// </summary>
    /// <param name="pxWidth">Texture width in pixels.</param>
    /// <param name="pxHeight">Texture height in pixels.</param>
    /// <param name="encoding">Block encoding.</param>
    /// <returns>
    ///     Block information for texture of <paramref name="pxWidth"/> and 
    ///     <paramref name="pxHeight"/> using <paramref name="encoding"/>.
    /// </returns>
    public static TextureBlocksInfo FromPixelDimensions(int pxWidth, int pxHeight, IBlockEncoding encoding)
    {
        // Compute how many blocks needed for texture dimensions
        int blocksCountX = (int)MathF.Ceiling((float)pxWidth / encoding.BlockPixelWidth);
        int blocksCountY = (int)MathF.Ceiling((float)pxHeight / encoding.BlockPixelHeight);
        int blocksCount = blocksCountX * blocksCountY;
        // Compute number of pixels the above blocks store
        int blocksPixelCount = blocksCount * encoding.BlockPixelWidth * encoding.BlockPixelHeight;
        // Compute total pixels texture stores - can be less than above count for small textures.
        int pxCount = pxWidth * pxHeight;

        TextureBlocksInfo blockSize = new()
        {
            BlockCountX = blocksCountX,
            BlockCountY = blocksCountY,
            BlockCount = blocksCount,
            BlockPixelWidth = encoding.BlockPixelWidth,
            BlockPixelHeight = encoding.BlockPixelHeight,
            BlockPixelCount = blocksPixelCount,
            TexturePixelWidth = pxWidth,
            TexturePixelHeight = pxHeight,
            TexturePixelCount = pxCount,
        };
        return blockSize;
    }
}
