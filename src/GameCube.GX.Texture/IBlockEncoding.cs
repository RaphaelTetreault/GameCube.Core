namespace GameCube.GX.Texture;

/// <summary>
///     Interface for both direct and indirect block encodings.
/// </summary>
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
