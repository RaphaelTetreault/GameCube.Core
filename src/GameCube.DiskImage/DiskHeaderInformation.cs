// Resources
// https://hitmen.c02.at/files/yagcd/yagcd/chap13.html#sec13.2

using Manifold.IO;
using System;

namespace GameCube.DiskImage;

/// <summary>
///     
/// </summary>
/// <remarks>
///     Disk header also includes 2kb after above which is "sys/bi2.bin".
/// </remarks>
public class DiskHeaderInformation :
    IBinaryAddressable,
    IBinarySerializable
{
    private byte[] bi2Bin = [];

    public const int Address = 0x440;
    public const int Size = 0x2000;

    public AddressRange AddressRange { get; set; }
    public byte[] Bi2BinRaw => bi2Bin;


    public void Deserialize(EndianBinaryReader reader)
    {
        this.RecordStartAddress(reader);
        {
            // Read "sys/bi2.bin" as raw byte array
            reader.Read(ref bi2Bin, Size);
        }
        this.RecordEndAddress(reader);
    }

    public void Serialize(EndianBinaryWriter writer)
    {
        throw new NotImplementedException();
    }
}
