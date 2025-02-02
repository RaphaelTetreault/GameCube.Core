using Manifold;
using Manifold.IO;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace GameCube.Common;

/// <summary>
///     A bounding sphere around a set of points.
/// </summary>
public struct BoundingSphere :
    IBinarySerializable,
    IBinaryAddressable,
    ITextPrintable
{
    // FIELDS
    public Vector3 origin;
    public float radius;


    // PROPERTIES
    public AddressRange AddressRange { get; set; }

    public BoundingSphere()
    {
    }

    public BoundingSphere(Vector3 origin, float radius)
    {
        AddressRange = new AddressRange();
        this.origin = origin;
        this.radius = radius;
    }


    // METHODS
    public void Deserialize(EndianBinaryReader reader)
    {
        var addressRange = new AddressRange();
        addressRange.RecordStartAddress(reader);
        {
            reader.Read(ref origin);
            reader.Read(ref radius);
        }
        addressRange.RecordEndAddress(reader);
        AddressRange = addressRange;
    }

    public void Serialize(EndianBinaryWriter writer)
    {
        var addressRange = new AddressRange();
        addressRange.RecordStartAddress(writer);
        {
            writer.Write(origin);
            writer.Write(radius);
        }
        addressRange.RecordEndAddress(writer);
        AddressRange = addressRange;
    }

    public readonly void PrintMultiLine(System.Text.StringBuilder builder, int indentLevel = 0, string indent = "\t")
    {
        builder.AppendLineIndented(indent, indentLevel, nameof(BoundingSphere));
        indentLevel++;
        builder.AppendLineIndented(indent, indentLevel, $"{nameof(origin)}: {origin}");
        builder.AppendLineIndented(indent, indentLevel, $"{nameof(radius)}: {radius}");
    }

    public readonly string PrintSingleLine()
    {
        return $"{nameof(BoundingSphere)}({nameof(origin)}: {origin}, {nameof(radius)}: {radius})";
    }

    public override readonly string ToString() => PrintSingleLine();

    public static BoundingSphere CreateBoundingSphereFromPoints(IEnumerable<Vector3> points, int length)
    {
        ArgumentNullException.ThrowIfNull(points);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);

        float radius = 0;
        Vector3 center = new();
        float lengthReciprocal = 1f / length;

        // Find the center of gravity for the point 'cloud'.
        foreach (var point in points)
        {
            Vector3 pointWeighted = point * lengthReciprocal;
            center += pointWeighted;
        }

        // Calculate the radius of the needed sphere (it equals the distance between the center and the point further away).
        foreach (var point in points)
        {
            Vector3 centerToPoint = point - center;
            float distance = centerToPoint.Length();

            if (distance > radius)
                radius = distance;
        }

        return new BoundingSphere(center, radius);
    }


}
