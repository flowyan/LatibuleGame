using Latibule.Core.Rendering.Helpers;
using OpenTK.Mathematics;

namespace Latibule.Core.Rendering.Shapes;

/// <summary>
/// Represents an isosphere generated using spherical coordinates (longitude/latitude).
/// </summary>
class IsoSphere : Shape
{
    const double DoublePI = Math.PI * 2.0;

    /// <summary>
    /// Creates an isosphere with the specified tessellation and scale.
    /// </summary>
    /// <param name="longitudeSegments">Number of longitudinal segments (horizontal divisions around the sphere, east-west). Default is 32.</param>
    /// <param name="latitudeSegments">Number of latitudinal segments (vertical divisions from pole to pole, north-south). Default is 16.</param>
    /// <param name="scaleX">Scale factor along the X axis. Default is 1.0.</param>
    /// <param name="scaleY">Scale factor along the Y axis. Default is 1.0.</param>
    /// <param name="scaleZ">Scale factor along the Z axis. Default is 1.0.</param>
    public IsoSphere(int longitudeSegments = 32, int latitudeSegments = 16, float scaleX = 1.0f, float scaleY = 1.0f, float scaleZ = 1.0f)
    {
        // We need (latitudeSegments + 1) rings of vertices from pole to pole
        // Each ring has (longitudeSegments + 1) vertices (with wraparound for texture seam)
        int vertexCount = (latitudeSegments + 1) * (longitudeSegments + 1);

        Vertices = new Vector3[vertexCount];
        Normals = new Vector3[vertexCount];
        Texcoords = new Vector2[vertexCount];

        // Generate vertices
        int vertexIndex = 0;
        for (int lat = 0; lat <= latitudeSegments; lat++)
        {
            // t is the polar angle (latitude): ranges from 0 to π (north pole to south pole)
            double t = Math.PI * lat / latitudeSegments;
            double sinT = Math.Sin(t);
            double cosT = Math.Cos(t);

            for (int lon = 0; lon <= longitudeSegments; lon++)
            {
                // s is the azimuthal angle (longitude): ranges from 0 to 2π (full rotation around the sphere)
                double s = DoublePI * lon / longitudeSegments;

                // Convert spherical coordinates (s, t) to Cartesian coordinates (x, y, z)
                Vertices[vertexIndex].X = scaleX * (float)(Math.Cos(s) * sinT);
                Vertices[vertexIndex].Y = scaleY * (float)cosT;
                Vertices[vertexIndex].Z = scaleZ * (float)(Math.Sin(s) * sinT);

                Normals[vertexIndex] = Vector3.Normalize(Vertices[vertexIndex]);

                // Map spherical coordinates to UV texture coordinates
                Texcoords[vertexIndex].X = (float)lon / longitudeSegments;
                Texcoords[vertexIndex].Y = 1.0f - (float)lat / latitudeSegments;  // Invert V to match texture flip

                vertexIndex++;
            }
        }

        // Generate indices for triangles
        int indexCount = latitudeSegments * longitudeSegments * 6;
        Indices = new uint[indexCount];

        int index = 0;
        for (int lat = 0; lat < latitudeSegments; lat++)
        {
            for (int lon = 0; lon < longitudeSegments; lon++)
            {
                // Calculate the four corners of this quad
                uint topLeft = (uint)(lat * (longitudeSegments + 1) + lon);
                uint topRight = topLeft + 1;
                uint bottomLeft = (uint)((lat + 1) * (longitudeSegments + 1) + lon);
                uint bottomRight = bottomLeft + 1;

                // First triangle (top-left, bottom-left, top-right)
                Indices[index++] = topLeft;
                Indices[index++] = bottomLeft;
                Indices[index++] = topRight;

                // Second triangle (top-right, bottom-left, bottom-right)
                Indices[index++] = topRight;
                Indices[index++] = bottomLeft;
                Indices[index++] = bottomRight;
            }
        }
    }
}