using GameCube.Common;
using GameCube.DiskImage;
using GameCube.GX.Texture;
using Manifold.IO;
using System;
using System.IO;
using System.Text;

namespace GameCube.GCI;

/// <summary>
///     GCI File System Table Entry.
///     This stub of data belongs in the FST of the GameCube memory card.
/// </summary>
/// <seealso cref="https://github.com/suloku/gcmm/blob/master/source/gci.h"/>
public record struct GciFstEntry :
    IBinaryAddressable,
    IBinarySerializable
{
    // CONSTANTS
    public const Endianness endianness = Endianness.BigEndian;
    public const int Size = 0x40; // 64 bytes
    public const int BlockSize = 0x2000; // 8192
    public const byte Const0x06 = 0xFF;
    public const ushort Const0x3A = 0xFFFF;
    public const int InternalFileNameLength = 32;

    // FIELDS
    private string gameID;                          // 0x00 eg. GFZJ8P
    // Constant 0xFF                                // 0x06 const 0xFF
    private GciBannerIconFlags bannerAndIconFlags;  // 0x07 
    private string internalFileName;                // 0x08 32 bytes including null terminating 0x00
    private uint modificationTime;                  // 0x28 seconds since epoch 2000/01/01
    private Offset imageDataOffset;                 // 0x2C offset after header (0x40) to start of 
    private GciImageFormat imageFormat;             // 0x30 
    private GciAnimationSpeed animationSpeed;       // 0x32 
    private GciPermissionFlags permissionFlags;     // 0x34 
    private byte copyCount;                         // 0x35 Times this GCI has been copied
    private ushort firstBlockIndex;                 // 0x36 
    private ushort blockCount;                      // 0x38 
    // Constant 0xFFFF                              // 0x3A const 0xFFFF
    private Offset commentOffset;                   // 0x3C offset after header (0x40) to start of GCI comment

    public AddressRange AddressRange { get; set; }

    #region Accessors

    /// <summary>
    ///     
    /// </summary>
    public string GameID { readonly get => gameID; set => gameID = value; }

    /// <summary>
    ///     
    /// </summary>
    public GciBannerIconFlags BannerIconFlags
    {
        readonly get => bannerAndIconFlags;
        set => bannerAndIconFlags = SanitizeBannerIconFlags(value);
    }

    /// <summary>
    ///     
    /// </summary>
    public string InternalFileName
    {
        readonly get => internalFileName;
        set => internalFileName = SanitizeInternalFileName(value);
    }

    /// <summary>
    ///     Time of file's last modification in seconds since 12am, January 1st, 2000
    /// </summary>
    public uint ModificationTime { readonly get => modificationTime; set => modificationTime = value; }

    /// <summary>
    ///     
    /// </summary>
    public Offset ImageDataOffset { readonly get => imageDataOffset; set => imageDataOffset = value; }

    /// <summary>
    ///     
    /// </summary>
    public GciImageFormat GciImageFormat { readonly get => imageFormat; set => imageFormat = value; }

    /// <summary>
    ///     
    /// </summary>
    public GciAnimationSpeed GciAnimationSpeed { readonly get => animationSpeed; set => animationSpeed = value; }

    /// <summary>
    ///     
    /// </summary>
    public GciPermissionFlags GciPermissionFlags { readonly get => permissionFlags; set => permissionFlags = value; }

    /// <summary>
    ///     
    /// </summary>
    public byte CopyCount { readonly get => copyCount; set => copyCount = value; }

    /// <summary>
    ///     
    /// </summary>
    public ushort FirstBlockIndex { readonly get => firstBlockIndex; set => firstBlockIndex = value; }

    /// <summary>
    ///     
    /// </summary>
    public ushort BlockCount { readonly get => blockCount; set => blockCount = value; }

    /// <summary>
    ///     
    /// </summary>
    public Offset CommentOffset { readonly get => commentOffset; set => commentOffset = value; }

    #endregion

    public readonly Pointer GetImageDataPtr(Pointer baseAddress) => baseAddress + ImageDataOffset;

    public readonly Pointer GetCommentPtr(Pointer baseAddress) => baseAddress + commentOffset;


    public void Deserialize(EndianBinaryReader reader)
    {
        // Read
        AddressRange addressRange = new();
        addressRange.RecordStartAddress(reader);
        //
        reader.Read(ref gameID, TextEncoding.ShiftJIS, 6);
        reader.AssertValue(reader.ReadByte, Const0x06);
        Encoding encoding = GetTextEncoding();
        reader.Read(ref bannerAndIconFlags);
        reader.Read(ref internalFileName, encoding, InternalFileNameLength);
        reader.Read(ref modificationTime);
        reader.Read(ref imageDataOffset);
        reader.Read(ref imageFormat);
        reader.Read(ref animationSpeed);
        reader.Read(ref permissionFlags);
        reader.Read(ref copyCount);
        reader.Read(ref firstBlockIndex);
        reader.Read(ref blockCount);
        reader.AssertValue(reader.ReadUInt16, Const0x3A);
        reader.Read(ref commentOffset);
        //
        addressRange.RecordEndAddress(reader);
        AddressRange = addressRange;
        // Validation
        Assert.IsTrue(AddressRange.Size == Size);
        BannerIconFlags.Validate();
    }

    public void Serialize(EndianBinaryWriter writer)
    {
        // Validation
        BannerIconFlags.Validate();
        Assert.IsTrue(internalFileName.Length <= InternalFileNameLength);

        // Prep some variables
        int bytesPadInternalFileName = InternalFileNameLength - internalFileName.Length;
        Encoding encoding = GetTextEncoding();

        // Write
        AddressRange addressRange = new();
        addressRange.RecordStartAddress(writer);
        //
        writer.Write(gameID, encoding, false);
        writer.Write(Const0x06);
        writer.Write(bannerAndIconFlags);
        writer.Write(internalFileName, encoding, false);
        writer.WritePadding(0x00, bytesPadInternalFileName);
        writer.Write(modificationTime);
        writer.Write(imageDataOffset);
        writer.Write(imageFormat);
        writer.Write(animationSpeed);
        writer.Write(permissionFlags);
        writer.Write(copyCount);
        writer.Write(firstBlockIndex);
        writer.Write(blockCount);
        writer.Write(Const0x3A);
        writer.Write(commentOffset);
        //
        addressRange.RecordEndAddress(writer);
        AddressRange = addressRange;
        // Validation
        Assert.IsTrue(AddressRange.Size == Size);
    }

    public static uint GetModificationTime(DateTime dateTime)
    {
        DateTime epoch = new(2000, 01, 01);
        TimeSpan timeSpan = dateTime - epoch;
        uint secondsSince2000 = (uint)timeSpan.TotalSeconds;
        return secondsSince2000;
    }

    public static string GetDefaultComment(DateTime dateTime)
    {
        //string time = SaveTime.ToString("yyyy/MM/dd hh:mm.ss");
        string time = dateTime.ToString("yy/MM/dd hh:mm");
        //var assembly = System.Reflection.Assembly.GetEntryAssembly();
        //string? assemblyName = assembly?.GetName().Name;
        //string name = assemblyName is null ? string.Empty : assemblyName;
        string comment = time;
        //string comment = $"Created by {name} at {time}.";
        return comment;
    }

    /// <summary>
    ///     Set filename and prevent file length overflow.
    /// </summary>
    /// <param name="internalFileName"></param>
    /// <returns>
    ///     True if <paramref name="internalFileName"/> fits in character limit, false otherwise.
    /// </returns>
    private static string SanitizeInternalFileName(string internalFileName)
    {
        //TODO: better warnings, keeping the file extension, etc.

        // Trim file name if it is too long
        int maxLength = InternalFileNameLength - 1;
        bool fileNameFits = internalFileName.Length <= maxLength;
        if (!fileNameFits)
        {
            internalFileName = internalFileName[..maxLength];
        }

        // 
        return internalFileName;
    }

    private static GciBannerIconFlags SanitizeBannerIconFlags(GciBannerIconFlags value)
    {
        value.Validate();
        return value;
    }

    /// <summary>
    ///     Get the correct text encoding based on the region of <paramref name="gameID"/>.
    /// </summary>
    /// <param name="gameID">The Game ID for this file.</param>
    /// <returns>
    ///     
    /// </returns>
    /// <exception cref="NotImplementedException">
    ///     Thrown if <paramref name="gameID"/> region code is not implemented.
    /// </exception>
    public readonly Encoding GetTextEncoding()
    {
        char region = GetRegionChar();
        return region switch
        {
            'E' => TextEncoding.Windows1252,
            'J' => TextEncoding.ShiftJIS,
            'P' => TextEncoding.Windows1252,
            _ => throw new NotImplementedException($"Unhandled region code '{region}'."),
        };
    }

    public readonly char GetRegionChar()
    {
        char region = gameID.ToUpper()[3];
        return region;
    }

    public readonly int[] GetAnimationFrameDurations()
    {
        int count = GetAnimationFrameCount();
        int[] durations = new int[count];

        ushort flags = (ushort)animationSpeed;
        for (int i = 0; i < durations.Length; i++)
        {
            int flagsAtIndex = (flags >> i) & 0b11;
            int duration = flagsAtIndex * 4; // 4 frames per each
            durations[i] = duration;
        }

        return durations;
    }

    public readonly int GetAnimationFrameCount()
    {
        int count = 0;
        ushort flags = (ushort)animationSpeed;
        for (int i = 0; i < 8; i++)
        {
            int flagsAtIndex = (flags >> i) & 0b11;
            bool hasFlagsAtIndex = flagsAtIndex != 0;
            if (hasFlagsAtIndex)
                count = i;
        }

        return count;
    }

    public static int ComputeGciPaddingLength(Pointer currentAddress)
    {
        // Account for GCI header as part of file
        Pointer position = currentAddress - Size;
        // Calculate remaining bytes to pad
        int paddingLength = BlockSize - (position % BlockSize);
        return paddingLength;
    }

}
