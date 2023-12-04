using Manifold.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameCube.DiskImage
{
    // TODO: rewrite recursive functions to use type-based switch?
    //       functional paradigm to reuse code?

    public class DirectoryNode : FileSystemNode
    {
        public override FileSystemNodeType Type => FileSystemNodeType.Directory;
        public Offset DirectoryParentNameOffset { get => filePtrOrDirectoryParentOffset; set => filePtrOrDirectoryParentOffset = value; }
        public int DirectoryLastChildIndex { get => fileLengthOrDirectoryLastChildIndex; set => fileLengthOrDirectoryLastChildIndex = value; }
        public int RootNodeCount { get => fileLengthOrDirectoryLastChildIndex; set => fileLengthOrDirectoryLastChildIndex = value; }

        public List<FileSystemNode> Children { get; private set; } = new List<FileSystemNode>();

        /// <summary>
        ///     Add a child to this directory. Assigns proper hierarchical relationships.
        /// </summary>
        /// <param name="node">The node to add as child.</param>
        public void AddChild(FileSystemNode node)
        {
            Children.Add(node);
            node.Parent = this;
        }

        /// <summary>
        ///     Order all children below this node in alphabetically.
        /// </summary>
        internal void AlphabetizeChildrenRecursively()
        {
            // Alphabetize own children
            Children = Children
                .OrderBy(child => child.Name.Value)
                .ToList();

            // Recursively alphabetive children's children
            foreach (var child in Children)
                if (child is DirectoryNode directoryNode)
                    directoryNode.AlphabetizeChildrenRecursively();
        }

        /// <summary>
        ///     Write all files below this directory (inclusive) to the supplied <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        internal void SerializeFileSystemRecursively(EndianBinaryWriter writer)
        {
            // Serialize self (ie: root)
            Serialize(writer);
            Console.WriteLine($"Next {DirectoryLastChildIndex}, {GetResolvedPath()}");

            // Serialize children recursively
            foreach (var child in Children)
            {
                if (child is DirectoryNode directoryNode)
                {
                    directoryNode.SerializeFileSystemRecursively(writer);
                }
                else if (child is FileNode fileNode)
                {
                    fileNode.Serialize(writer);
                    Console.WriteLine(child.GetResolvedPath());
                }
                else
                {
                    throw new FileSystemException("Unable to serialize node.");
                }
            }
        }

        /// <summary>
        ///     Writes all names of files below this directory (inclusive) to the supplied <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        internal void SerializeNamesRecursively(EndianBinaryWriter writer)
        {
            // Serialize self (ie: root)
            writer.Write<AsciiCString>(Name);

            // Serialize children recursively
            foreach (var child in Children)
            {
                if (child is DirectoryNode directoryNode)
                {
                    directoryNode.SerializeNamesRecursively(writer);
                }
                else if (child is FileNode fileNode)
                {
                    writer.Write<AsciiCString>(child.Name);
                }
                else
                {
                    throw new FileSystemException("Unable to serialize node.");
                }
            }
        }

        /// <summary>
        ///     Prepares this node and child nodes' type, name offset pointer, and node index number in
        ///     linearized graph for deserialization.
        /// </summary>
        /// <param name="nameTableBasePointer"></param>
        /// <param name="currentNodeIndex">
        ///     The current node index in linearized tree.Default is 1 (to account for root).
        /// </param>
        internal int PrepareFileSystemDataRecursively(Pointer nameTableBasePointer, int currentNodeIndex = 1)
        {
            PrepareFileSystemData(nameTableBasePointer, currentNodeIndex);

            foreach (var child in Children)
            {
                currentNodeIndex++;

                if (child is DirectoryNode directoryNode)
                    currentNodeIndex = directoryNode.PrepareFileSystemDataRecursively(nameTableBasePointer, currentNodeIndex);
                else
                    child.PrepareFileSystemData(nameTableBasePointer, currentNodeIndex);
            }

            return currentNodeIndex;
        }

        internal override void PrepareFileSystemData(Pointer nameTableBasePointer, int currentIndex)
        {
            base.PrepareFileSystemData(nameTableBasePointer, currentIndex);
            //
            DirectoryParentNameOffset = (Parent is null) ? 0 : Parent.NameOffset;
            DirectoryLastChildIndex = GetChildCountRecursively() + currentIndex;
        }

        /// <summary>
        ///     Gets all files in this directory node and below and adds it to <paramref name="fileNodesList"/>.
        /// </summary>
        /// <param name="fileNodesList">The list of file nodes to add files to.</param>
        internal void GetFiles(List<FileNode> fileNodesList)
        {
            foreach (var child in Children)
                if (child is DirectoryNode directoryNode)
                    directoryNode.GetFiles(fileNodesList);
                else
                    fileNodesList.Add((FileNode)child);
        }

        /// <summary>
        ///     Gets all directories in this directory node and below and adds it to <paramref name="fileNodesList"/>.
        /// </summary>
        /// <param name="directoryNodeList">The list of directory nodes to add directories to.</param>
        internal void GetDirectories(List<DirectoryNode> directoryNodeList)
        {
            directoryNodeList.Add(this);

            foreach (var child in Children)
            {
                if (child is DirectoryNode directoryNode)
                {
                    directoryNode.GetDirectories(directoryNodeList);
                }
            }
        }

        /// <summary>
        ///     Get the total node count from this node down.
        /// </summary>
        /// <returns>
        ///     
        /// </returns>
        internal int GetChildCountRecursively()
        {
            int count = 0;

            // Add child count recursively
            foreach (var child in Children)
                if (child is DirectoryNode directoryNode)
                    count += directoryNode.GetNodeCount();
                else
                    count++;

            return count;
        }

        /// <summary>
        ///     Get the total node count from this node only.
        /// </summary>
        /// <returns>
        ///     
        /// </returns>
        internal int GetNodeCount()
        {
            int count = GetChildCountRecursively() + 1; // 1 for self
            return count;
        }

        /// <summary>
        ///     Compute the size in bytes of the name table from this node down.
        /// </summary>
        /// <returns>
        ///     
        /// </returns>
        internal int GetNameTableLengthRecursively()
        {
            int runningLength = Name.GetSerializedLength();

            // Add child name length recursively
            foreach (var child in Children)
                if (child is DirectoryNode directoryNode)
                    runningLength += directoryNode.GetNameTableLengthRecursively();

            return runningLength;
        }

        /// <summary>
        ///     Prepares the state of this node to be a root node.
        /// </summary>
        internal void SetAsRootNode()
        {
            RootNodeCount = GetNodeCount();
            DirectoryParentNameOffset = 0;
        }

        /// <summary>
        ///     See if this directory contains a child named <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the child.</param>
        /// <returns>
        ///     True if this directory node has a child named <paramref name="name"/>, false otherwise.
        /// </returns>
        internal bool HasChildNamed(string name)
        {
            foreach (var child in Children)
            {
                if (child.Name == name)
                    return true;
            }
            return false;
        }

        /// <summary>
        ///     Remove child named <paramref name="name"/> if contained in this directory node.
        /// </summary>
        /// <param name="name">The name of the child.</param>
        /// <returns>
        ///     True if child named <paramref name="name"/> was successfully removed, false otherwise.
        /// </returns>
        internal bool RemoveChildNamed(string name)
        {
            foreach (var child in Children)
            {
                if (child.Name == name)
                {
                    Children.Remove(child);
                    return true;
                }
            }
            return false;
        }
    }
}
