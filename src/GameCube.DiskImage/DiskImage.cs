// Resources
// https://www.gc-forever.com/yagcd/chap14.html#sec14.1
// https://github.com/dolphin-emu/dolphin/blob/master/Source/Core/Core/HW/GCMemcard/GCMemcard.h#L240



using Manifold.IO;
using System;

namespace GameCube.DiskImage
{
    /// <summary>
    ///     Represents the root GameCube disk image (ISO/ROM).
    /// </summary>
    public class DiskImage :
        IBinaryFileType,
        IBinarySerializable
    {
        // Proper file structure
        private DiskHeader diskHeader = new();
        private DiskHeaderInformation diskHeaderInformation = new();
        private Apploader apploader = new();
        private FileSystem fileSystem = new();
        private byte[] mainExecutableRaw = [];
        //private MainExecutable mainExecutable;

        public const Endianness endianness = Endianness.BigEndian;


        public Endianness Endianness => endianness;
        public string FileExtension => ".iso";
        public string FileName { get; set; } = string.Empty;

        public Apploader Apploader { get => apploader; set => apploader = value; }
        public DiskHeader DiskHeader { get => diskHeader; set => diskHeader = value; }
        public DiskHeaderInformation DiskHeaderInformation { get => diskHeaderInformation; set => diskHeaderInformation = value; }
        public FileSystem FileSystem { get => fileSystem; set => fileSystem = value; }
        public byte[] MainExecutableRaw => mainExecutableRaw;
        //public MainExecutable MainExecutable { get => mainExecutable; set => mainExecutable = value; }

        public void Deserialize(EndianBinaryReader reader)
        {
            // Fixed addresses
            Assert.IsTrue(reader.BaseStream.Position == DiskHeader.Address);
            reader.JumpToAddress(DiskHeader.Address);
            reader.Read(ref diskHeader);
            // cont.
            Assert.IsTrue(reader.BaseStream.Position == DiskHeaderInformation.Address);
            reader.JumpToAddress(DiskHeaderInformation.Address);
            reader.Read(ref diskHeaderInformation);
            // cont.
            Assert.IsTrue(reader.BaseStream.Position == Apploader.Address);
            reader.JumpToAddress(Apploader.Address);
            reader.Read(ref apploader);
            
            // Non-fixed addresses
            // Read FS description
            reader.JumpToAddress(diskHeader.FileSystemPointer);
            reader.Read(ref fileSystem);
            // cont.
            reader.JumpToAddress(diskHeader.MainExecutablePtr);
            int size = diskHeader.FileSystemPointer - diskHeader.MainExecutablePtr;
            reader.Read(ref mainExecutableRaw, size);
        }

        public void Serialize(EndianBinaryWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}