using Manifold.IO;
using System;

namespace GameCube.DiskImage;

/// <summary>
///     TODO
/// </summary>
public class MainExecutable :
    IBinaryAddressable,
    IBinarySerializable
{
    public AddressRange AddressRange { get; set; }

    public void Deserialize(EndianBinaryReader reader)
    {
        throw new NotImplementedException();
    }

    public void Serialize(EndianBinaryWriter writer)
    {
        throw new NotImplementedException();
    }
}
