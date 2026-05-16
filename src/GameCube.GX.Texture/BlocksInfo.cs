using System;

namespace GameCube.GX.Texture;

public readonly record struct BlocksInfo
{
    public int BlockCountX { get; init; }
    public int BlockCountY { get; init; }
    public int BlockCount { get; init; }
    public int BlockPixelWidth { get; init; }
    public int BlockPixelHeight { get; init; }
    public int PixelCountX { get; init; }
    public int PixelCountY { get; init; }
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
            PixelCountX = pxWidth,
            PixelCountY = pxWidth,
            PixelCount = pxCount,
        };
        return blockSize;
    }

    public static BlocksInfo FromTexture(Texture texture)
    {
        IEncoding encoding = IEncoding.MapFormatToEncoding[texture.Format];
        BlocksInfo blockSize = FromPixelDimensions(texture.Width, texture.Height, encoding);
        return blockSize;
    }
}
