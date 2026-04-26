using System;

namespace GameCube.GCI;

/// <summary>
///     Indicates color format and animation type for GameCube GCI icon.
/// </summary>
[System.Flags]
public enum GciBannerIconFlags : byte
{
    None = 0,

    // Color information
    IndirectColorCI8 = 1 << 0,
    DirectColorRGB5A3   = 1 << 1,
    Metadata_InvalidBanner = IndirectColorCI8 | DirectColorRGB5A3,

    // Animation flags
    // Unset is loop, set is ping-pong (AKA seesaw)
    AnimationLoop     = None, // 0 << 2,
    AnimationPingPong = 1 << 2,
}

public static class GciBannerIconFlagsExtensions
{
    extension(GciBannerIconFlags gciBannerIconFlags)
    {
        public byte Byte => (byte)gciBannerIconFlags;

        public void Validate()
        {
            // Make sure flags are all good
            if (!Enum.IsDefined(gciBannerIconFlags))
            {
                string msg = $"Invalid {nameof(GciBannerIconFlags)} flags detected.";
                throw new Exception(msg);
            }

            // Make sure combinations of flags are good
            bool isInvalid = (gciBannerIconFlags & GciBannerIconFlags.Metadata_InvalidBanner) == GciBannerIconFlags.Metadata_InvalidBanner;
            if (isInvalid)
            {
                string msg = $"Invalid banner and icon format detected.";
                throw new Exception(msg);
            }
        }
    }
}