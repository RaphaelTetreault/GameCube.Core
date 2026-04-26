//using GameCube.DiskImage;
//using Manifold.IO;
//using System;
//using System.IO;

//namespace GameCube.GCI;

///// <summary>
/////     Raw GameCube GCI save data header.
///// </summary>
///// <example>
/////     Primary source of information:
/////     https://docs.google.com/document/d/1c4a7d6xZ-rnK-E5p7d__6V1qhLxcdOHwAmv-FR8K50s/edit
/////     TODO: https://github.com/dolphin-emu/dolphin/blob/master/Source/Core/Core/HW/GCMemcard/GCMemcard.h#L240
///// </example>
//public class GciHeader :
//    IBinarySerializable
//{
//    // Const / readonly
//    public const int HeaderSize = 0x40; // FS entry?
//    public const int BlockSize = 0x2000;
//    public const int InternalFileNameLength = 32;
//    public static readonly System.Text.Encoding Windows1252Encoding = System.Text.Encoding.GetEncoding(1252); // Windows-1252
//    public static readonly System.Text.Encoding ShiftJisEncoding = System.Text.Encoding.GetEncoding(932); // shift-jis code page

//    // Fields
//    private GameID gameID = new();
//    private byte const_0xFF;
//    private GciBannerIconFlags bannerAndIconFlags;
//    private string fileName = string.Empty;
//    private uint modificationTime;
//    private Offset imageDataOffset;
//    private GciImageFormat imageFormat;
//    private GciAnimationSpeed animationSpeed;
//    private GciPermissionFlags permissionFlags;
//    private byte copyCount;
//    private ushort firstBlockIndex; // on memory card AFAICT
//    private ushort blockCount;
//    private ushort const_0xFFFF; // FFFF
//    private Offset commentOffset;

//    // Accessors
//    public GameID GameID { get => gameID; set => gameID = value; }
//    public GciBannerIconFlags BannerAndIconFlags { get => bannerAndIconFlags; set => bannerAndIconFlags = value; }
//    public string FileName { get => fileName; set => fileName = value; }
//    /// <summary>
//    ///     Time of file's last modification in seconds since 12am, January 1st, 2000
//    /// </summary>
//    public uint ModificationTime { get => modificationTime; set => modificationTime = value; }
//    public Offset ImageDataOffset { get => imageDataOffset; set => imageDataOffset = value; }
//    public GciImageFormat ImageFormat { get => imageFormat; set => imageFormat = value; }
//    public GciAnimationSpeed AnimationSpeed { get => animationSpeed; set => animationSpeed = value; }
//    public GciPermissionFlags PermissionFlags { get => permissionFlags; set => permissionFlags = value; }
//    public byte CopyCount { get => copyCount; set => copyCount = value; }
//    public ushort FirstBlockIndex { get => firstBlockIndex; set => firstBlockIndex = value; }
//    public ushort BlockCount { get => blockCount; set => blockCount = value; }
//    public Offset CommentOffset { get => commentOffset; set => commentOffset = value; }
//    public DateTime SaveTime { get; private set; }

//    public Pointer ImageDataPtr { get; private set; }
//    public Pointer CommentPtr { get; private set; }
//    public int Size => blockCount * BlockSize + HeaderSize;

//    // Methods
//    public void Deserialize(EndianBinaryReader reader)
//    {
//        reader.Read(ref gameID);
//        reader.Read(ref const_0xFF);
//        reader.Read(ref bannerAndIconFlags);
//        reader.Read(ref fileName, Windows1252Encoding, InternalFileNameLength);
//        reader.Read(ref modificationTime);
//        reader.Read(ref imageDataOffset);
//        reader.Read(ref imageFormat);
//        reader.Read(ref animationSpeed);
//        reader.Read(ref permissionFlags);
//        reader.Read(ref copyCount);
//        reader.Read(ref firstBlockIndex);
//        reader.Read(ref blockCount);
//        reader.Read(ref const_0xFFFF);
//        reader.Read(ref commentOffset);
//        // Prepare pointers
//        Pointer currentAddress = reader.GetPositionAsPointer();
//        CommentPtr = currentAddress + commentOffset;
//        ImageDataPtr = currentAddress + imageDataOffset;

//        Assert.IsTrue(const_0xFF == 0xFF);
//        Assert.IsTrue(const_0xFFFF == 0xFFFF);
//    }

//    public void Serialize(EndianBinaryWriter writer)
//    {
//        Assert.IsTrue(fileName.Length <= InternalFileNameLength);
//        // TODO: ptrs
//        SetTime(DateTime.Now);

//        writer.Write(gameID);
//        writer.Write((byte)0xFF);
//        writer.Write(bannerAndIconFlags);
//        writer.Write(fileName, GetTextEncoding(gameID), false);
//        writer.WritePadding(0x00, InternalFileNameLength - fileName.Length);
//        writer.Write(modificationTime);
//        writer.Write(imageDataOffset);
//        writer.Write(imageFormat);
//        writer.Write(animationSpeed);
//        writer.Write(permissionFlags);
//        writer.Write(copyCount);
//        writer.Write(firstBlockIndex);
//        writer.Write(blockCount);
//        writer.Write((ushort)0xFFFF);
//        writer.Write(commentOffset);
//    }

//    /// <summary>
//    /// 
//    /// </summary>
//    /// <returns></returns>
//    public string GetDefaultComment()
//    {
//        //string time = SaveTime.ToString("yyyy/MM/dd hh:mm.ss");
//        string time = SaveTime.ToString("yy/MM/dd hh:mm");
//        var assembly = System.Reflection.Assembly.GetEntryAssembly();
//        string? assemblyName = assembly?.GetName().Name;
//        string name = assemblyName is null ? string.Empty : assemblyName;
//        string comment = time;
//        //string comment = $"Created by {name} at {time}.";
//        return comment;
//    }

//    /// <summary>
//    ///     Sets the header timestamp to the provided <paramref name="dateTime"/>.
//    /// </summary>
//    /// <param name="dateTime">The time to use.</param>
//    private void SetTime(DateTime dateTime)
//    {
//        DateTime epoch = new(2000, 01, 01);
//        TimeSpan timeSpan = dateTime - epoch;
//        uint secondsSince2000 = (uint)timeSpan.TotalSeconds;
//        modificationTime = secondsSince2000;
//        //
//        SaveTime = dateTime;
//    }



//    /// <summary>
//    ///     Set filename and prevent file length overflow.
//    /// </summary>
//    /// <param name="internalFileName"></param>
//    /// <returns>
//    ///     True if <paramref name="internalFileName"/> fits in character limit, false otherwise.
//    /// </returns>
//    public bool SafeSetInternalFileName(string internalFileName)
//    {
//        // TODO: manage this better
//        string fileName = Path.GetFileNameWithoutExtension(internalFileName);
//        string extension = Path.GetExtension(internalFileName);
//        if (extension != ".dat")
//        {
//            string msg = $"Internal file name must end in \".dat!\"";
//            throw new ArgumentException(msg);
//        }

//        // Trim file name if it is too long
//        int maxLength = InternalFileNameLength - 1;
//        bool fileNameFits = internalFileName.Length <= maxLength;
//        if (!fileNameFits)
//        {
//            internalFileName = internalFileName[..maxLength];
//        }

//        // Set file name
//        FileName = internalFileName;

//        // provide info
//        return fileNameFits;
//    }

//    /// <summary>
//    ///     Get the correct text endoing based on the region of <paramref name="gameID"/>.
//    /// </summary>
//    /// <param name="gameID">The Game ID for this file.</param>
//    /// <returns>
//    ///     
//    /// </returns>
//    /// <exception cref="NotImplementedException">
//    ///     Thrown if <paramref name="gameID"/> region code is not implemented.
//    /// </exception>
//    private static System.Text.Encoding GetTextEncoding(GameID gameID)
//    {
//        return gameID.RegionCode switch
//        {
//            'E' => Windows1252Encoding,
//            'J' => ShiftJisEncoding,
//            'P' => Windows1252Encoding,
//            _ => throw new NotImplementedException($"Unhandled region code '{gameID.RegionCode}'."),
//        };
//    }
//    public System.Text.Encoding GetTextEncoding()
//    {
//        return GetTextEncoding(GameID);
//    }

//    public int[] GetAnimationFrameDurations()
//    {
//        int count = GetAnimationFrameCount();
//        int[] durations = new int[count];

//        ushort flags = (ushort)animationSpeed;
//        for (int i = 0; i < durations.Length; i++)
//        {
//            int flagsAtIndex = ((flags >> i) & 0b11);
//            int duration = flags * 4; // 4 frames per each
//            durations[i] = duration;
//        }

//        return durations;
//    }
//    public int GetAnimationFrameCount()
//    {
//        int count = 0;
//        ushort flags = (ushort)animationSpeed;
//        for (int i = 0; i < 8; i++)
//        {
//            int flagsAtIndex = (flags >> i) & 0b11;
//            bool hasFlagsAtIndex = flagsAtIndex != 0;
//            if (hasFlagsAtIndex)
//                count = i;
//        }

//        return count;
//    }

//}
