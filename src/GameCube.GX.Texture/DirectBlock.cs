using System;
using System.Collections.Immutable;

namespace GameCube.GX.Texture;

/// <summary>
///     A colour block which is directly encoded using a pixel format.
/// </summary>
public record class DirectBlock
{
    /// <summary>
    ///     The block's encoding.
    /// </summary>
    public readonly DirectEncoding DirectEncoding;

    /// <summary>
    ///     This block's direct colours (pixels).
    /// </summary>
    public readonly ImmutableArray<TexturePixel> Colors;

    /// <summary>
    ///     Indexer to get direct colour (pixel).
    /// </summary>
    /// <param name="i">The pixel's direct colour index in this block.</param>
    /// <returns>
    ///     Direct colour (pixel) at the specified index within this block.
    /// </returns>
    public TexturePixel this[int i] 
    { 
        get => Colors[i];
    }

    /// <summary>
    ///     Indexer to get direct colour (pixel).
    /// </summary>
    /// <param name="x">The horizontal coordinate of the pixel in this block.</param>
    /// <param name="y">The vertical coordinate of the pixel in this block.</param>
    /// <returns>
    ///     Direct colour (pixel) at the specified coordinate within this block.
    /// </returns>
    public TexturePixel this[int x, int y]
    { 
        get
        {
            int index = x + y * DirectEncoding.BlockPixelWidth;
            TexturePixel color = Colors[index];
            return color;
        }
    }

    /// <summary>
    ///     Construct a new direct colour block.
    /// </summary>
    /// <param name="directEncoding">The direct encoding to use for this block.</param>
    public DirectBlock(DirectEncoding directEncoding, TexturePixel[] pixels)
    {
        // Validate and assign encoding
        directEncoding.DirectFormat.Validate();
        DirectEncoding = directEncoding;
        // Validate pixels according to encoding
        AssertNumberOfPixels(directEncoding, pixels.Length);
        Colors = ImmutableArray.Create(pixels);
    }

    /// <summary>
    ///     
    /// </summary>
    /// <param name="directEncoding"></param>
    /// <param name="pixelsLength"></param>
    /// <exception cref="ArgumentException">
    ///     
    /// </exception>
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
