using System;

namespace GameCube.GX.Texture;

public readonly record struct BlocksInfo
{
    public int BlockCountX { get; init; }
    public int BlockCountY { get; init; }
    public int BlockCount { get; init; }
    public int BlockPixelWidth { get; init; }
    public int BlockPixelHeight { get; init; }
    public int TexturePixelWidth { get; init; }
    public int TexturePixelHeight { get; init; }
    public int PixelCount { get; init; }

    public static BlocksInfo FromPixelDimensions(int pxWidth, int pxHeight, IEncoding encoding)
    {
        int blocksCountX = (int)MathF.Ceiling((float)pxWidth / encoding.BlockWidth);
        int blocksCountY = (int)MathF.Ceiling((float)pxHeight / encoding.BlockHeight);
        int blocksCount = blocksCountX * blocksCountY;
        int pxCount = pxWidth * pxHeight;
        BlocksInfo blockSize = new()
        {
            BlockCountX = blocksCountX,
            BlockCountY = blocksCountY,
            BlockCount = blocksCount,
            BlockPixelWidth = encoding.BlockWidth,
            BlockPixelHeight = encoding.BlockHeight,
            TexturePixelWidth = pxWidth,
            TexturePixelHeight = pxHeight,
            PixelCount = pxCount,
        };
        return blockSize;
    }
}
