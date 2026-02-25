using Latibule.Core.Rendering.Helpers;

namespace Latibule.Core.Rendering.Shapes;

public class Cube : Shape
{
    public Cube()
    {
        Vertices =
        [
            // FRONT (+Z)
            new(-1, -1, 1),
            new(1, -1, 1),
            new(1, 1, 1),
            new(-1, 1, 1),

            // BACK (-Z)
            new(1, -1, -1),
            new(-1, -1, -1),
            new(-1, 1, -1),
            new(1, 1, -1),

            // LEFT (-X)
            new(-1, -1, -1),
            new(-1, -1, 1),
            new(-1, 1, 1),
            new(-1, 1, -1),

            // RIGHT (+X)
            new(1, -1, 1),
            new(1, -1, -1),
            new(1, 1, -1),
            new(1, 1, 1),

            // TOP (+Y)
            new(-1, 1, 1),
            new(1, 1, 1),
            new(1, 1, -1),
            new(-1, 1, -1),

            // BOTTOM (-Y)
            new(-1, -1, -1),
            new(1, -1, -1),
            new(1, -1, 1),
            new(-1, -1, 1),
        ];

        Indices =
        [
            0, 1, 2, 2, 3, 0, // Front
            4, 5, 6, 6, 7, 4, // Back
            8, 9, 10, 10, 11, 8, // Left
            12, 13, 14, 14, 15, 12, // Right
            16, 17, 18, 18, 19, 16, // Top
            20, 21, 22, 22, 23, 20 // Bottom
        ];

        Normals =
        [
            // Front
            new(0, 0, 1),
            new(0, 0, 1),
            new(0, 0, 1),
            new(0, 0, 1),

            // Back
            new(0, 0, -1),
            new(0, 0, -1),
            new(0, 0, -1),
            new(0, 0, -1),

            // Left
            new(-1, 0, 0),
            new(-1, 0, 0),
            new(-1, 0, 0),
            new(-1, 0, 0),

            // Right
            new(1, 0, 0),
            new(1, 0, 0),
            new(1, 0, 0),
            new(1, 0, 0),

            // Top
            new(0, 1, 0),
            new(0, 1, 0),
            new(0, 1, 0),
            new(0, 1, 0),

            // Bottom
            new(0, -1, 0),
            new(0, -1, 0),
            new(0, -1, 0),
            new(0, -1, 0),
        ];

        Texcoords =
        [
            // Front
            new(0, 0),
            new(1, 0),
            new(1, 1),
            new(0, 1),

            // Back
            new(0, 0),
            new(1, 0),
            new(1, 1),
            new(0, 1),

            // Left
            new(0, 0),
            new(1, 0),
            new(1, 1),
            new(0, 1),

            // Right
            new(0, 0),
            new(1, 0),
            new(1, 1),
            new(0, 1),

            // Top
            new(0, 0),
            new(1, 0),
            new(1, 1),
            new(0, 1),

            // Bottom
            new(0, 0),
            new(1, 0),
            new(1, 1),
            new(0, 1),
        ];
    }
}