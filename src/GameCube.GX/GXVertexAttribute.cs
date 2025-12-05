using Manifold.IO;

namespace GameCube.GX;

/// <summary>
///     
/// </summary>
public class GXVertexAttribute
{
    private GXComponentCount nElements;
    private GXComponentType componentFormat;
    private int nFracBits;

    public GXComponentCount NElements { get => nElements; set => nElements = value; }
    public GXComponentType ComponentType { get => componentFormat; set => componentFormat = value; }
    public int NFracBits { get => nFracBits; set => nFracBits = value; }

    public GXVertexAttribute(GXComponentCount nElements, GXComponentType format, int nFracBits = 0)
    {
        // Assert that we aren't shifting more bits than we have
        if (format == GXComponentType.GX_S8 | format == GXComponentType.GX_U8)
            Assert.IsTrue(nFracBits < 8);
        if (format == GXComponentType.GX_S16 | format == GXComponentType.GX_U16)
            Assert.IsTrue(nFracBits < 16);
        // Make sure nFracBits is not negative
        Assert.IsTrue(nFracBits >= 0);

        this.nElements = nElements;
        this.componentFormat = format;
        this.nFracBits = nFracBits;
    }
}