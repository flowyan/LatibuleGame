using Latibule.Core.Rendering.Helpers;

namespace Latibule.Core.Rendering.Shapes;

public class PlaneWallShape : Shape
{
    public PlaneWallShape()
    {
        Vertices =
        [
            new(1f, 1, 0),
            new(-1f, 1f, 0),
            new(1f, -1f, 0),
            new(-1f, -1f, 0),
        ];

        Indices =
        [
            0, 1, 2,
            2, 1, 3
        ];

        Normals =
        [
            new(0f, 0f, 1f),
            new(0f, 0f, 1f),
            new(0f, 0f, 1f),
            new(0f, 0f, 1f)
        ];

        // Base UV coordinates (0-1 range)
        // ShapeRendererComponent will apply scaling and rotation
        Texcoords =
        [
            new(1f, 1f), // Bottom-right
            new(0f, 1f), // Bottom-left
            new(1f, 0f), // Top-right
            new(0f, 0f), // Top-left
        ];
    }
}