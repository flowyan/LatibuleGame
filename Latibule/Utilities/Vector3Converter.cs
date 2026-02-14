namespace Latibule.Utilities;

public static class Vector3Converter
{
    public static OpenTK.Mathematics.Vector3 ToOpenTK(this System.Numerics.Vector3 vector)
    {
        return new OpenTK.Mathematics.Vector3(vector.X, vector.Y, vector.Z);
    }

    public static System.Numerics.Vector3 ToNumerics(this OpenTK.Mathematics.Vector3 vector)
    {
        return new System.Numerics.Vector3(vector.X, vector.Y, vector.Z);
    }
}