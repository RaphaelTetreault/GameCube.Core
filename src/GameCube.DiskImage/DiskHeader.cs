using Manifold.IO;
using System;

namespace GameCube.DiskImage
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    ///     Disk header is extract by Dolphin as "sys/boot.bin".
    /// </remarks>
    public class DiskHeader :
        IBinaryAddressable,
        IBinarySerializable
    {
        // NOTE: I used Offset to be consistent with YAGCD but unsure
        //  if their use of "offset" is the same as me (offset != pointer).
        private GameID gameID;
        private ushort makerCode;
        private byte diskID;
        private byte version;
        private byte audioStreaming;
        private byte streamBufferSize;
        private byte[] unused_0x0a = []; // size: 0x12
        private uint dvdMagicWord; // 0xc2339f3d
        private AsciiCString gameName = new(); // Buffer size: 0x03e0
        private Offset debugMonitorOffset;
        private Pointer debugMonitorLoadAddress;
        private byte[] unused_0x0408 = []; // size: 0x18
        private Pointer mainExecutablePtr;
        private Pointer fileSystemPtr;
        private uint fileSystemSize;
        private uint fileSystemMaximumSize;
        private uint userPosition;
        private uint userLength;
        private uint unk_0x0438;
        private uint zero_0x043C;
        // Structure (ie: above) raw. See YAGCD "DVD Structure"
        // https://www.gc-forever.com/yagcd/chap13.html
        private byte[] bootBin = [];

        private const int unused_0x0a_size = 0x12;
        private const int unused_0x0408_size = 0x18;
        private const int endOfNameAddress = 0x0400;
        //
        public const int Address = 0x0;
        public const int Size = 0x440;

        public AsciiCString GameName => gameName;
        public Pointer FileSystemPointer => fileSystemPtr;
        public AddressRange AddressRange { get; set ; }
        public byte[] BootBinRaw => bootBin;

        public Pointer MainExecutablePtr => mainExecutablePtr;

        public void Deserialize(EndianBinaryReader reader)
        {
            this.RecordStartAddress(reader);

            reader.Read(ref gameID);
            reader.Read(ref makerCode);
            reader.Read(ref diskID);
            reader.Read(ref version);
            reader.Read(ref audioStreaming);
            reader.Read(ref streamBufferSize);
            reader.Read(ref unused_0x0a, unused_0x0a_size);
            reader.Read(ref dvdMagicWord);
            reader.Read(ref gameName);
            reader.JumpToAddress(endOfNameAddress);
            reader.Read(ref debugMonitorOffset);
            reader.Read(ref debugMonitorLoadAddress);
            reader.Read(ref unused_0x0408, unused_0x0408_size);
            reader.Read(ref mainExecutablePtr);
            reader.Read(ref fileSystemPtr);
            reader.Read(ref fileSystemSize);
            reader.Read(ref fileSystemMaximumSize);
            reader.Read(ref userPosition);
            reader.Read(ref userLength);
            reader.Read(ref unk_0x0438);
            reader.Read(ref zero_0x043C);

            this.RecordEndAddress(reader);

            // Read "sys/boot.bin" as raw byte array
            reader.JumpToAddress(AddressRange.startAddress);
            reader.Read(ref bootBin, Size);
            // Make sure endings align betwen reads
            Assert.IsTrue(reader.BaseStream.Position == AddressRange.endAddress);
        }

        public void Serialize(EndianBinaryWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
