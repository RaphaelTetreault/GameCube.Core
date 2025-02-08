namespace GameCube.GCI;

/// <summary>
///     Indicates the image format for the GCI.
/// </summary>
public enum ImageFormat : ushort
{
    NoIcon,
    IndirectColor_SharedPalette,
    DirectColor,
    IndirectorColor_UniquePalettes,
}
