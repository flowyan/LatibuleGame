using OpenTK.Mathematics;

namespace Latibule.Core.ECS;

public class Transform
{
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Scale { get; set; } = Vector3.One;
    public Vector3 Rotation { get; set; } = Vector3.Zero;

        public Transform() { }

        public Transform(Vector3 position, Vector3 scale, Vector3 rotation)
        {
            Position = position;
            Scale = scale;
            Rotation = rotation;
        }

        public Transform(Vector3 position, Vector3 scale)
        {
            Position = position;
            Scale = scale;
            Rotation = Vector3.Zero;
        }

        public Transform(Vector3 position)
        {
            Position = position;
            Scale = Vector3.One;
            Rotation = Vector3.Zero;
        }
}