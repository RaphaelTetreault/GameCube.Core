using System;
using System.Text;

namespace GameCube.Common;

/// <summary>
///     GameCube text encodings.
/// </summary>
public class TextEncoding
{
    /// <summary>
    ///     Encoding used for North America (and Europe?).
    /// </summary>
    public static readonly Encoding Windows1252 = Encoding.GetEncoding(codepage: 1252);

    /// <summary>
    ///     Encoding used for Japan.
    /// </summary>
    public static readonly Encoding ShiftJIS = Encoding.GetEncoding(codepage: 932);

    public static bool StringIsOnlyHexCharacters(string value)
    {
        // Skip
        if (string.IsNullOrWhiteSpace(value))
            return false;

        // "Sanitize"
        value = value.ToUpper();

        // Ensure each char is in range 0-9 or A-F
        foreach (char c in value)
            if (c < '0' || '9' < c && c < 'A' || 'F' < c)
                return false; // Found non-hex character.

        // Did not find non-hex character.
        return true;
    }

    public static byte[] GetHexStringAsByteArray(string value)
    {
        // Catch invalid string
        if (string.IsNullOrWhiteSpace(value))
        {
            string msg = $"Value is null or whitespace.";
            throw new ArgumentException(msg);
        }

        // Make sure value has 2 hex chars per byte.
        bool isNotEven = value.Length % 2 == 1;
        if (isNotEven)
        {
            string msg = $"Value length must be multiple of 2 (to be 2 hex chars per byte).";
            throw new ArgumentException(msg);
        }

        // Extract bytes from string
        int length = value.Length / 2;
        byte[] byteArray = new byte[length];
        for (int i = 0; i < length; i++)
        {
            string byteString = value.Substring(i * 2, 2);
            byte @byte = byte.Parse(byteString, System.Globalization.NumberStyles.HexNumber);
            byteArray[i] = @byte;
        }

        return byteArray;
    }

    public static string ConvertBytesToEncoding(string value, Encoding encoding)
    {
        // Sanitize string
        value = value.Replace(" ", "");
        // 
        bool isInvalid = !StringIsOnlyHexCharacters(value);
        if (isInvalid)
        {
            string msg = $"Value must be only hexadecimal characters.";
            throw new ArgumentException(msg);
        }
        // Convert bytes to encoded string
        byte[] bytes = GetHexStringAsByteArray(value);
        string result = encoding.GetString(bytes);
        return result;
    }

    public static string ConvertEncodingToEncoding(string value, Encoding encodingInput, Encoding encodingOutput)
    {
        // Convert string from one format to another
        byte[] bytes = encodingInput.GetBytes(value);
        string result = encodingOutput.GetString(bytes);
        return result;
    }

}
