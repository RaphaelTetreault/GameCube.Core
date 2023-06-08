﻿using Manifold.IO;

namespace GameCube.DiskImage
{
    public class FileSystem :
        IBinaryAddressable,
        IBinarySerializable
    {
        private byte[] raw;
        private FileSystemEntry root;
        private FileSystemEntry[] entries;

        public AddressRange AddressRange { get; set; }
        public List<FileEntry> FileEntries { get; private set; } = new List<FileEntry>();
        public byte[] Raw => raw;


        public void Deserialize(EndianBinaryReader reader)
        {
            this.RecordStartAddress(reader);
            {
                reader.Read(ref root);
                reader.Read(ref entries, root.RootEntries - 1);
            }
            // defer recording final address

            // Compile all files and directories
            FileSystemEntry[] allFileEntries = new FileSystemEntry[root.RootEntries];
            allFileEntries[0] = root;
            entries.CopyTo(allFileEntries, 1);

            // Set default for paths
            string[] paths = new string[allFileEntries.Length];
            for (int i = 0; i < paths.Length; i++)
                paths[i] = "";
            paths[0] = "ROOT";

            for (int i = 1; i < paths.Length; i++)
            {
                var entry = allFileEntries[i];
                AsciiCString str = null;
                reader.Read(ref str);

                // Append directory if necessary
                if (entry.Type == FileSystemEntryType.Directory)
                {
                    for (int j = i; j < entry.DirectoryStackCount; j++)
                    {
                        int index = j;
                        paths[index] += $"{str}/";
                    }
                }
                else
                {
                    paths[i] += str;
                }

                // DEBUG
                //Console.WriteLine(paths[i]);
            }

            // Now that strings are read, we have the final address
            this.RecordEndAddress(reader);

            // Compile file paths with data.
            // TODO: perhaps make this an enumerator? Not multithreadable as-is.
            FileEntries = new List<FileEntry>();
            for (int i = 0; i < allFileEntries.Length; i++)
            {
                var entry = allFileEntries[i];
                if (entry.Type == FileSystemEntryType.Directory)
                    continue;

                var ptr = entry.FileOffset;
                var len = entry.FileLength;
                reader.JumpToAddress(ptr);
                var data = reader.ReadBytes(len);

                var fileEntry = new FileEntry()
                {
                    Name = paths[i],
                    Data = data,
                };
                FileEntries.Add(fileEntry);
            }

            // Read FST as single block
            reader.JumpToAddress(AddressRange.startAddress);
            reader.Read(ref raw, AddressRange.Size);
            // The address should end where it was, otherwise wrong amount of data read.
            Assert.IsTrue(reader.BaseStream.Position == AddressRange.endAddress);
        }

        public void Serialize(EndianBinaryWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}