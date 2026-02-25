using Latibule.Core.Rendering.Helpers;

namespace Latibule.Core.Rendering.Shapes;

public class PlaneShape : Shape
{
    public PlaneShape()
    {
        Vertices =
        [
            new(-1f, 0f, -1f),
            new(1f, 0f, -1f),
            new(-1f, 0f, 1f),
            new(1f, 0f, 1f)
        ];

        Indices =
        [
            0, 2, 1,
            2, 3, 1
        ];

        Normals =
        [
            new(0f, 1f, 0f),
            new(0f, 1f, 0f),
            new(0f, 1f, 0f),
            new(0f, 1f, 0f)
        ];

        // Base UV coordinates (0-1 range)
        // ShapeRendererComponent will apply scaling and rotation
        Texcoords =
        [
            new(0f, 1f), // Bottom-left
            new(1f, 1f), // Bottom-right
            new(0f, 0f), // Top-left
            new(1f, 0f) // Top-right
        ];
    }
}