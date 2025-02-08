using Manifold.IO;
using System;
using System.Text;

namespace GameCube.DiskImage;

/// <summary>
///     GameCube game ID.
/// </summary>
public struct GameID :
    IBinarySerializable
{
    // CONSTANTS
    private const int ByteLength = 6;
    private readonly Encoding encoding = Encoding.ASCII;


    // FIELDS
    private byte[] characters;


    // PROPERTIES
    public readonly char ConsoleCode
    {
        get => this[0];
        set => this[0] = value;
    }
    public readonly ushort GameCode
    {
        get => (ushort)(characters[1] << 8 | characters[2] << 0);
        set
        {
            characters[1] = (byte)((value >> 8) & 0xFF);
            characters[2] = (byte)((value >> 0) & 0xFF);
        }
    }
    public readonly char RegionCode
    {
        get => this[3];
        set => this[3] = value;
    }
    public readonly ushort DeveloperCode
    {
        get => (ushort)(characters[4] << 8 | characters[5] << 0);
        set
        {
            characters[4] = (byte)((value >> 8) & 0xFF);
            characters[5] = (byte)((value >> 0) & 0xFF);
        }
    }
    public readonly byte[] CharactersRaw => characters;


    // CONSTRUCTORS
    public GameID()
    {
        characters = new byte[ByteLength];
    }


    // INDEXERS
    public readonly char this[int i]
    {
        get => Convert.ToChar(characters[i]);
        set => characters[i] = Convert.ToByte(value);
    }


    // METHODS
    public void Deserialize(EndianBinaryReader reader)
    {
        reader.Read(ref characters, ByteLength);
        ThrowIfInvalidRegion();
    }
    public readonly void Serialize(EndianBinaryWriter writer)
    {
        ThrowIfInvalidRegion();
        writer.Write(characters);
    }
    public readonly string GetAsString()
    {
        var str = encoding.GetString(characters);
        return str;
    }
    public override readonly string ToString()
    {
        return GetAsString();
    }
    public static char GetRegionChar(Region region)
    {
        return region switch
        {
            // TODO: Datel 'X', region free?
            Region.NorthAmerica => 'E',
            Region.Europe => 'P',
            Region.Japan => 'J',
            Region.RegionFree => throw new NotImplementedException($"{region}"),
            _ => '\0',
        };
    }
    private readonly bool IsValidRegionChar()
    {
        return RegionCode switch
        {
            'E' or 'J' or 'P' => true,
            _ => false,
        };
    }
    public readonly void ThrowIfInvalidRegion()
    {
        bool isInvalidRegion = IsValidRegionChar();
        if (!isInvalidRegion)
        {
            string msg = $"Invalid region code '{RegionCode}'.";
            throw new NotImplementedException(msg);
        }
    }
}
