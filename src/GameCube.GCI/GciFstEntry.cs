using GameCube.Common;
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
public record struct GciFstEntry :
    IBinarySerializable
{
    public const int Size = 0x40; // 64 bytes
    public const int InternalFileNameLength = 32;
    public static readonly Encoding Windows1252Encoding = TextEncoding.Windows1252;
    public static readonly Encoding ShiftJisEncoding = TextEncoding.ShiftJIS;

    public const Endianness endianness = Endianness.BigEndian;
    public const int BlockSize = 0x2000; // 8192
    public const int MaxFileNameLength = 32;
    public const byte Const0x06 = 0xFF;
    public const ushort Const0x3A = 0xFFFF;

    // FIELDS
    private string gameID;                          // 0x00 eg. GFZJ8P, etc. All codes are *8P instead of *01.
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
        set => bannerAndIconFlags = SanitizeBannerAndIconFlags(value);
    }

    /// <summary>
    ///     
    /// </summary>
    public string InternalFileName
    {
        readonly get => internalFileName;
        set => internalFileName = SanitizeFileName(value);
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
    public GciImageFormat ImageFormat { readonly get => imageFormat; set => imageFormat = value; }

    /// <summary>
    ///     
    /// </summary>
    public GciAnimationSpeed AnimationSpeed { readonly get => animationSpeed; set => animationSpeed = value; }

    /// <summary>
    ///     
    /// </summary>
    public GciPermissionFlags PermissionFlags { readonly get => permissionFlags; set => permissionFlags = value; }

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

    public DateTime SaveTime { get; private set; }

    public readonly Pointer GetImageDataPtr(Pointer baseAddress)
        => baseAddress + ImageDataOffset;

    public readonly Pointer GetCommentPtr(Pointer baseAddress)
        => baseAddress + commentOffset;

    public void Deserialize(EndianBinaryReader reader)
    {
        // Read
        reader.Read(ref gameID, TextEncoding.ShiftJIS, 6);
        reader.AssertValue(reader.ReadByte, Const0x06);
        reader.Read(ref bannerAndIconFlags);
        reader.Read(ref internalFileName, Windows1252Encoding, InternalFileNameLength);
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

        // Validation
        BannerIconFlags.Validate();
    }

    public void Serialize(EndianBinaryWriter writer)
    {
        // Validation
        BannerIconFlags.Validate();
        Assert.IsTrue(internalFileName.Length <= InternalFileNameLength);

        // Prep some variables
        // TODO: ptrs
        SetTime(DateTime.Now);
        int bytesPadInternalFileName = InternalFileNameLength - internalFileName.Length;
        Encoding encoding = GetTextEncoding(gameID);

        // Write
        writer.Write(gameID);
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
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GetDefaultComment()
    {
        //string time = SaveTime.ToString("yyyy/MM/dd hh:mm.ss");
        string time = SaveTime.ToString("yy/MM/dd hh:mm");
        var assembly = System.Reflection.Assembly.GetEntryAssembly();
        string? assemblyName = assembly?.GetName().Name;
        string name = assemblyName is null ? string.Empty : assemblyName;
        string comment = time;
        //string comment = $"Created by {name} at {time}.";
        return comment;
    }

    /// <summary>
    ///     Sets the header timestamp to the provided <paramref name="dateTime"/>.
    /// </summary>
    /// <param name="dateTime">The time to use.</param>
    private void SetTime(DateTime dateTime)
    {
        DateTime epoch = new(2000, 01, 01);
        TimeSpan timeSpan = dateTime - epoch;
        uint secondsSince2000 = (uint)timeSpan.TotalSeconds;
        modificationTime = secondsSince2000;
        //
        SaveTime = dateTime;
    }



    /// <summary>
    ///     Set filename and prevent file length overflow.
    /// </summary>
    /// <param name="internalFileName"></param>
    /// <returns>
    ///     True if <paramref name="internalFileName"/> fits in character limit, false otherwise.
    /// </returns>
    public static string SanitizeFileName(string internalFileName)
    {
        //TODO 2026/04/26:
        //  Key insight, internal file name is what hangs up game...
        //  Must be .dat extension in file. Causes file loading hang otherwise.
        //  Must have fze020 for whatever reason. Causes pointer issues.
        //  To that point. file is fze_02000_02000 (no _ in actual). 02000 repeats twice.

        // TODO: manage this better
        string fileName = Path.GetFileNameWithoutExtension(internalFileName);
        string extension = Path.GetExtension(internalFileName);
        if (extension != ".dat")
        {
            string msg = $"Internal file name must end in \".dat!\"";
            throw new ArgumentException(msg);
        }

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

    public static GciBannerIconFlags SanitizeBannerAndIconFlags(GciBannerIconFlags value)
    {
        if ((value & GciBannerIconFlags.Metadata_InvalidBanner) == GciBannerIconFlags.Metadata_InvalidBanner)
        {
            string msg =
                $"{nameof(BannerIconFlags)} cannot be both " +
                $"{nameof(GciBannerIconFlags.IndirectColorCI8)} and " +
                $"{nameof(GciBannerIconFlags.DirectColorRGB5A3)}.";
            throw new ArgumentException(msg);
        }

        return value;
    }

    /// <summary>
    ///     Get the correct text endoing based on the region of <paramref name="gameID"/>.
    /// </summary>
    /// <param name="gameID">The Game ID for this file.</param>
    /// <returns>
    ///     
    /// </returns>
    /// <exception cref="NotImplementedException">
    ///     Thrown if <paramref name="gameID"/> region code is not implemented.
    /// </exception>
    private static Encoding GetTextEncoding(string gameID)
    {
        char region = gameID.ToUpper()[4];
        return region switch
        {
            'E' => Windows1252Encoding,
            'J' => ShiftJisEncoding,
            'P' => Windows1252Encoding,
            _ => throw new NotImplementedException($"Unhandled region code '{region}'."),
        };
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
