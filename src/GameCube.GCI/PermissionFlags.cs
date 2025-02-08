namespace GameCube.GCI;

/// <summary>
///     Indicates user permissions for managing the GCI save data.
/// </summary>
[System.Flags]
public enum PermissionFlags : byte
{
    IsPublic = 1 << 2,
    NoCopy = 1 << 3,
    NoMove = 1 << 4,
}
