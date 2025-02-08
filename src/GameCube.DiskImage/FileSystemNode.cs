// Resources
// https://hitmen.c02.at/files/yagcd/yagcd/chap13.html#sec13.4.1

using Manifold.IO;

namespace GameCube.DiskImage;

/// <summary>
///     Represents a node (file or directory) in the <see cref="FileSystem"/>.
/// </summary>
public class FileSystemNode :
    IBinaryAddressable,
    IBinarySerializable
{
    // CONST    
    public const int StructureSize = 12; // bytes

    // MEMBERS
    internal FileSystemNodeType type; // 1 byte
    internal Offset nameOffset; // 3 bytes
    internal int filePtrOrDirectoryParentOffset;
    internal int fileLengthOrDirectoryLastChildIndex;

    // PROPERTIES
    public AddressRange AddressRange { get; set; }
    public AsciiCString Name { get; set; } = string.Empty;
    public Offset NameOffset { get => nameOffset; set => nameOffset = value; }
    public DirectoryNode? Parent { get; set; } = null;
    public virtual FileSystemNodeType Type { get => type; }


    // METHODS
    public void Deserialize(EndianBinaryReader reader)
    {
        this.RecordStartAddress(reader);
        {
            //
            uint typeAndFileNameOffset = reader.ReadUInt32();
            reader.Read(ref filePtrOrDirectoryParentOffset);
            reader.Read(ref fileLengthOrDirectoryLastChildIndex);

            // Unpack type and file name offset
            type = (FileSystemNodeType)((typeAndFileNameOffset >> 24) & 0xFF);
            nameOffset = typeAndFileNameOffset & 0x00FFFFFF;
        }
        this.RecordEndAddress(reader);
    }

    public void Serialize(EndianBinaryWriter writer)
    {
        this.RecordStartAddress(writer);
        {
            //
            uint typePacked = (uint)Type << 24;
            uint fileNameOffsetPacked = (uint)nameOffset & 0x00FFFFFF;
            uint typeAndFileNameOffset = typePacked | fileNameOffsetPacked;

            //
            writer.Write(typeAndFileNameOffset);
            writer.Write(filePtrOrDirectoryParentOffset);
            writer.Write(fileLengthOrDirectoryLastChildIndex);
        }
        this.RecordEndAddress(writer);
    }

    /// <summary>
    ///     Get the full path of this node.
    /// </summary>
    /// <returns>
    ///     The complete path of this node from the root to the element.
    /// </returns>
    public string GetResolvedPath()
    {
        if (Parent != null)
        {
            string parentName = Parent.GetResolvedPath();
            string name = $"{parentName}/{Name}";
            return name;
        }

        return Name;
    }

    /// <summary>
    ///     Casts node into proper type, FileNode or DirectoryNode.
    /// </summary>
    /// <returns>
    ///     A node but with the proper type.
    /// </returns>
    /// <exception cref="System.Exception">Thrown if 'type' is neither file or directory.</exception>
    public FileSystemNode GetNodeAsProperType()
    {
        FileSystemNode node = type switch
        {
            FileSystemNodeType.File => new FileNode(),
            FileSystemNodeType.Directory => new DirectoryNode(),
            _ => throw new FileSystemException(),
        };

        // Copy values
        node.nameOffset = nameOffset;
        node.filePtrOrDirectoryParentOffset = filePtrOrDirectoryParentOffset;
        node.fileLengthOrDirectoryLastChildIndex = fileLengthOrDirectoryLastChildIndex;

        return node;
    }

    /// <summary>
    ///     Get the root node of this node.
    /// </summary>
    /// <returns>
    ///     The root node of this node.
    ///     Returns self if called on root node.
    /// </returns>
    public DirectoryNode GetRoot()
    {
        if (Parent is not null)
            return Parent.GetRoot();
        else
            return (DirectoryNode)this;
    }

    /// <summary>
    ///     Prepares this node's pointers and other important serialization data.
    /// </summary>
    /// <remarks>
    ///     This function needs a better name.
    /// </remarks>
    /// <param name="nameTableBasePointer">The base pointer of the name table.</param>
    /// <param name="currenIndex">The current node index of the graph when linearized.</param>
    internal virtual void PrepareFileSystemData(Pointer nameTableBasePointer, int currentIndex)
    {
        type = Type;
        nameOffset = Name.GetPointer() - nameTableBasePointer;
    }

    public override string ToString()
    {
        string name = Name == null ? "null" : Name;
        return $"{GetType().Name}({name})";
    }
}
