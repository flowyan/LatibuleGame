﻿namespace Latibule.Utilities;

public static class VectorConverter
{
    public static OpenTK.Mathematics.Vector4 ToOpenTK(this System.Numerics.Vector4 vector)
    {
        return new OpenTK.Mathematics.Vector4(vector.X, vector.Y, vector.Z, vector.W);
    }

    public static System.Numerics.Vector4 ToNumerics(this OpenTK.Mathematics.Vector4 vector)
    {
        return new System.Numerics.Vector4(vector.X, vector.Y, vector.Z, vector.W);
    }

    public static OpenTK.Mathematics.Vector3 ToOpenTK(this System.Numerics.Vector3 vector)
    {
        return new OpenTK.Mathematics.Vector3(vector.X, vector.Y, vector.Z);
    }

    public static System.Numerics.Vector3 ToNumerics(this OpenTK.Mathematics.Vector3 vector)
    {
        return new System.Numerics.Vector3(vector.X, vector.Y, vector.Z);
    }

    public static OpenTK.Mathematics.Vector2 ToOpenTK(this System.Numerics.Vector2 vector)
    {
        return new OpenTK.Mathematics.Vector2(vector.X, vector.Y);
    }

    public static System.Numerics.Vector2 ToNumerics(this OpenTK.Mathematics.Vector2 vector)
    {
        return new System.Numerics.Vector2(vector.X, vector.Y);
    }
}