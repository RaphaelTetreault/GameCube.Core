using Manifold.IO;

namespace GameCube.DiskImage;

/// <summary>
///     File wrapper for <see cref="DiskImage"/>.
/// </summary>
public class DiskImageFile : BinaryFileWrapper<DiskImage>
{
    // CONSTANTS
    public const Endianness endianness = Endianness.BigEndian;
    public const string fileExtension = ".iso";

    // PROPERTIES
    public override Endianness Endianness { get; set; } = endianness;
    public override string FileExtension { get; set; } = fileExtension;
    public override string FileName { get; set; } = string.Empty;

    // CONSTRUCTORS
    public DiskImageFile() : base() { }
    public DiskImageFile(string inputPath) : base(inputPath) { }
}