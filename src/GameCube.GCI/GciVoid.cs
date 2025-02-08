using Manifold.IO;

namespace GameCube.GCI;

/// <summary>
///     An empty data type for GCI testing.
/// </summary>
public class GciVoid : 
    IBinaryFileType,
    IBinarySerializable
{
    public Endianness Endianness => Endianness.BigEndian;
    public string FileExtension => string.Empty;
    public string FileName { get; set; } = string.Empty;

    public void Deserialize(EndianBinaryReader reader)
    {
    }

    public void Serialize(EndianBinaryWriter writer)
    {
    }
}
