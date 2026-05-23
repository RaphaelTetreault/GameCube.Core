namespace GameCube.GX.Texture;

public interface IBlockEncoding
{
    /// <summary>
    ///     Bleck width in pixels.
    /// </summary>
    public byte BlockPixelWidth { get; }

    /// <summary>
    ///     Bleck height in pixels.
    /// </summary>
    public byte BlockPixelHeight { get; }
}
