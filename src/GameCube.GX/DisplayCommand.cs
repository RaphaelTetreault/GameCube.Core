using Manifold.IO;

namespace GameCube.GX;

/// <summary>
///     
/// </summary>
public class DisplayCommand :
    IBinarySerializable
{
    // CONST
    private const byte kPrimitiveMask = 0b00000111; // 3 lowest bits
    private const byte kVertexFormatMask = 0b11111000; // 5 highest bits


    // FIELDS
    private byte command;
    private Primitive primitive;
    private VertexFormat vertexFormat;


    // PROPERTIES
    public Primitive Primitive
    {
        get => primitive;
        set
        {
            primitive = value;
            command &= kPrimitiveMask;
            command |= (byte)primitive;
        }
    }
    public VertexFormat VertexFormat
    {
        get => vertexFormat;
        set
        {
            vertexFormat = value;
            command &= kVertexFormatMask;
            command |= (byte)vertexFormat;
        }
    }
    public byte VertexFormatIndex => (byte)VertexFormat;


    // METHODS
    public void Deserialize(EndianBinaryReader reader)
    {
        reader.Read(ref command);
        primitive = (Primitive)(command & kVertexFormatMask); 
        vertexFormat = (VertexFormat)(command & kPrimitiveMask); 
    }

    public void Serialize(EndianBinaryWriter writer)
    {
        writer.Write(command);
    }

}