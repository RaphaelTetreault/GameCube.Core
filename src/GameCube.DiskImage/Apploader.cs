using Manifold.IO;
using System;

namespace GameCube.DiskImage
{
    public class Apploader :
        IBinaryAddressable,
        IBinarySerializable
    {
        private AsciiCString dateTime = new(); // 10 bytes YYYY/MM/DD (?)
        private Pointer entryAddress;
        private int size;
        private int trailerSize;
        //
        private byte[] raw = [];

        public const int Address = 0x2440;
        public const int HeaderSize = 0x20;

        public AddressRange AddressRange { get; set; }
        public byte[] Raw => raw;


        public void Deserialize(EndianBinaryReader reader)
        {
            this.RecordStartAddress(reader);
            {
                reader.Read(ref dateTime);
                reader.AlignTo(0x10);
                reader.Read(ref entryAddress);
                reader.Read(ref size);
                reader.Read(ref trailerSize);
                // Jump back to start
                reader.JumpToAddress(AddressRange.startAddress);
                reader.Read(ref raw, HeaderSize + size + trailerSize);
            }
            this.RecordEndAddress(reader);
        }

        public void Serialize(EndianBinaryWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
