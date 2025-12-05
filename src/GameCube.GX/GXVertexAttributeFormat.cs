using System;

namespace GameCube.GX;

/// <summary>
///     What would comprise a column in GX Vertex Attribute Table (VAT).
/// </summary>
/// <remarks>
///     GX Vertex Attribute Format.
/// </remarks>
public class GXVertexAttributeFormat
{
    public GXVertexAttribute pos { get; set; }
    public GXVertexAttribute nrm { get; set; }
    public GXVertexAttribute nbt { get; set; }
    public GXVertexAttribute clr0 { get; set; }
    public GXVertexAttribute clr1 { get; set; }
    public GXVertexAttribute tex0 { get; set; }
    public GXVertexAttribute tex1 { get; set; }
    public GXVertexAttribute tex2 { get; set; }
    public GXVertexAttribute tex3 { get; set; }
    public GXVertexAttribute tex4 { get; set; }
    public GXVertexAttribute tex5 { get; set; }
    public GXVertexAttribute tex6 { get; set; }
    public GXVertexAttribute tex7 { get; set; }

    public GXVertexAttribute GetAttr(GXAttribute attribute)
    {
        switch (attribute)
        {
            case GXAttribute.GX_VA_POS: return pos;
            case GXAttribute.GX_VA_NRM: return nrm;
            case GXAttribute.GX_VA_NBT: return nbt;
            case GXAttribute.GX_VA_CLR0: return clr0;
            case GXAttribute.GX_VA_CLR1: return clr1;
            case GXAttribute.GX_VA_TEX0: return tex0;
            case GXAttribute.GX_VA_TEX1: return tex1;
            case GXAttribute.GX_VA_TEX2: return tex2;
            case GXAttribute.GX_VA_TEX3: return tex3;
            case GXAttribute.GX_VA_TEX4: return tex4;
            case GXAttribute.GX_VA_TEX5: return tex5;
            case GXAttribute.GX_VA_TEX6: return tex6;
            case GXAttribute.GX_VA_TEX7: return tex7;

            default:
                throw new ArgumentException();
        }
    }

    public GXVertexAttribute GetAttr(GXAttributeFlags attribute)
    {
        switch (attribute)
        {
            case GXAttributeFlags.GX_VA_POS: return pos;
            case GXAttributeFlags.GX_VA_NRM: return nrm;
            case GXAttributeFlags.GX_VA_NBT: return nbt;
            case GXAttributeFlags.GX_VA_CLR0: return clr0;
            case GXAttributeFlags.GX_VA_CLR1: return clr1;
            case GXAttributeFlags.GX_VA_TEX0: return tex0;
            case GXAttributeFlags.GX_VA_TEX1: return tex1;
            case GXAttributeFlags.GX_VA_TEX2: return tex2;
            case GXAttributeFlags.GX_VA_TEX3: return tex3;
            case GXAttributeFlags.GX_VA_TEX4: return tex4;
            case GXAttributeFlags.GX_VA_TEX5: return tex5;
            case GXAttributeFlags.GX_VA_TEX6: return tex6;
            case GXAttributeFlags.GX_VA_TEX7: return tex7;

            default:
                throw new ArgumentException();
        }
    }

    public void SetAttr(GXAttribute attribute, GXVertexAttribute vertexAttribute)
    {
        switch (attribute)
        {
            case GXAttribute.GX_VA_POS: pos = vertexAttribute; break;
            case GXAttribute.GX_VA_NRM: nrm = vertexAttribute; break;
            case GXAttribute.GX_VA_NBT: nbt = vertexAttribute; break;
            case GXAttribute.GX_VA_CLR0: clr0 = vertexAttribute; break;
            case GXAttribute.GX_VA_CLR1: clr1 = vertexAttribute; break;
            case GXAttribute.GX_VA_TEX0: tex0 = vertexAttribute; break;
            case GXAttribute.GX_VA_TEX1: tex1 = vertexAttribute; break;
            case GXAttribute.GX_VA_TEX2: tex2 = vertexAttribute; break;
            case GXAttribute.GX_VA_TEX3: tex3 = vertexAttribute; break;
            case GXAttribute.GX_VA_TEX4: tex4 = vertexAttribute; break;
            case GXAttribute.GX_VA_TEX5: tex5 = vertexAttribute; break;
            case GXAttribute.GX_VA_TEX6: tex6 = vertexAttribute; break;
            case GXAttribute.GX_VA_TEX7: tex7 = vertexAttribute; break;

            default:
                throw new ArgumentException();
        }
    }
}