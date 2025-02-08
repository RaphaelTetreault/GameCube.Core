using Manifold.IO;

namespace GameCube.Cheats;

/// <summary>
///     File wrapper for <see cref="Gct"/>.
/// </summary>
internal class GctFile : BinaryFileWrapper<Gct>
{
    // CONSTANTS
    public const Endianness endianness = Endianness.BigEndian;
    public const string fileExtension = ".gct";

    // PROPERTIES
    public override Endianness Endianness { get; set; } = endianness;
    public override string FileExtension { get; set; } = fileExtension;
    public override string FileName { get; set; } = string.Empty;

    // CONSTRUCTORS
    public GctFile() : base() { }
    public GctFile(string inputPath) : base(inputPath) { }
}
