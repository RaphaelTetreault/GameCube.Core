using Manifold.IO;
using System;
using System.Numerics;

namespace GameCube.GX;

/// <summary>
///     
/// </summary>
public static class GXUtility
{
    /// <summary>
    ///     GameCube GPU No-Operation opcode
    /// </summary>
    public const byte GX_NOP = 0x00;

    /// <summary>
    ///     GameCube GPU FIFO alignment.
    /// </summary>
    public const int GX_FIFO_ALIGN = 32;

    public static Vector3 ReadPos(EndianBinaryReader reader, GXComponentCount nElements, GXComponentType componentType, int nFracs)
    {
        if (nElements == GXComponentCount.GX_POS_XYZ)
        {
            return new Vector3(
                ReadNumber(reader, componentType, nFracs),
                ReadNumber(reader, componentType, nFracs),
                ReadNumber(reader, componentType, nFracs));
        }
        else if (nElements == GXComponentCount.GX_POS_XY)
        {
            return new Vector3(
                ReadNumber(reader, componentType, nFracs),
                ReadNumber(reader, componentType, nFracs),
                0f);
        }
        else
        {
            throw new NotImplementedException();
        }
    }
    public static Vector3 ReadNormal(EndianBinaryReader reader, GXComponentCount nElements, GXComponentType componentType, int nFracs)
    {
        // For NBT, the caller of this function should call it 3 times, each for N, B, and T
        if (nElements == GXComponentCount.GX_NRM_XYZ || nElements == GXComponentCount.GX_NRM_NBT)
        {
            return new Vector3(
                ReadNumber(reader, componentType, nFracs),
                ReadNumber(reader, componentType, nFracs),
                ReadNumber(reader, componentType, nFracs));
        }
        else
        {
            throw new NotImplementedException();
        }
    }
    public static Vector2 ReadUV(EndianBinaryReader reader, GXComponentCount nElements, GXComponentType componentType, int nFracs)
    {
        if (nElements == GXComponentCount.GX_TEX_ST)
        {
            return new Vector2(
                ReadNumber(reader, componentType, nFracs),
                ReadNumber(reader, componentType, nFracs));
        }
        else if (nElements == GXComponentCount.GX_TEX_S)
        {
            return new Vector2(
                ReadNumber(reader, componentType, nFracs),
                0f);
        }
        else
        {
            throw new NotImplementedException();
        }
    }
    public static float ReadNumber(EndianBinaryReader reader, GXComponentType type, int nFracBits)
    {
        switch (type)
        {
            case GXComponentType.GX_F32:
                return reader.ReadFloat();

            case GXComponentType.GX_S8:
                return FixedS8ToFloat(reader.ReadInt8(), nFracBits);

            case GXComponentType.GX_U8:
                return FixedU8ToFloat(reader.ReadUInt8(), nFracBits);

            case GXComponentType.GX_S16:
                return FixedS16ToFloat(reader.ReadInt16(), nFracBits);

            case GXComponentType.GX_U16:
                return FixedU16ToFloat(reader.ReadUInt16(), nFracBits);

            default:
                throw new NotImplementedException();
        }
    }

    public static float FixedU8ToFloat(byte value, int nFracBits)
    {
        return (float)value / (1 << nFracBits);
    }
    public static float FixedS8ToFloat(sbyte value, int nFracBits)
    {
        return (float)value / (1 << nFracBits);
    }
    public static float FixedU16ToFloat(ushort value, int nFracBits)
    {
        return (float)value / (1 << nFracBits);
    }
    public static float FixedS16ToFloat(short value, int nFracBits)
    {
        return (float)value / (1 << nFracBits);
    }

    public static void WritePosition(EndianBinaryWriter writer, Vector3 position, GXComponentCount nElements, GXComponentType componentType, int nFracs)
    {
        if (nElements == GXComponentCount.GX_POS_XYZ)
        {
            WriteNumber(writer, position.X, componentType, nFracs);
            WriteNumber(writer, position.Y, componentType, nFracs);
            WriteNumber(writer, position.Z, componentType, nFracs);
        }
        else if (nElements == GXComponentCount.GX_POS_XY)
        {
            WriteNumber(writer, position.X, componentType, nFracs);
            WriteNumber(writer, position.Y, componentType, nFracs);
        }
        else
        {
            throw new ArgumentException();
        }
    }
    public static void WriteNormal(EndianBinaryWriter writer, Vector3 normal, GXComponentCount nElements, GXComponentType componentType, int nFracs)
    {
        // For NBT, the caller of this function should call it 3 times, each for N, B, and T
        if (nElements == GXComponentCount.GX_NRM_XYZ || nElements == GXComponentCount.GX_NRM_NBT)
        {
            WriteNumber(writer, normal.X, componentType, nFracs);
            WriteNumber(writer, normal.Y, componentType, nFracs);
            WriteNumber(writer, normal.Z, componentType, nFracs);
        }
        else
        {
            throw new ArgumentException();
        }
    }
    public static void WriteUV(EndianBinaryWriter writer, Vector2 textureUV, GXComponentCount nElements, GXComponentType componentType, int nFracs)
    {
        if (nElements == GXComponentCount.GX_TEX_ST)
        {
            WriteNumber(writer, textureUV.X, componentType, nFracs);
            WriteNumber(writer, textureUV.Y, componentType, nFracs);
        }
        else if (nElements == GXComponentCount.GX_TEX_S)
        {
            WriteNumber(writer, textureUV.X, componentType, nFracs);
        }
        else
        {
            throw new ArgumentException();
        }
    }
    public static void WriteNumber(EndianBinaryWriter writer, float value, GXComponentType componentType, int nFracs)
    {
        switch (componentType)
        {
            case GXComponentType.GX_F32: writer.Write(value); return;
            case GXComponentType.GX_S16: writer.Write(FloatToFixedS16(value, nFracs)); return;
            case GXComponentType.GX_U16: writer.Write(FloatToFixedU16(value, nFracs)); return;
            case GXComponentType.GX_S8: writer.Write(FloatToFixedS8(value, nFracs)); return;
            case GXComponentType.GX_U8: writer.Write(FloatToFixedU8(value, nFracs)); return;

            default:
                throw new ArgumentException();
        };
    }

    public static byte FloatToFixedU8(float value, int nFracBits)
    {
        return (byte)(value * (1 << nFracBits));
    }
    public static sbyte FloatToFixedS8(float value, int nFracBits)
    {
        return (sbyte)(value * (1 << nFracBits));
    }
    public static ushort FloatToFixedU16(float value, int nFracBits)
    {
        return (ushort)(value * (1 << nFracBits));
    }
    public static short FloatToFixedS16(float value, int nFracBits)
    {
        return (short)(value * (1 << nFracBits));
    }

    public static int GetGxVertexSize(GXAttributeFlags attributes, GXVertexAttributeFormat fmt)
    {
        // Check each component type, see if it is used
        bool hasPNMTXIDX = attributes.HasFlag(GXAttributeFlags.GX_VA_PNMTXIDX);
        bool hasTEX0MTXIDX = attributes.HasFlag(GXAttributeFlags.GX_VA_TEX0MTXIDX);
        bool hasTEX1MTXIDX = attributes.HasFlag(GXAttributeFlags.GX_VA_TEX1MTXIDX);
        bool hasTEX2MTXIDX = attributes.HasFlag(GXAttributeFlags.GX_VA_TEX2MTXIDX);
        bool hasTEX3MTXIDX = attributes.HasFlag(GXAttributeFlags.GX_VA_TEX3MTXIDX);
        bool hasTEX4MTXIDX = attributes.HasFlag(GXAttributeFlags.GX_VA_TEX4MTXIDX);
        bool hasTEX5MTXIDX = attributes.HasFlag(GXAttributeFlags.GX_VA_TEX5MTXIDX);
        bool hasTEX6MTXIDX = attributes.HasFlag(GXAttributeFlags.GX_VA_TEX6MTXIDX);
        bool hasTEX7MTXIDX = attributes.HasFlag(GXAttributeFlags.GX_VA_TEX7MTXIDX);
        bool hasPOS_MTX_ARRAY = attributes.HasFlag(GXAttributeFlags.GX_VA_POS_MTX_ARRAY);
        bool hasNRM_MTX_ARRAY = attributes.HasFlag(GXAttributeFlags.GX_VA_NRM_MTX_ARRAY);
        bool hasTEX_MTX_ARRAY = attributes.HasFlag(GXAttributeFlags.GX_VA_TEX_MTX_ARRAY);
        bool hasLIGHT_ARRAY = attributes.HasFlag(GXAttributeFlags.GX_VA_LIGHT_ARRAY);
        bool hasPOS = attributes.HasFlag(GXAttributeFlags.GX_VA_POS);
        bool hasNRM = attributes.HasFlag(GXAttributeFlags.GX_VA_NRM);
        bool hasNBT = attributes.HasFlag(GXAttributeFlags.GX_VA_NBT);
        bool hasCLR0 = attributes.HasFlag(GXAttributeFlags.GX_VA_CLR0);
        bool hasCLR1 = attributes.HasFlag(GXAttributeFlags.GX_VA_CLR1);
        bool hasTEX0 = attributes.HasFlag(GXAttributeFlags.GX_VA_TEX0);
        bool hasTEX1 = attributes.HasFlag(GXAttributeFlags.GX_VA_TEX1);
        bool hasTEX2 = attributes.HasFlag(GXAttributeFlags.GX_VA_TEX2);
        bool hasTEX3 = attributes.HasFlag(GXAttributeFlags.GX_VA_TEX3);
        bool hasTEX4 = attributes.HasFlag(GXAttributeFlags.GX_VA_TEX4);
        bool hasTEX5 = attributes.HasFlag(GXAttributeFlags.GX_VA_TEX5);
        bool hasTEX6 = attributes.HasFlag(GXAttributeFlags.GX_VA_TEX6);
        bool hasTEX7 = attributes.HasFlag(GXAttributeFlags.GX_VA_TEX7);

        // Don't know what these look like
        if (hasPOS_MTX_ARRAY || hasNRM_MTX_ARRAY || hasTEX_MTX_ARRAY || hasLIGHT_ARRAY)
            throw new NotImplementedException("Unsupported GXAttributes flag");
        
        const int mtxIdxSize = 1;
        int size = 0;
        size += hasPNMTXIDX ? mtxIdxSize : 0;
        size += hasTEX0MTXIDX ? mtxIdxSize : 0;
        size += hasTEX1MTXIDX ? mtxIdxSize : 0;
        size += hasTEX2MTXIDX ? mtxIdxSize : 0;
        size += hasTEX3MTXIDX ? mtxIdxSize : 0;
        size += hasTEX4MTXIDX ? mtxIdxSize : 0;
        size += hasTEX5MTXIDX ? mtxIdxSize : 0;
        size += hasTEX6MTXIDX ? mtxIdxSize : 0;
        size += hasTEX7MTXIDX ? mtxIdxSize : 0;
        size += hasPOS ? CompSizeNumber(fmt.pos.ComponentType) * GetPosCompCount(fmt.pos.NElements) : 0;
        size += hasNRM ? CompSizeNumber(fmt.nrm.ComponentType) * GetNrmCompCount(fmt.nrm.NElements) : 0;
        size += hasNBT ? CompSizeNumber(fmt.nbt.ComponentType) * GetNrmCompCount(fmt.nbt.NElements) : 0;
        size += hasCLR0 ? CompSizeColor(fmt.clr0.ComponentType) : 0;
        size += hasCLR1 ? CompSizeColor(fmt.clr1.ComponentType) : 0;
        size += hasTEX0 ? CompSizeNumber(fmt.tex0.ComponentType) * GetTexCompCount(fmt.tex0.NElements) : 0;
        size += hasTEX1 ? CompSizeNumber(fmt.tex1.ComponentType) * GetTexCompCount(fmt.tex1.NElements) : 0;
        size += hasTEX2 ? CompSizeNumber(fmt.tex2.ComponentType) * GetTexCompCount(fmt.tex2.NElements) : 0;
        size += hasTEX3 ? CompSizeNumber(fmt.tex3.ComponentType) * GetTexCompCount(fmt.tex3.NElements) : 0;
        size += hasTEX4 ? CompSizeNumber(fmt.tex4.ComponentType) * GetTexCompCount(fmt.tex4.NElements) : 0;
        size += hasTEX5 ? CompSizeNumber(fmt.tex5.ComponentType) * GetTexCompCount(fmt.tex5.NElements) : 0;
        size += hasTEX6 ? CompSizeNumber(fmt.tex6.ComponentType) * GetTexCompCount(fmt.tex6.NElements) : 0;
        size += hasTEX7 ? CompSizeNumber(fmt.tex7.ComponentType) * GetTexCompCount(fmt.tex7.NElements) : 0;

        return size;
    }
    private static int GetTexCompCount(GXComponentCount componentCount)
    {
        switch (componentCount)
        {
            case GXComponentCount.GX_TEX_S: return 1;
            case GXComponentCount.GX_TEX_ST: return 2;

            default:
                throw new ArgumentException();
        }
    }
    private static int GetPosCompCount(GXComponentCount componentCount)
    {
        switch (componentCount)
        {
            case GXComponentCount.GX_POS_XY: return 2;
            case GXComponentCount.GX_POS_XYZ: return 3;

            default:
                throw new ArgumentException();
        }
    }
    private static int GetNrmCompCount(GXComponentCount componentCount)
    {
        switch (componentCount)
        {
            case GXComponentCount.GX_NRM_XYZ: return 3;
            case GXComponentCount.GX_NRM_NBT: return 9;
            case GXComponentCount.GX_NRM_NBT3: throw new NotImplementedException();

            default:
                throw new ArgumentException();
        }
    }

    private static int CompSizeNumber(GXComponentType compType)
    {
        switch (compType)
        {
            case GXComponentType.GX_U8: return 1;
            case GXComponentType.GX_S8: return 1;
            case GXComponentType.GX_U16: return 2;
            case GXComponentType.GX_S16: return 2;
            case GXComponentType.GX_F32: return 4;

            default:
                throw new ArgumentException();
        }
    }
    private static int CompSizeColor(GXComponentType compType)
    {
        switch (compType)
        {
            case GXComponentType.GX_RGB565: return 2;
            case GXComponentType.GX_RGB8: return 3;
            case GXComponentType.GX_RGBX8: return 4;
            case GXComponentType.GX_RGBA4: return 2;
            case GXComponentType.GX_RGBA6: return 3;
            case GXComponentType.GX_RGBA8: return 4;

            default:
                throw new ArgumentException();
        }
    }

}