using OpenTK.Mathematics;

namespace Latibule.Core.Rendering;

public sealed class PointLight
{
    public Vector3 Position = new Vector3(0, 0, 0);

    public Vector3 Color = Vector3.One; // 0..1
    public float Intensity = 1f;

    // Attenuation: 1 / (c + l*d + q*d^2)
    public float Constant = 1f;
    public float Linear = 0.09f;
    public float Quadratic = 0.032f;
}