using System;
using System.Runtime.InteropServices;

namespace GameCube.GX.Texture;

/// <summary>
///     Represents a pixel's colour within a GameCube texture.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct TextureColor
{
    /// <summary>
    ///     Raw colour in RGBA format.
    /// </summary>
    [FieldOffset(0x00)] public uint raw;
    /// <summary>
    ///     Red component.
    /// </summary>
    [FieldOffset(0x00)] public byte r;
    /// <summary>
    ///     Green component.
    /// </summary>
    [FieldOffset(0x01)] public byte g;
    /// <summary>
    ///     Blue component.
    /// </summary>
    [FieldOffset(0x02)] public byte b;
    /// <summary>
    ///     Alpha component.
    /// </summary>
    [FieldOffset(0x03)] public byte a;

    /// <summary>
    ///     Create a new colour from <paramref name="raw"/> colour RGBA data.
    /// </summary>
    /// <param name="raw">Raw color in RGBA format.</param>
    public TextureColor(int raw)
    {
        r = g = b = a = 0;
        this.raw = (uint)raw;
    }

    /// <summary>
    ///     Create a new colour from <paramref name="raw"/> colour RGBA data.
    /// </summary>
    /// <param name="raw">Raw color in RGBA format.</param>
    public TextureColor(uint raw)
    {
        r = g = b = a = 0;
        this.raw = raw;
    }

    /// <summary>
    ///     Create a new colour from the supplied <paramref name="r"/>, <paramref name="g"/>, 
    ///     <paramref name="b"/>, and <paramref name="a"/> values.
    /// </summary>
    /// <param name="r">8-bit red color component.</param>
    /// <param name="g">8-bit green color component.</param>
    /// <param name="b">8-bit blue color component.</param>
    /// <param name="a">8-bit alpha component.</param>
    public TextureColor(byte r, byte g, byte b, byte a = 0xFF)
    {
        raw = 0;
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }

    /// <summary>
    ///     Create a new grayscale colour from the supplied <paramref name="intensity"/>
    ///     and <paramref name="a"/> values.
    /// </summary>
    /// <param name="intensity">8-bit intensity (grayscale) value.</param>
    /// <param name="a">8-bit alpha component.</param>
    public TextureColor(byte intensity, byte a = 0xFF)
    {
        raw = 0;
        r = g = b = intensity;
        this.a = a;
    }

    /// <summary>
    ///     Get this colour's grayscale value.
    /// </summary>
    /// <returns>
    ///     Returns the 8-bit grayscale value of this colour.
    /// </returns>
    public byte GetIntensity()
    {
        return (byte)(
            (r * 0.30f) +
            (g * 0.59f) +
            (b * 0.11f));
    }

    /// <summary>
    ///     Create a new colour by linearly interpolating between <paramref name="c0"/> and <paramref name="c1"/>
    ///     at the specified intermediate point <paramref name="time01"/>.
    /// </summary>
    /// <param name="c0">Colour 0.</param>
    /// <param name="c1">Colour 1.</param>
    /// <param name="time01">The interpolation time between <paramref name="c0"/> (t=0) and <paramref name="c1"/> (t=1).</param>
    /// <returns>
    ///     Returns a new colour derived from interpolating between <paramref name="c0"/> and <paramref name="c1"/> at <paramref name="time01"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown if <paramref name="time01"/> is not between 0f and 1f (inclusive at both ends).
    /// </exception>
    public static TextureColor Lerp(TextureColor c0, TextureColor c1, float time01)
    {
        bool isTimeValid = time01 >= 0f && time01 <= 1f;
        if (!isTimeValid)
        {
            string msg = $"Argument `{nameof(time01)}` must be between 0 and 1 (inclusive). Value was: {time01}.";
            throw new ArgumentException(msg);
        }

        float timeC0 = 1f - time01;
        float timeC1 = time01;
        float r = c0.r * timeC0 + c1.r * timeC1;
        float g = c0.g * timeC0 + c1.g * timeC1;
        float b = c0.b * timeC0 + c1.b * timeC1;
        float a = c0.a * timeC0 + c1.a * timeC1;
        var color = new TextureColor((byte)r, (byte)g, (byte)b, (byte)a);
        return color;
    }

    /// <summary>
    ///     Convert GameCube IA4 value into colour.
    /// </summary>
    /// <param name="ia4">The 8-bit IA4 colour.</param>
    /// <returns>
    ///     A 32-bit colour representing the <paramref name="ia4"/> value.
    /// </returns>
    public static TextureColor FromIA4(byte ia4)
    {
        byte i = (byte)(((ia4 >> 4) & 0b_0000_1111) / 15f * 255);
        byte a = (byte)(((ia4 >> 0) & 0b_0000_1111) / 15f * 255);
        var color = new TextureColor(i, a);
        return color;
    }
    /// <summary>
    ///     Convert colour into GameCube IA4 value.
    /// </summary>
    /// <param name="c">Colour.</param>
    /// <returns>
    ///     An 8-bit value representing colour <paramref name="c"/> in IA4 format.
    /// </returns>
    public static byte ToIA4(TextureColor c)
    {
        byte i = c.GetIntensity();
        byte i4 = (byte)(i >> 4);
        byte a4 = (byte)(c.a >> 4);
        byte ia4 = (byte)((i4 << 4) + (a4 << 0));
        return ia4;
    }

    /// <summary>
    ///     Convert GameCube IA8 value into colour.
    /// </summary>
    /// <param name="ia8">The 16-bit IA8 value.</param>
    /// <returns>
    ///     A 32-bit colour representing the <paramref name="ia8"/> value.
    /// </returns>
    public static TextureColor FromIA8(ushort ia8)
    {
        byte i = (byte)((ia8 >> 8) & 0b_1111_1111);
        byte a = (byte)((ia8 >> 0) & 0b_1111_1111);
        var color = new TextureColor(i, a);
        return color;
    }
    /// <summary>
    ///     Convert colour into GameCube IA8 value.
    /// </summary>
    /// <param name="c">Colour.</param>
    /// <returns>
    ///     A 16-bit value representing colour <paramref name="c"/> in IA8 format.
    /// </returns>
    public static ushort ToIA8(TextureColor c)
    {
        byte i = c.GetIntensity();
        byte a = c.a;
        ushort ia8 = (ushort)((i << 8) + (a << 00));
        return ia8;
    }

    /// <summary>
    ///     Convert GameCube RGB565 value into colour.
    /// </summary>
    /// <param name="rgb565">The 16-bit RGB565 value.</param>
    /// <returns>
    ///     A 32-bit colour representing the <paramref name="rgb565"/> value.
    /// </returns>
    public static TextureColor FromRGB565(ushort rgb565)
    {
        byte r = (byte)(((rgb565 >> 11) & (0b_0001_1111)) / 31f * 255);
        byte g = (byte)(((rgb565 >> 05) & (0b_0011_1111)) / 63f * 255);
        byte b = (byte)(((rgb565 >> 00) & (0b_0001_1111)) / 31f * 255);
        var color = new TextureColor(r, g, b);
        return color;
    }
    /// <summary>
    ///     Convert colour into GameCube RGB565 value.
    /// </summary>
    /// <param name="c">Colour.</param>
    /// <returns>
    ///     A 16-bit value representing colour <paramref name="c"/> in RGB565 format.
    /// </returns>
    public static ushort ToRGB565(TextureColor c)
    {
        byte r5 = (byte)((c.r >> 3) & 0b_0001_1111);
        byte g6 = (byte)((c.g >> 2) & 0b_0011_1111);
        byte b5 = (byte)((c.b >> 3) & 0b_0001_1111);
        ushort rgb565 = (ushort)((r5 << 11) + (g6 << 05) + (b5 << 00));
        return rgb565;
    }

    /// <summary>
    ///     Convert GameCube RGB5A3 value into colour.
    /// </summary>
    /// <param name="rgb5a3">The 16-bit RGB5A3 value.</param>
    /// <returns>
    ///     A 32-bit colour representing the <paramref name="rgb5a3"/> value.
    /// </returns>
    public static TextureColor FromRGB5A3(ushort rgb5a3)
    {
        byte r, g, b, a;
        bool hasAlpha = (rgb5a3 & 0x8000) == 0;
        if (hasAlpha)
        {
            a = (byte)(((rgb5a3 >> 12) & (0b_0000_0111)) / 07f * 255); // lower 3 of 4 bits
            r = (byte)(((rgb5a3 >> 08) & (0b_0000_1111)) / 15f * 255); // 4 bits
            g = (byte)(((rgb5a3 >> 04) & (0b_0000_1111)) / 15f * 255); // 4 bits
            b = (byte)(((rgb5a3 >> 00) & (0b_0000_1111)) / 15f * 255); // 4 bits
        }
        else
        {
            a = 0xFF;                                                  // alpha implied
            r = (byte)(((rgb5a3 >> 10) & (0b_0001_1111)) / 31f * 255); // 5 bits
            g = (byte)(((rgb5a3 >> 05) & (0b_0001_1111)) / 31f * 255); // 5 bits
            b = (byte)(((rgb5a3 >> 00) & (0b_0001_1111)) / 31f * 255); // 5 bits
        }
        var color = new TextureColor(r, g, b, a);
        return color;
    }
    /// <summary>
    ///     Convert colour into GameCube RGB5A3 value.
    /// </summary>
    /// <param name="c">Colour.</param>
    /// <returns>
    ///     A 16-bit value representing colour <paramref name="c"/> in RGB5A3 format.
    /// </returns>
    public static ushort ToRGB5A3(TextureColor c)
    {
        byte r, g, b, a;
        ushort rgb5a3;

        // If alpha is mostly opaque, consider it fully opaque so that
        // we can use the format with more color depth.
        bool isVeryOpaque = (c.a >> 4) > 0b_0000_0111;
        if (isVeryOpaque)
        {
            // Opaque alpha 'a' is const 1 in bit position 15, 0x8000
            const ushort opaque = 0x8000;
            r = (byte)((c.r >> 3) & 0b_0001_1111); // 5 bits
            g = (byte)((c.g >> 3) & 0b_0001_1111); // 5 bits
            b = (byte)((c.b >> 3) & 0b_0001_1111); // 5 bits
            rgb5a3 = (ushort)(opaque + (r << 10) + (g << 5) + (b << 0));
        }
        else
        {
            a = (byte)((c.a >> 5) & 0b_0000_0111); // 3 bits
            r = (byte)((c.r >> 4) & 0b_0000_1111); // 4 bits
            g = (byte)((c.g >> 4) & 0b_0000_1111); // 4 bits
            b = (byte)((c.b >> 4) & 0b_0000_1111); // 4 bits
            rgb5a3 = (ushort)((a << 12) + (r << 8) + (g << 4) + (b << 0));
        }
        return rgb5a3;
    }


    public static readonly TextureColor Clear = new TextureColor(0, 0);

    public override string ToString()
    {
        return $"{nameof(TextureColor)}(R:{r:x2}, G:{g:x2}, B:{b:x2}, A:{a:x2})";
    }
}
