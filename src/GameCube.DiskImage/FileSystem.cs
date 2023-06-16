﻿using Manifold.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameCube.DiskImage
{
    public class FileSystem :
        IBinaryAddressable,
        IBinarySerializable
    {
        private byte[] raw = Array.Empty<byte>();
        private DirectoryNode root = new();
        private AddressRange fileSystemNodesAddressRange;
        private AddressRange namesAddressRange;

        public AddressRange AddressRange { get; set; }
        public AddressRange FileSystemNodesAddressRange { get => fileSystemNodesAddressRange; set => fileSystemNodesAddressRange = value; }
        public AddressRange NamesAddressRange { get => namesAddressRange; set => namesAddressRange = value; }
        public int FileAlignment { get; set; } = 4; // GC ISO default
        public byte[] Raw => raw;
        public DirectoryNode RootNode { get => root; set => root = value; }

        public void Deserialize(EndianBinaryReader reader)
        {
            // TODO: consider recusive deserialization

            FileSystemNode[] nodes = Array.Empty<FileSystemNode>();

            this.RecordStartAddress(reader);
            fileSystemNodesAddressRange.RecordStartAddress(reader);
            {
                // Read root and reset stream position.
                // Loops later in code begin at index 1 to skip root.
                reader.Read(ref root);
                reader.JumpToAddress(AddressRange.startAddress);
                // Read all entries in one go
                reader.Read(ref nodes, root.RootNodeCount);
            }
            fileSystemNodesAddressRange.RecordEndAddress(reader);
            namesAddressRange.RecordStartAddress(reader);
            {
                var directoryStack = new Stack<DirectoryNode>();
                directoryStack.Push(root);

                // Get names for all elements
                for (int i = 1; i < nodes.Length; i++)
                {
                    // Clear directories from stack when last child index is met
                    while (i >= directoryStack.Peek().DirectoryLastChildIndex)
                        directoryStack.Pop();

                    // Interally cast node to FileNode or DirectoryNode
                    nodes[i] = nodes[i].GetNodeAsProperType();
                    var currentNode = nodes[i];

                    // Add current node to directory stack
                    var currentDirectory = directoryStack.Peek();
                    currentDirectory.AddChild(currentNode);

                    // Get name
                    AsciiCString entryName = string.Empty;
                    var namePtr = namesAddressRange.startAddress + currentNode.NameOffset;
                    reader.JumpToAddress(namePtr);
                    reader.Read(ref entryName);
                    currentNode.Name = entryName;

                    // If a directory, add to directory stack
                    bool isDirectory = currentNode.Type == FileSystemNodeType.Directory;
                    if (isDirectory)
                    {
                        var directory = currentNode as DirectoryNode;
                        Assert.IsTrue(directory != null);
                        directoryStack.Push(directory);
                    }
                }

                // Remove root node from final array
                nodes = nodes[1..];

                //// DEBUG
                //for (int i = 0; i < nodes.Length; i++)
                //{
                //    string fullName = nodes[i].GetResolvedPath();
                //    Console.WriteLine($"{i} {fullName}");
                //}
            }
            // Now that strings are read, we have the final address
            namesAddressRange.RecordEndAddress(reader);
            this.RecordEndAddress(reader);

            // TEMP? Read as block to save out for Dolphin (play via main.dol, etc)
            // Read FST as single block
            reader.JumpToAddress(AddressRange.startAddress);
            reader.Read(ref raw, AddressRange.Size);
            // The address should end where it was, otherwise wrong amount of data read.
            Assert.IsTrue(reader.BaseStream.Position == AddressRange.endAddress);

            // TODO: include files read in here?
            ReadAllFiles(reader);
        }

        public void Serialize(EndianBinaryWriter writer)
        {
            // Prepare graph
            RootNode.AlphabetizeChildrenRecursively();

            // Write temp file system
            this.RecordStartAddress(writer);
            fileSystemNodesAddressRange.RecordStartAddress(writer);
            RootNode.SerializeFileSystemRecursively(writer);
            fileSystemNodesAddressRange.RecordEndAddress(writer);

            // Write file system name table
            namesAddressRange.RecordStartAddress(writer);
            RootNode.SerializeNamesRecursively(writer);
            namesAddressRange.RecordEndAddress(writer);
            this.RecordEndAddress(writer);

            // Write files
            WriteAllFiles(writer);

            // Write actual file system
            Pointer nameTableBasePointer = namesAddressRange.startAddress;
            RootNode.PrepareFileSystemDataRecursively(nameTableBasePointer, 0);
            RootNode.SetAsRootNode();
            //
            writer.JumpToAddress(fileSystemNodesAddressRange.startAddress);
            RootNode.SerializeFileSystemRecursively(writer);
            // Make sure we end at the same spot
            Assert.IsTrue(fileSystemNodesAddressRange.endAddress == writer.GetPositionAsPointer());

            // Jump back to true end...
            writer.JumpToAddress(AddressRange.endAddress);
        }

        /// <summary>
        ///     Get child node of <paramref name="directoryNode"/> named <paramref name="childDirectoryName"/>.
        /// </summary>
        /// <remarks>
        ///     Returns null if node child node is named <paramref name="childDirectoryName"/>.
        /// </remarks>
        /// <param name="directoryNode"></param>
        /// <param name="childDirectoryName"></param>
        /// <returns>
        ///     
        /// </returns>
        public DirectoryNode? GetChildDirectoryNode(DirectoryNode directoryNode, string childDirectoryName)
        {
            foreach (var childNode in directoryNode.Children)
            {
                // Skip files
                if (childNode is not DirectoryNode)
                    continue;

                bool isMatch = childNode.Name == childDirectoryName;
                if (isMatch)
                    return childNode as DirectoryNode;
            }

            // No matches
            return null;
        }

        public void AddFile(string destinationFilePath, byte[] fileData)
        {
            DirectoryNode directoryNode = RootNode;
            string[] pathSegments = GetPathSegments(destinationFilePath);
            string[] directories = pathSegments[..^1];
            string fileName = pathSegments[pathSegments.Length - 1];

            foreach (string directory in directories)
            {
                var childNode = GetChildDirectoryNode(directoryNode, directory);
                // Add new directory node if it does not exist
                if (childNode == null)
                {
                    var newDirectory = new DirectoryNode()
                    {
                        Name = directory,
                    };
                    childNode = newDirectory;
                    childNode.Parent = directoryNode;
                    directoryNode.Children.Add(childNode);
                }
                // Set reference for next iteration of this loop
                directoryNode = childNode;
            }

            FileNode fileNode = new FileNode()
            {
                Name = fileName,
                Data = fileData,
            };
            directoryNode.Children.Add(fileNode);
        }

        public void AddFiles(IEnumerable<string> sourceFilePaths, string destinationRootPath)
        {
            foreach (string sourceFilePath in sourceFilePaths)
            {
                if (!File.Exists(sourceFilePath))
                {
                    string msg = $"File at path \"{sourceFilePath}\" does not exist.";
                    throw new FileNotFoundException(msg);
                }

                string destinationFilePath = sourceFilePath.Replace(destinationRootPath, string.Empty);
                byte[] fileData = File.ReadAllBytes(sourceFilePath);
                AddFile(destinationFilePath, fileData);
            }
        }

        /// <summary>
        ///     Remove the node at the specifiec <paramref name="nodePath"/>.
        /// </summary>
        /// <param name="nodePath">The path within this Filesystem to remove.</param>
        /// <returns>
        ///     True upon successful removal of node, false otherwise.
        /// </returns>
        public bool RemoveNode(string nodePath)
        {
            // See if directory structure matches request file's
            DirectoryNode directoryNode = RootNode;
            string[] pathSegments = GetPathSegments(nodePath);
            string[] directories = pathSegments[..^1];
            string nodeName = pathSegments[pathSegments.Length - 1];

            foreach (string directory in directories)
            {
                var childNode = GetChildDirectoryNode(directoryNode, directory);
                // Quit if file does not exist
                if (childNode == null)
                    return false;
            }

            // If we get here, directories exist. Now, check for final node.
            FileSystemNode? node = null;
            foreach (var child in directoryNode.Children)
            {
                if (child.Name == nodeName)
                {
                    node = child;
                    break;
                }
            }

            // Remove node if it exists
            if (node is not null)
            {
                directoryNode.Children.Remove(node);
                return true;
            }

            // Indicate failed removal otherwise
            return false;
        }

        private string[] GetPathSegments(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                string msg = $"Cannot process null, empty or whitespace-only path.";
                throw new ArgumentException(msg);
            }

            // Clean path directory separator, ensure it does not begin with it, too.
            path = path.Replace('\\', '/');
            if (path.StartsWith('/'))
                path = path[1..];

            //
            string[] pathSegments = path.Split('/');
            return pathSegments;
        }



        public FileNode[] GetFiles()
        {
            List<FileNode> files = new List<FileNode>();
            RootNode.GetFiles(files);
            return files.ToArray();
        }
        public DirectoryNode[] GetDirectories()
        {
            List<DirectoryNode> directories = new List<DirectoryNode>();
            RootNode.GetDirectories(directories);
            return directories.ToArray();
        }
        public int GetNodeCount() => RootNode.GetNodeCount();
        public int GetFileSystemSize() => RootNode.GetNodeCount() * FileSystemNode.StructureSize;
        public int GetNameTableSize() => RootNode.GetNameTableLength();
        public int GetFileSystemSizeOnDisk()
        {
            int size = GetFileSystemSize() + GetNameTableSize();
            return size;
        }

        public void ReadAllFiles(EndianBinaryReader reader)
        {
            var files = GetFiles();
            foreach (var file in files)
                file.ReadData(reader);
        }
        public void WriteAllFiles(EndianBinaryWriter writer)
        {
            var files = GetFiles();
            foreach (var file in files)
            {
                writer.AlignTo(FileAlignment);
                file.WriteData(writer);
            }
        }
    }
}