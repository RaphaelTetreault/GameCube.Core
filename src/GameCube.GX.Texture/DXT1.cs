using System;
using System.Numerics;

namespace GameCube.GX.Texture;

/// <summary>
///     Utility class for DXT1 / Block Compression 1 algorithm.
/// </summary>
public static class DXT1
{
    public static void MinMaxFitColors(in TextureColor[] pixelsBlock4x4, out ushort c0, out ushort c1, out uint packedIndexes)
    {
        bool isValid4x4Block = pixelsBlock4x4.Length == 4 * 4;
        if (!isValid4x4Block)
        {
            string msg = $"Argument {nameof(pixelsBlock4x4)}.Length is not exactly 16 (4x4).";
            throw new ArgumentException(msg);
        }

        // TODO: pass in function to get colour endpoints. This is a quick and dirty temp solution.
        // Compute endpoints using min/max color components.
        TextureColor min = new TextureColor(0xFFFFFFFF); // default is upper limit
        TextureColor max = new TextureColor(0x00000000); // default is lower limit
        foreach (TextureColor pixel in pixelsBlock4x4)
        {
            // Get minimum color of each component
            min.r = Math.Min(pixel.r, min.r);
            min.g = Math.Min(pixel.g, min.g);
            min.b = Math.Min(pixel.b, min.b);
            // Get maximum color of each component
            max.r = Math.Max(pixel.r, max.r);
            max.g = Math.Max(pixel.g, max.g);
            max.b = Math.Max(pixel.b, max.b);
        }

        ushort minRgb565 = TextureColor.ToRGB565(min);
        ushort maxRgb565 = TextureColor.ToRGB565(max);
        bool hasAlpha = ContainsTranslucidPixels(pixelsBlock4x4, 128);
        // c0 < c1 IF has alpha
        c0 = hasAlpha ? minRgb565 : maxRgb565;
        c1 = hasAlpha ? maxRgb565 : minRgb565;
        TextureColor[] colorPalette = EncodingCMPR.GetCmprPalette(c0, c1);

        // Convert color palette into Vector4 space
        Vector4[] vector4Palette = new Vector4[colorPalette.Length];
        for (int i = 0; i < vector4Palette.Length; i++)
            vector4Palette[i] = ColorToVector4(colorPalette[i]);

        // Get each pixel's nearest color value in palette
        byte[] unpackedIndexes = new byte[4 * 4];
        for (int i = 0; i < unpackedIndexes.Length; i++)
        {
            TextureColor color = pixelsBlock4x4[i];
            byte closestColorIndex = GetClosestIndex(color, vector4Palette);
            unpackedIndexes[i] = closestColorIndex;
        }

        packedIndexes = EncodingCMPR.PackIndexes(unpackedIndexes);

        // We're done! We have:
        // c0 and c1, properly ordered for DXT1 alpha lerp
        // packed indexes for palette lookup
    }

    /// <summary>
    ///     Check to see if any pixel in <paramref name="pixels"/> contains alpha lower than <paramref name="alphaThreshold"/>.
    /// </summary>
    /// <param name="pixels">The pixels to check against for alpha.</param>
    /// <param name="alphaThreshold">The maximum alpha value not considered to be translucid.</param>
    /// <returns>
    ///     True when any <paramref name="pixels"/>' alpha component is less than <paramref name="alphaThreshold"/>.
    /// </returns>
    private static bool ContainsTranslucidPixels(TextureColor[] pixels, byte alphaThreshold)
    {
        foreach (TextureColor pixel in pixels)
        {
            if (pixel.a < alphaThreshold)
                return true;
        }
        return false;
    }

    /// <summary>
    ///     Get the index of the closest colour to <paramref name="color"/> within <paramref name="palette"/>.
    /// </summary>
    /// <param name="color">The colour to compare againts the palette.</param>
    /// <param name="palette">The colour palette used to represent colours.</param>
    /// <returns>
    ///     The index of the neared colour in <paramref name="palette"/> when representing
    ///     <paramref name="color"/> as a 4-dimensional vector.
    /// </returns>
    private static byte GetClosestIndex(TextureColor color, Vector4[] palette)
    {
        byte closesIndex = 0;
        float closestDistance = float.PositiveInfinity;
        Vector4 colorV4 = ColorToVector4(color);

        //
        for (byte i = 0; i < palette.Length; i++)
        {
            Vector4 paletteColor = palette[i];
            float distance = Vector4.Distance(colorV4, paletteColor);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closesIndex = i;
            }
        }
        return closesIndex;
    }

    private static Vector4 ColorToVector4(TextureColor color)
    {
        const float InvMax = 1f / 255f;
        Vector4 colorV4 = new(
            color.r * InvMax,
            color.g * InvMax,
            color.b * InvMax,
            color.a * InvMax);
        return colorV4;
    }

}
