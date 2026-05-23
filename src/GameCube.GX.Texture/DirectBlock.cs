using System;
using System.Collections.Immutable;

namespace GameCube.GX.Texture;

/// <summary>
///     A direct color block which directly encodes pixel colors.
/// </summary>
public record class DirectBlock
{
    /// <summary>
    ///     The block's direct encoding.
    /// </summary>
    public readonly DirectEncoding DirectEncoding;

    /// <summary>
    ///     This block's direct colors (pixels).
    /// </summary>
    public readonly ImmutableArray<TexturePixel> Pixels;

    /// <summary>
    ///     Indexer to get direct color (pixel).
    /// </summary>
    /// <param name="i">The pixel's direct color index in this block.</param>
    /// <returns>
    ///     Direct color (pixel) at the specified index within this block.
    /// </returns>
    public TexturePixel this[int i] 
    { 
        get => Pixels[i];
    }

    /// <summary>
    ///     Indexer to get direct color (pixel).
    /// </summary>
    /// <param name="x">The horizontal coordinate of the pixel in this block.</param>
    /// <param name="y">The vertical coordinate of the pixel in this block.</param>
    /// <returns>
    ///     Direct color (pixel) at the specified coordinate within this block.
    /// </returns>
    public TexturePixel this[int x, int y]
    { 
        get
        {
            int index = x + y * DirectEncoding.BlockPixelWidth;
            TexturePixel color = Pixels[index];
            return color;
        }
    }

    /// <summary>
    ///     Construct a new direct color block.
    /// </summary>
    /// <param name="directEncoding">The direct encoding to use for this block.</param>
    public DirectBlock(DirectEncoding directEncoding, TexturePixel[] pixels)
    {
        // Validate and assign encoding
        directEncoding.DirectFormat.Validate();
        DirectEncoding = directEncoding;
        // Validate pixels according to encoding
        AssertNumberOfPixels(directEncoding, pixels.Length);
        Pixels = ImmutableArray.Create(pixels);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    internal static void AssertNumberOfPixels(DirectEncoding directEncoding, int pixelsLength)
    {
        // Assert pixel count
        if (pixelsLength != directEncoding.PixelsPerBlock)
        {
            string msg = $"{nameof(DirectBlock)} encoding of {directEncoding.DirectFormat} " +
                $"defines {directEncoding.PixelsPerBlock} pixels but an array of {pixelsLength}.";
            throw new ArgumentException(msg);
        }
    }
}
