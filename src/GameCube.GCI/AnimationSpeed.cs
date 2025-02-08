namespace GameCube.GCI;

/// <summary>
///     Indicates animation speed for GameCube GCI icon.
/// </summary>
public enum AnimationSpeed : ushort
{
    NoIcon = 0,
    // Animation for Icon 0
    Icon0_FrameCount04 = 0b_01 << 0 * 2,
    Icon0_FrameCount08 = 0b_10 << 0 * 2,
    Icon0_FrameCount12 = 0b_11 << 0 * 2,
    // Animation for Icon 1
    Icon1_FrameCount04 = 0b_01 << 1 * 2,
    Icon1_FrameCount08 = 0b_10 << 1 * 2,
    Icon1_FrameCount12 = 0b_11 << 1 * 2,
    // Animation for Icon 2
    Icon2_FrameCount04 = 0b_01 << 2 * 2,
    Icon2_FrameCount08 = 0b_10 << 2 * 2,
    Icon2_FrameCount12 = 0b_11 << 2 * 2,
    // Animation for Icon 3
    Icon3_FrameCount04 = 0b_01 << 3 * 2,
    Icon3_FrameCount08 = 0b_10 << 3 * 2,
    Icon3_FrameCount12 = 0b_11 << 3 * 2,
    // Animation for Icon 4
    Icon4_FrameCount04 = 0b_01 << 4 * 2,
    Icon4_FrameCount08 = 0b_10 << 4 * 2,
    Icon4_FrameCount12 = 0b_11 << 4 * 2,
    // Animation for Icon 5
    Icon5_FrameCount04 = 0b_01 << 5 * 2,
    Icon5_FrameCount08 = 0b_10 << 5 * 2,
    Icon5_FrameCount12 = 0b_11 << 5 * 2,
    // Animation for Icon 6
    Icon6_FrameCount04 = 0b_01 << 6 * 2,
    Icon6_FrameCount08 = 0b_10 << 6 * 2,
    Icon6_FrameCount12 = 0b_11 << 6 * 2,
    // Animation for Icon 7
    Icon7_FrameCount04 = 0b_01 << 7 * 2,
    Icon7_FrameCount08 = 0b_10 << 7 * 2,
    Icon7_FrameCount12 = 0b_11 << 7 * 2,
}
