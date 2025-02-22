using Manifold.IO;

namespace GameCube.DiskImage;

/// <summary>
///     File node within the <see cref="FileSystem"/>.
/// </summary>
public class FileNode : FileSystemNode
{
    public override FileSystemNodeType Type => FileSystemNodeType.File;

    public Pointer FilePointer { get => filePtrOrDirectoryParentOffset; set => filePtrOrDirectoryParentOffset = value; }
    public int FileLength { get => fileLengthOrDirectoryLastChildIndex; set => fileLengthOrDirectoryLastChildIndex = value; }
    public byte[] Data { get; set; } = [];

    /// <summary>
    ///     Read this node's file data using it's FilePointer and the supplied <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    public void ReadData(EndianBinaryReader reader)
    {
        reader.JumpToAddress(FilePointer);
        Data = reader.ReadBytes(FileLength);
    }

    /// <summary>
    ///     Write this node's file data to the <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    public void WriteData(EndianBinaryWriter writer)
    {
        FilePointer = writer.GetPositionAsPointer();
        FileLength = Data.Length;
        writer.Write(Data);
    }

}
