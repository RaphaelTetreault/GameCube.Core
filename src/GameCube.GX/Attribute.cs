namespace GameCube.GX;

/// <summary>
///     Name of vertex attribute or array. Attributes are listed in the ascending order vertex data is required to be sent to the GPU.
/// 
///     Notes:
///     Tells GX what to expect from oncoming vertex information.
///     That data provided should be 32-byte aligned. Refer to GX FIFO.
/// </summary>
public enum Attribute
{
    /// <summary>
    ///     Position/normal matrix index.
    /// </summary>
    GX_VA_PNMTXIDX = 0,
    /// <summary>
    ///     Texture 0 matrix index.
    /// </summary>
    GX_VA_TEX0MTXIDX,
    /// <summary>
    ///     Texture 1 matrix index.
    /// </summary>
    GX_VA_TEX1MTXIDX,
    /// <summary>
    ///     Texture 2 matrix index.
    /// </summary>
    GX_VA_TEX2MTXIDX,
    /// <summary>
    ///     Texture 3 matrix index.
    /// </summary>
    GX_VA_TEX3MTXIDX,
    /// <summary>
    ///     Texture 4 matrix index.
    /// </summary>
    GX_VA_TEX4MTXIDX,
    /// <summary>
    ///     Texture 5 matrix index.
    /// </summary>
    GX_VA_TEX5MTXIDX,
    /// <summary>
    ///     Texture 6 matrix index.
    /// </summary>
    GX_VA_TEX6MTXIDX,
    /// <summary>
    ///     Texture 7 matrix index.
    /// </summary>
    GX_VA_TEX7MTXIDX,

    /// <summary>
    ///     Position.
    /// </summary>
    GX_VA_POS,
    /// <summary>
    ///     Normal.
    /// </summary>
    GX_VA_NRM,

    /// <summary>
    ///     Color 0.
    /// </summary>
    GX_VA_CLR0,
    /// <summary>
    ///     Color 1.
    /// </summary>
    GX_VA_CLR1,

    /// <summary>
    ///     Input texture coordinate 0.
    /// </summary>
    GX_VA_TEX0,
    /// <summary>
    ///     Input texture coordinate 1.
    /// </summary>
    GX_VA_TEX1,
    /// <summary>
    ///     Input texture coordinate 2.
    /// </summary>
    GX_VA_TEX2,
    /// <summary>
    ///     Input texture coordinate 3.
    /// </summary>
    GX_VA_TEX3,
    /// <summary>
    ///     Input texture coordinate 4.
    /// </summary>
    GX_VA_TEX4,
    /// <summary>
    ///     Input texture coordinate 5.
    /// </summary>
    GX_VA_TEX5,
    /// <summary>
    ///     Input texture coordinate 6.
    /// </summary>
    GX_VA_TEX6,
    /// <summary>
    ///     Input texture coordinate 7.
    /// </summary>
    GX_VA_TEX7,

    /// <summary>
    ///     Position matrix array pointer.
    /// </summary>
    GX_VA_POS_MTX_ARRAY,
    /// <summary>
    ///     Normal matrix array pointer.
    /// </summary>
    GX_VA_NRM_MTX_ARRAY,
    /// <summary>
    ///     Texture matrix array pointer.
    /// </summary>
    GX_VA_TEX_MTX_ARRAY,
    /// <summary>
    ///     Light matrix array pointer.
    /// </summary>
    GX_VA_LIGHT_ARRAY,
    /// <summary>
    ///     Normal, bi-normal, tangent.
    /// </summary>
    GX_VA_NBT,
    /// <summary>
    ///     Maximum number of vertex attributes.
    /// </summary>
    GX_VA_MAX_ATTR,

    /// <summary>
    ///     NULL attribute (to mark end of lists).
    /// </summary>
    GX_VA_NULL = 0xff,
}
