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
}
