using Manifold.IO;
using System;

namespace GameCube.GX;

/// <summary>
///     Vertex Attribute Table (GameCube VAT).
/// </summary>
public class GXVertexAttributeTable
{
    // FIELDS
    private GXVertexAttributeFormat[] gxVertexAttributeFormats = new GXVertexAttributeFormat[8];

    // INDEXERS
    public GXVertexAttributeFormat this[int i]
    {
        get => gxVertexAttributeFormats[i];
    }
    public GXVertexAttributeFormat this[GXVertexFormat vertexFormat]
    {
        get => gxVertexAttributeFormats[(byte)vertexFormat];
    }
    public GXVertexAttributeFormat this[GXDisplayCommand displayCommand]
    {
        get => gxVertexAttributeFormats[displayCommand.VertexFormatIndex];
    }


    public GXVertexAttributeTable(params GXVertexAttributeFormat[] formats)
    {
        if (formats.Length > 8)
            throw new ArgumentOutOfRangeException();

        // Update formats
        for (int i = 0; i < formats.Length; i++)
            gxVertexAttributeFormats[i] = formats[i];

        // Clear old refs
        for (int i = formats.Length; i < gxVertexAttributeFormats.Length; i++)
            gxVertexAttributeFormats[i] = null;
    }

    public bool VatHasAttr(GXDisplayCommand gxCmd, GXAttribute attribute)
    {
        Assert.IsTrue((byte)gxCmd.VertexFormat < 8);

        if (attribute == 0)
        {
            return false;
        }
        else
        {
            var vatIndex = (int)gxCmd.VertexFormat;
            var attr = gxVertexAttributeFormats[vatIndex].GetAttr(attribute);
            return attr != null;
        }
    }

    public bool HasAttr(GXDisplayCommand gxCmd, GXAttributeFlags attribute)
    {
        Assert.IsTrue(gxCmd.VertexFormatIndex < 8);

        if (attribute == 0)
        {
            return false;
        }
        else
        {
            var vatIndex = (int)gxCmd.VertexFormat;
            var vertexAttribute = gxVertexAttributeFormats[vatIndex].GetAttr(attribute);
            return vertexAttribute != null;
        }
    }

}