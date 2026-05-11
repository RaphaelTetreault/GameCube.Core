using Manifold.IO;
using System;

namespace GameCube.GX;

/// <summary>
///     Represents a GameCube GPU (GX) color.
/// </summary>
public struct GXColor :
    IBinarySerializable
{
    // FIELDS
    public byte R;
    public byte G;
    public byte B;
    public byte A;
    public GXComponentType ComponentType;


    // CONSTRUCTORS
    public GXColor(GXComponentType componentType)
    {
        R = G = B = A = 0;
        ComponentType = componentType;
    }

    public GXColor(byte r, byte g, byte b, byte a, GXComponentType componentType)
    {
        R = r;
        G = g;
        B = b;
        A = a;
        ComponentType = componentType;
    }

    public GXColor(uint raw, GXComponentType componentType) : this(raw)
    {
        ComponentType = componentType;
    }

    public GXColor(int raw, GXComponentType componentType) : this(raw)
    {
        ComponentType = componentType;
    }

    public GXColor(int raw)
    {
        R = (byte)(raw >>> 24);
        G = (byte)(raw >>> 16);
        B = (byte)(raw >>> 08);
        A = (byte)(raw >>> 00);
        //R = (byte)((raw >> 24) & 0b11111111);
        //G = (byte)((raw >> 16) & 0b11111111);
        //B = (byte)((raw >> 08) & 0b11111111);
        //A = (byte)((raw >> 00) & 0b11111111);
        ComponentType = GXComponentType.GX_RGBA8;
    }

    public GXColor(uint raw)
    {
        R = (byte)(raw >>> 24);
        G = (byte)(raw >>> 16);
        B = (byte)(raw >>> 08);
        A = (byte)(raw >>> 00);
        //R = (byte)((raw >> 24) & 0b11111111);
        //G = (byte)((raw >> 16) & 0b11111111);
        //B = (byte)((raw >> 08) & 0b11111111);
        //A = (byte)((raw >> 00) & 0b11111111);
        ComponentType = GXComponentType.GX_RGBA8;
    }

    public GXColor(byte r, byte g, byte b, byte a = 255)
    {
        R = r;
        G = g;
        B = b;
        A = a;
        ComponentType = GXComponentType.GX_RGBA8;
    }


    // SERIALIZATION
    public void Deserialize(EndianBinaryReader reader)
    {
        switch (ComponentType)
        {
            case GXComponentType.GX_RGB565: ReadRGBA565(reader); break;
            case GXComponentType.GX_RGB8: ReadRGB8(reader); break;
            case GXComponentType.GX_RGBA4: ReadRGBA4(reader); break;
            case GXComponentType.GX_RGBA6: ReadRGBA6(reader); break;
            case GXComponentType.GX_RGBA8: ReadRGBA8(reader); break;
            case GXComponentType.GX_RGBX8: ReadRGBX8(reader); break;

            default:
                throw new ArgumentException("Invalid GXColor type");
        }
    }

    public readonly void Serialize(EndianBinaryWriter writer)
    {
        switch (ComponentType)
        {
            case GXComponentType.GX_RGB565: WriteRGBA565(writer); break;
            case GXComponentType.GX_RGB8: WriteRGB8(writer); break;
            case GXComponentType.GX_RGBA4: WriteRGBA4(writer); break;
            case GXComponentType.GX_RGBA6: WriteRGBA6(writer); break;
            case GXComponentType.GX_RGBA8: WriteRGBA8(writer); break;
            case GXComponentType.GX_RGBX8: WriteRGBX8(writer); break;

            default:
                throw new ArgumentException("Invalid GXColor type");
        }
    }


    // METHODS
    private void GetRGBA8(uint raw)
    {
        R = (byte)(raw >>> 24);
        G = (byte)(raw >>> 16);
        B = (byte)(raw >>> 08);
        A = (byte)(raw >>> 00);
        //R = (byte)((raw >> 24) & 0b11111111);
        //G = (byte)((raw >> 16) & 0b11111111);
        //B = (byte)((raw >> 08) & 0b11111111);
        //A = (byte)((raw >> 00) & 0b11111111);
    }

    private void ReadRGBA565(EndianBinaryReader reader)
    {
        ushort rgb565 = reader.ReadUInt16();

        byte r5 = (byte)(rgb565 >>> 11 /***********/); // implicit truncation
        byte g6 = (byte)(rgb565 >>> 05 & 0b0011_1111); // keep lowest 6 bits
        byte b5 = (byte)(rgb565 >>> 00 & 0b0001_1111); // keep lowest 5 bits
        // Make 8 bit values from 5 or 6 bit values
        // R_B: Lowest 3 bits are the highest of 5 bits
        // _G_: Lowest 2 bits are the highest of 6 bits
        R = (byte)(r5 << 3 | r5 >>> 2);
        G = (byte)(g6 << 2 | b5 >>> 4);
        B = (byte)(b5 << 3 | b5 >>> 2);
        //R = (byte)(((rgb565 >> 11) & (0b_0001_1111)) * (1 << 3));
        //G = (byte)(((rgb565 >> 05) & (0b_0011_1111)) * (1 << 2));
        //B = (byte)(((rgb565 >> 00) & (0b_0001_1111)) * (1 << 3));
    }

    private void ReadRGB8(EndianBinaryReader reader)
    {
        uint rgba8 = Read3BytesCorrectEndianness(reader);
        // Get 8 bits per channel.
        // Final (byte) cast would trim any upper value.
        R = (byte)(rgba8 >>> 16);
        G = (byte)(rgba8 >>> 08);
        B = (byte)(rgba8 >>> 00);
        //R = (byte)((rgba8 >> 16) & (0b_1111_1111));
        //G = (byte)((rgba8 >> 08) & (0b_1111_1111));
        //B = (byte)((rgba8 >> 00) & (0b_1111_1111));
    }

    private void ReadRGBA4(EndianBinaryReader reader)
    {
        ushort rgba4 = reader.ReadUInt16();
        byte r4 = (byte)(rgba4 >>> 12 /***********/); // implicit truncation
        byte g4 = (byte)(rgba4 >>> 08 & 0b0000_1111); // keep lowest 4 bits
        byte b4 = (byte)(rgba4 >>> 04 & 0b0000_1111); // keep lowest 4 bits
        byte a4 = (byte)(rgba4 >>> 00 & 0b0000_1111); // keep lowest 4 bits
        // Make 8 bit values from 4 bit values
        // Lowest 4 bits are the same 4 bits
        R = (byte)(r4 << 4 | r4);
        G = (byte)(g4 << 4 | g4);
        B = (byte)(b4 << 4 | b4);
        A = (byte)(a4 << 4 | a4);
        //R = (byte)(((rgba4 >> 12) & (0b_0000_1111)) * (1 << 4));
        //G = (byte)(((rgba4 >> 08) & (0b_0000_1111)) * (1 << 4));
        //B = (byte)(((rgba4 >> 04) & (0b_0000_1111)) * (1 << 4));
        //A = (byte)(((rgba4 >> 00) & (0b_0000_1111)) * (1 << 4));
    }

    private void ReadRGBA6(EndianBinaryReader reader)
    {
        uint rgba6 = Read3BytesCorrectEndianness(reader);
        // Get individual values, 6 bits each
        byte r6 = (byte)(rgba6 >>> 18 & 0b0011_1111); // keep lowest 6 bits
        byte g6 = (byte)(rgba6 >>> 12 & 0b0011_1111); // keep lowest 6 bits
        byte b6 = (byte)(rgba6 >>> 06 & 0b0011_1111); // keep lowest 6 bits
        byte a6 = (byte)(rgba6 >>> 00 & 0b0011_1111); // keep lowest 6 bits
        // Make 8 bit values from 6 bit values
        // Lowest 2 bits are the highest of 6 bits
        R = (byte)(r6 << 2 | r6 >>> 4);
        G = (byte)(g6 << 2 | g6 >>> 4);
        B = (byte)(b6 << 2 | b6 >>> 4);
        A = (byte)(a6 << 2 | a6 >>> 4);
        //R = (byte)(((rgba6 >> 18) & (0b_0011_1111)) * (1 << 2));
        //G = (byte)(((rgba6 >> 12) & (0b_0011_1111)) * (1 << 2));
        //B = (byte)(((rgba6 >> 06) & (0b_0011_1111)) * (1 << 2));
        //A = (byte)(((rgba6 >> 00) & (0b_0011_1111)) * (1 << 2));
    }

    private void ReadRGBA8(EndianBinaryReader reader)
    {
        uint raw = reader.ReadUInt32();
        GetRGBA8(raw);
    }

    private void ReadRGBX8(EndianBinaryReader reader)
    {
        ReadRGBA8(reader);
        A = 0xFF; // discard alpha
    }

    private static uint Read3BytesCorrectEndianness(EndianBinaryReader reader)
    {
        //// Reconstruct the 24bit color as uint32
        //byte[] bytes = reader.ReadBytes(3);
        //if (reader.IsLittleEndian ^ BitConverter.IsLittleEndian)
        //    Array.Reverse(bytes);
        //uint color32 = BitConverter.ToUInt32(bytes);
        //uint color24 = color32 & 0x00FFFFFF; // only 3 bytes

        // TODO: Validate ordering, check endianess. I assume GX is fixed endianness?
        throw new NotImplementedException();
#pragma warning disable CS0162 // Unreachable code detected
        // I'm assuming order is always RGB regardless of endianness
        byte[] bytes = reader.ReadBytes(3);
        uint color24 = (byte)(bytes[0] << 16 | bytes[1] << 8 | bytes[2] << 0);
        return color24;
#pragma warning restore CS0162 // Unreachable code detected
    }

    private static void Write3BytesCorrectEndianness(EndianBinaryWriter writer, uint color24)
    {
        // TODO: Validate ordering, check endianess. I assume GX is fixed endianness?
        throw new NotImplementedException();

        //var bytes32 = BitConverter.GetBytes(color24);
        //var bytes24 = new byte[3];
        //bytes32.CopyTo(bytes24, 1);

        //if (writer.IsLittleEndian ^ BitConverter.IsLittleEndian)
        //    Array.Reverse(bytes24);

        //writer.Write(bytes24);
    }

    private readonly void WriteRGBA565(EndianBinaryWriter writer)
    {
        //byte r5 = (byte)((R >> 3) & 0b_0001_1111);
        //byte g6 = (byte)((G >> 2) & 0b_0011_1111);
        //byte b5 = (byte)((B >> 3) & 0b_0001_1111);
        byte r5 = (byte)(R >>> 3);
        byte g6 = (byte)(G >>> 2);
        byte b5 = (byte)(B >>> 3);
        ushort rgb565 = (ushort)(r5 << 11 | g6 << 5 | b5 << 0);
        writer.Write(rgb565);
    }

    private readonly void WriteRGB8(EndianBinaryWriter writer)
    {
        uint raw = GetRGBA8();
        Write3BytesCorrectEndianness(writer, raw);
    }

    private readonly void WriteRGBA4(EndianBinaryWriter writer)
    {
        //byte r4 = (byte)((R >> 4) & 0b_0000_1111);
        //byte g4 = (byte)((G >> 4) & 0b_0000_1111);
        //byte b4 = (byte)((B >> 4) & 0b_0000_1111);
        //byte a4 = (byte)((A >> 4) & 0b_0000_1111);
        byte r4 = (byte)(R >>> 4);
        byte g4 = (byte)(G >>> 4);
        byte b4 = (byte)(B >>> 4);
        byte a4 = (byte)(A >>> 4);
        ushort rgba4 = (ushort)(r4 << 12 | g4 << 8 | b4 << 4 | a4 << 0);
        writer.Write(rgba4);
    }

    private readonly void WriteRGBA6(EndianBinaryWriter writer)
    {
        // WRONG SHIFT COUNT!!!
        //byte r6 = (byte)((R >> 6) & 0b_0011_1111);
        //byte g6 = (byte)((G >> 6) & 0b_0011_1111);
        //byte b6 = (byte)((B >> 6) & 0b_0011_1111);
        //byte a6 = (byte)((A >> 6) & 0b_0011_1111);
        byte r6 = (byte)(R >>> 2);
        byte g6 = (byte)(G >>> 2);
        byte b6 = (byte)(B >>> 2);
        byte a6 = (byte)(A >>> 2);
        uint rgba6 = (uint)(r6 << 18 | g6 << 12 | b6 << 6 | a6 << 0);
        Write3BytesCorrectEndianness(writer, rgba6);
    }

    private readonly void WriteRGBA8(EndianBinaryWriter writer)
    {
        uint raw = GetRGBA8();
        writer.Write(raw);
    }

    private readonly void WriteRGBX8(EndianBinaryWriter writer)
    {
        // Write color with fixed alpha
        uint raw = GetRGBA8();
        // Color is: raw & mask alpha | fixed alpha
        // Color is: raw &   RRGGBB-- |   ------AA
        uint color = raw & 0xFFFFFF00 | 0x000000FF;
        writer.Write(color);
    }

    public readonly override string ToString()
    {
        uint raw = GetRGBA8();
        return $"#{raw:x8}";
    }

    private readonly uint GetRGBA8()
    {
        uint value = (uint)(R << 24 | G << 16 | B << 8 | A << 0);
        return value;
    }

}
