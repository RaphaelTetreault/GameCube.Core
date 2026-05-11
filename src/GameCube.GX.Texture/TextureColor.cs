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
    public readonly byte GetIntensity()
    {
        byte intensity = (byte)(r * 0.30f + g * 0.59f + b * 0.11f);
        return intensity;
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
        byte i4 = (byte)(ia4 >>> 4 & 0b0000_1111);
        byte a4 = (byte)(ia4 >>> 0 & 0b0000_1111);
        byte i = (byte)(i4 << 4 | i4);
        byte a = (byte)(a4 << 4 | a4);
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
        byte i4 = (byte)(i >>> 4);
        byte a4 = (byte)(c.a >>> 4);
        byte ia4 = (byte)(i4 << 4 | a4 << 0);
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
        byte i = (byte)(ia8 >>> 8);
        byte a = (byte)(ia8 >>> 0);
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
        ushort ia8 = (ushort)(i << 8 | a << 0);
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
        byte r5 = (byte)(rgb565 >>> 11 /***********/); // implicit truncation
        byte g6 = (byte)(rgb565 >>> 05 & 0b0011_1111); // keep lowest 6 bits
        byte b5 = (byte)(rgb565 >>> 00 & 0b0001_1111); // keep lowest 5 bits
        // Make 8 bit values from 5 or 6 bit values
        // R_B: Lowest 3 bits are the highest of 5 bits
        // _G_: Lowest 2 bits are the highest of 6 bits
        byte r = (byte)(r5 << 3 | r5 >>> 2);
        byte g = (byte)(g6 << 2 | b5 >>> 4);
        byte b = (byte)(b5 << 3 | b5 >>> 2);
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
        byte r5 = (byte)(c.r >>> 3);
        byte g6 = (byte)(c.g >>> 2);
        byte b5 = (byte)(c.b >>> 3);
        ushort rgb565 = (ushort)(r5 << 11 | g6 << 5 | b5 << 0);
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
            // Old A value was wrong! The point of upper bit being off is to enbable alpha range!
            //a = (byte)(((rgb5a3 >>> 12) & (0b_0000_0111)) / 07f * 255); // lower 3 of 4 bits
            //r = (byte)(((rgb5a3 >>> 08) & (0b_0000_1111)) / 15f * 255); // 4 bits
            //g = (byte)(((rgb5a3 >>> 04) & (0b_0000_1111)) / 15f * 255); // 4 bits
            //b = (byte)(((rgb5a3 >>> 00) & (0b_0000_1111)) / 15f * 255); // 4 bits

            // If has alpha, then color is treated as RGBA4 (16bit)
            byte a4 = (byte)(rgb5a3 >>> 12 /***********/); // implicit truncation
            byte r4 = (byte)(rgb5a3 >>> 08 & 0b0000_1111); // keep lowest 4 bits
            byte g4 = (byte)(rgb5a3 >>> 04 & 0b0000_1111); // keep lowest 4 bits
            byte b4 = (byte)(rgb5a3 >>> 00 & 0b0000_1111); // keep lowest 4 bits
            // Make 8 bit values from 4 bit values
            // Lowest 4 bits are the same 4 bits
            a = (byte)(a4 << 4 | a4);
            r = (byte)(r4 << 4 | r4);
            g = (byte)(g4 << 4 | g4);
            b = (byte)(b4 << 4 | b4);
        }
        else
        {
            //a = 0xFF;                                                  // alpha implied
            //r = (byte)(((rgb5a3 >> 10) & (0b_0001_1111)) / 31f * 255); // 5 bits
            //g = (byte)(((rgb5a3 >> 05) & (0b_0001_1111)) / 31f * 255); // 5 bits
            //b = (byte)(((rgb5a3 >> 00) & (0b_0001_1111)) / 31f * 255); // 5 bits

            // If no alpha, trest as RGB5A1
            //   a1 = (byte)(rgb5a3 >>> 15 & 0b0000_0001); // implied
            byte r5 = (byte)(rgb5a3 >>> 10 & 0b0001_1111); // keep lowest 5 bits
            byte g5 = (byte)(rgb5a3 >>> 05 & 0b0001_1111); // keep lowest 5 bits
            byte b5 = (byte)(rgb5a3 >>> 00 & 0b0001_1111); // keep lowest 5 bits
            // Make 8 bit values from 5 bit values
            // Lowest 3 bits are the same 5 bits
            r = (byte)(r5 << 3 | r5 >>> 2); // 
            g = (byte)(g5 << 3 | g5 >>> 2); // 
            b = (byte)(b5 << 3 | b5 >>> 2); // 
            a = 0xFF; // alpha implied
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

        // Since this stores 3-bit alpha, check to see if we are opaque
        // This matches A of 224-255 (last 32 values, or 1/8 of range).
        bool isOpaque = (c.a >>> 5) == 0b_0000_0111;
        if (isOpaque)
        {
            //const ushort opaque = 0x8000;
            //r = (byte)((c.r >> 3) & 0b_0001_1111); // 5 bits
            //g = (byte)((c.g >> 3) & 0b_0001_1111); // 5 bits
            //b = (byte)((c.b >> 3) & 0b_0001_1111); // 5 bits
            //rgb5a3 = (ushort)(opaque + (r << 10) + (g << 5) + (b << 0));

            // Opaque alpha 'a' is const 1 in bit position 15, 0x8000
            const ushort opaque = 0x8000;
            r = (byte)(c.r >>> 3); // 5 bits
            g = (byte)(c.g >>> 3); // 5 bits
            b = (byte)(c.b >>> 3); // 5 bits
            rgb5a3 = (ushort)(opaque | r << 10 | g << 5 | b << 0);
        }
        else
        {
            //a = (byte)((c.a >> 5) & 0b_0000_0111); // 3 bits
            //r = (byte)((c.r >> 4) & 0b_0000_1111); // 4 bits
            //g = (byte)((c.g >> 4) & 0b_0000_1111); // 4 bits
            //b = (byte)((c.b >> 4) & 0b_0000_1111); // 4 bits
            //rgb5a3 = (ushort)((a << 12) + (r << 8) + (g << 4) + (b << 0));

            a = (byte)(c.a >>> 5); // 3 bits stored in top 4 bits, bit 15 CANNOT be 1!
            r = (byte)(c.r >>> 4); // 4 bits
            g = (byte)(c.g >>> 4); // 4 bits
            b = (byte)(c.b >>> 4); // 4 bits
            rgb5a3 = (ushort)(a << 12 | r << 8 | g << 4 | b << 0);
        }
        return rgb5a3;
    }


    public static readonly TextureColor Clear = new(0, 0);

    public readonly override string ToString()
    {
        return $"{nameof(TextureColor)}(R:{r:x2}, G:{g:x2}, B:{b:x2}, A:{a:x2})";
    }
}
