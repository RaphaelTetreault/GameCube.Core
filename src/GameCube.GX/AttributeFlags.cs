namespace GameCube.GX;

/// <summary>
///     Flag form of Gx.GXAttr
/// </summary>
[System.Flags]
public enum AttributeFlags : uint
{
    /// <summary>
    ///     Position/normal matrix index.
    ///     Note: used for character skinning.
    /// </summary>
    GX_VA_PNMTXIDX = 1 << Attribute.GX_VA_PNMTXIDX,
    /// <summary>
    ///     Texture 0 matrix index.
    ///     Note: used for character skinning.
    /// </summary>
    GX_VA_TEX0MTXIDX = 1 << Attribute.GX_VA_TEX0MTXIDX,
    /// <summary>
    ///     Texture 1 matrix index.
    ///     Note: used for character skinning.
    /// </summary>
    GX_VA_TEX1MTXIDX = 1 << Attribute.GX_VA_TEX1MTXIDX,
    /// <summary>
    ///     Texture 2 matrix index.
    ///     Note: used for character skinning.
    /// </summary>
    GX_VA_TEX2MTXIDX = 1 << Attribute.GX_VA_TEX2MTXIDX,
    /// <summary>
    ///     Texture 3 matrix index.
    ///     Note: used for character skinning.
    /// </summary>
    GX_VA_TEX3MTXIDX = 1 << Attribute.GX_VA_TEX3MTXIDX,
    /// <summary>
    ///     Texture 4 matrix index.
    ///     Note: used for character skinning.
    /// </summary>
    GX_VA_TEX4MTXIDX = 1 << Attribute.GX_VA_TEX4MTXIDX,
    /// <summary>
    ///     Texture 5 matrix index.
    ///     Note: used for character skinning.
    /// </summary>
    GX_VA_TEX5MTXIDX = 1 << Attribute.GX_VA_TEX5MTXIDX,
    /// <summary>
    ///     Texture 6 matrix index.
    ///     Note: used for character skinning.
    /// </summary>
    GX_VA_TEX6MTXIDX = 1 << Attribute.GX_VA_TEX6MTXIDX,
    /// <summary>
    ///     Texture 7 matrix index
    ///     Note: used for character skinning
    /// </summary>
    GX_VA_TEX7MTXIDX = 1 << Attribute.GX_VA_TEX7MTXIDX,

    /// <summary>
    ///     Position.
    /// </summary>
    GX_VA_POS = 1 << Attribute.GX_VA_POS,
    /// <summary>
    ///     Normal.
    /// </summary>
    GX_VA_NRM = 1 << Attribute.GX_VA_NRM,

    /// <summary>
    ///     Color 0.
    /// </summary>
    GX_VA_CLR0 = 1 << Attribute.GX_VA_CLR0,
    /// <summary>
    ///     Color 1.
    /// </summary>
    GX_VA_CLR1 = 1 << Attribute.GX_VA_CLR1,

    /// <summary>
    ///     Input texture coordinate 0.
    /// </summary>
    GX_VA_TEX0 = 1 << Attribute.GX_VA_TEX0,
    /// <summary>
    ///     Input texture coordinate 1.
    /// </summary>
    GX_VA_TEX1 = 1 << Attribute.GX_VA_TEX1,
    /// <summary>
    ///     Input texture coordinate 2.
    /// </summary>
    GX_VA_TEX2 = 1 << Attribute.GX_VA_TEX2,
    /// <summary>
    ///     Input texture coordinate 3.
    /// </summary>
    GX_VA_TEX3 = 1 << Attribute.GX_VA_TEX3,
    /// <summary>
    ///     Input texture coordinate 4.
    /// </summary>
    GX_VA_TEX4 = 1 << Attribute.GX_VA_TEX4,
    /// <summary>
    ///     Input texture coordinate 5.
    /// </summary>
    GX_VA_TEX5 = 1 << Attribute.GX_VA_TEX5,
    /// <summary>
    ///     Input texture coordinate 6.
    /// </summary>
    GX_VA_TEX6 = 1 << Attribute.GX_VA_TEX6,
    /// <summary>
    ///     Input texture coordinate 7.
    /// </summary>
    GX_VA_TEX7 = 1 << Attribute.GX_VA_TEX7,

    /// <summary>
    ///     Position matrix array pointer.
    /// </summary>
    GX_VA_POS_MTX_ARRAY = 1 << Attribute.GX_VA_POS_MTX_ARRAY,
    /// <summary>
    ///     Normal matrix array pointer.
    /// </summary>
    GX_VA_NRM_MTX_ARRAY = 1 << Attribute.GX_VA_NRM_MTX_ARRAY,
    /// <summary>
    ///     Texture matrix array pointer.
    /// </summary>
    GX_VA_TEX_MTX_ARRAY = 1 << Attribute.GX_VA_TEX_MTX_ARRAY,
    /// <summary>
    ///     Light matrix array pointer.
    /// </summary>
    GX_VA_LIGHT_ARRAY = 1 << Attribute.GX_VA_LIGHT_ARRAY,
    /// <summary>
    ///     Normal, bi-normal, tangent.
    /// </summary>
    GX_VA_NBT = 1 << Attribute.GX_VA_NBT,
}