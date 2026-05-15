using System;

namespace GameCube.GX.Texture;

public readonly record struct BlocksInfo(int CountX, int CountY, int Count)
{
    public static BlocksInfo FromPixelDimensions(int pxWidth, int pxHeight, IEncoding encoding)
    {
        int blocksWidth = (int)MathF.Ceiling((float)pxWidth / encoding.BlockWidth);
        int blocksHeight = (int)MathF.Ceiling((float)pxHeight / encoding.BlockHeight);
        int blocksCount = blocksWidth * blocksHeight;
        BlocksInfo blockSize = new()
        {
            CountX = blocksWidth,
            CountY = blocksHeight,
            Count = blocksCount,
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
