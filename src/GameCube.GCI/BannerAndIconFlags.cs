namespace GameCube.GCI;

/// <summary>
///     Indicates color format and animation type for GameCube GCI icon.
/// </summary>
[System.Flags]
public enum BannerAndIconFlags : byte
{
    NoBanner = 0,

    // Color information
    IndirectColorCI8 = 1 << 0,
    DirectColorRGB5A3   = 1 << 1,
    InvalidBanner = IndirectColorCI8 | DirectColorRGB5A3,

    // Animation flags
    // Unset is loop, set is ping-pong (AKA seesaw)
    AnimationLoop     = NoBanner, // 0 << 2,
    AnimationPingPong = 1 << 2,
}
