using Manifold.IO;

namespace GameCube.GX;

/// <summary>
///     
/// </summary>
public class VertexAttribute
{
    private ComponentCount nElements;
    private ComponentType componentFormat;
    private int nFracBits;

    public ComponentCount NElements { get => nElements; set => nElements = value; }
    public ComponentType ComponentType { get => componentFormat; set => componentFormat = value; }
    public int NFracBits { get => nFracBits; set => nFracBits = value; }

    public VertexAttribute(ComponentCount nElements, ComponentType format, int nFracBits = 0)
    {
        // Assert that we aren't shifting more bits than we have
        if (format == ComponentType.GX_S8 | format == ComponentType.GX_U8)
            Assert.IsTrue(nFracBits < 8);
        if (format == ComponentType.GX_S16 | format == ComponentType.GX_U16)
            Assert.IsTrue(nFracBits < 16);
        // Make sure nFracBits is not negative
        Assert.IsTrue(nFracBits >= 0);

        this.nElements = nElements;
        this.componentFormat = format;
        this.nFracBits = nFracBits;
    }
}