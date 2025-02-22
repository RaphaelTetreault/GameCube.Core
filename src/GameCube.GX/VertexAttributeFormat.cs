using System;

namespace GameCube.GX;

/// <summary>
///     What would comprise a column in GX Vertex Attribute Table (VAT).
/// </summary>
/// <remarks>
///     GX Vertex Attribute Format.
/// </remarks>
public class VertexAttributeFormat
{
    public VertexAttribute pos { get; set; }
    public VertexAttribute nrm { get; set; }
    public VertexAttribute nbt { get; set; }
    public VertexAttribute clr0 { get; set; }
    public VertexAttribute clr1 { get; set; }
    public VertexAttribute tex0 { get; set; }
    public VertexAttribute tex1 { get; set; }
    public VertexAttribute tex2 { get; set; }
    public VertexAttribute tex3 { get; set; }
    public VertexAttribute tex4 { get; set; }
    public VertexAttribute tex5 { get; set; }
    public VertexAttribute tex6 { get; set; }
    public VertexAttribute tex7 { get; set; }

    public VertexAttribute GetAttr(Attribute attribute)
    {
        switch (attribute)
        {
            case Attribute.GX_VA_POS: return pos;
            case Attribute.GX_VA_NRM: return nrm;
            case Attribute.GX_VA_NBT: return nbt;
            case Attribute.GX_VA_CLR0: return clr0;
            case Attribute.GX_VA_CLR1: return clr1;
            case Attribute.GX_VA_TEX0: return tex0;
            case Attribute.GX_VA_TEX1: return tex1;
            case Attribute.GX_VA_TEX2: return tex2;
            case Attribute.GX_VA_TEX3: return tex3;
            case Attribute.GX_VA_TEX4: return tex4;
            case Attribute.GX_VA_TEX5: return tex5;
            case Attribute.GX_VA_TEX6: return tex6;
            case Attribute.GX_VA_TEX7: return tex7;

            default:
                throw new ArgumentException();
        }
    }

    public VertexAttribute GetAttr(AttributeFlags attribute)
    {
        switch (attribute)
        {
            case AttributeFlags.GX_VA_POS: return pos;
            case AttributeFlags.GX_VA_NRM: return nrm;
            case AttributeFlags.GX_VA_NBT: return nbt;
            case AttributeFlags.GX_VA_CLR0: return clr0;
            case AttributeFlags.GX_VA_CLR1: return clr1;
            case AttributeFlags.GX_VA_TEX0: return tex0;
            case AttributeFlags.GX_VA_TEX1: return tex1;
            case AttributeFlags.GX_VA_TEX2: return tex2;
            case AttributeFlags.GX_VA_TEX3: return tex3;
            case AttributeFlags.GX_VA_TEX4: return tex4;
            case AttributeFlags.GX_VA_TEX5: return tex5;
            case AttributeFlags.GX_VA_TEX6: return tex6;
            case AttributeFlags.GX_VA_TEX7: return tex7;

            default:
                throw new ArgumentException();
        }
    }

    public void SetAttr(Attribute attribute, VertexAttribute vertexAttribute)
    {
        switch (attribute)
        {
            case Attribute.GX_VA_POS: pos = vertexAttribute; break;
            case Attribute.GX_VA_NRM: nrm = vertexAttribute; break;
            case Attribute.GX_VA_NBT: nbt = vertexAttribute; break;
            case Attribute.GX_VA_CLR0: clr0 = vertexAttribute; break;
            case Attribute.GX_VA_CLR1: clr1 = vertexAttribute; break;
            case Attribute.GX_VA_TEX0: tex0 = vertexAttribute; break;
            case Attribute.GX_VA_TEX1: tex1 = vertexAttribute; break;
            case Attribute.GX_VA_TEX2: tex2 = vertexAttribute; break;
            case Attribute.GX_VA_TEX3: tex3 = vertexAttribute; break;
            case Attribute.GX_VA_TEX4: tex4 = vertexAttribute; break;
            case Attribute.GX_VA_TEX5: tex5 = vertexAttribute; break;
            case Attribute.GX_VA_TEX6: tex6 = vertexAttribute; break;
            case Attribute.GX_VA_TEX7: tex7 = vertexAttribute; break;

            default:
                throw new ArgumentException();
        }
    }
}