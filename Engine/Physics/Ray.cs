using OpenTK.Mathematics;

namespace Engine.Physics;

public readonly struct Ray
{
    public Vector3 Position { get; }

    /// <summary>
    /// The direction of the ray. This should be a normalized vector.
    /// </summary>
    public Vector3 Direction { get; }

    public Ray(Vector3 position, Vector3 direction)
    {
        Position = position;
        Direction = direction.Normalized();
    }
}