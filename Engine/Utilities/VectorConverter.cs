using OpenTK.Mathematics;

namespace Engine.Utilities;

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

    public static OpenTK.Mathematics.Vector3 ToOpenTKEulerDegrees(this System.Numerics.Quaternion quaternion)
    {
        // Normalize first so conversion remains stable when physics accumulates small errors.
        quaternion = System.Numerics.Quaternion.Normalize(quaternion);

        float sinrCosp = 2f * (quaternion.W * quaternion.X + quaternion.Y * quaternion.Z);
        float cosrCosp = 1f - 2f * (quaternion.X * quaternion.X + quaternion.Y * quaternion.Y);
        float x = MathF.Atan2(sinrCosp, cosrCosp);

        float sinp = 2f * (quaternion.W * quaternion.Y - quaternion.Z * quaternion.X);
        float y = MathF.Abs(sinp) >= 1f ? MathF.CopySign(MathF.PI / 2f, sinp) : MathF.Asin(sinp);

        float sinyCosp = 2f * (quaternion.W * quaternion.Z + quaternion.X * quaternion.Y);
        float cosyCosp = 1f - 2f * (quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z);
        float z = MathF.Atan2(sinyCosp, cosyCosp);

        const float RadToDeg = 180f / MathF.PI;
        return new OpenTK.Mathematics.Vector3(x * RadToDeg, y * RadToDeg, z * RadToDeg);
    }

    public static System.Numerics.Quaternion ToQuaternion(this Vector3 vector)
    {
        return new System.Numerics.Quaternion(
            MathF.Sin(MathHelper.DegreesToRadians(vector.X) / 2f) * MathF.Cos(MathHelper.DegreesToRadians(vector.Y) / 2f) * MathF.Cos(MathHelper.DegreesToRadians(vector.Z) / 2f) -
            MathF.Cos(MathHelper.DegreesToRadians(vector.X) / 2f) * MathF.Sin(MathHelper.DegreesToRadians(vector.Y) / 2f) * MathF.Sin(MathHelper.DegreesToRadians(vector.Z) / 2f),

            MathF.Cos(MathHelper.DegreesToRadians(vector.X) / 2f) * MathF.Sin(MathHelper.DegreesToRadians(vector.Y) / 2f) * MathF.Cos(MathHelper.DegreesToRadians(vector.Z) / 2f) +
            MathF.Sin(MathHelper.DegreesToRadians(vector.X) / 2f) * MathF.Cos(MathHelper.DegreesToRadians(vector.Y) / 2f) * MathF.Sin(MathHelper.DegreesToRadians(vector.Z) / 2f),

            MathF.Cos(MathHelper.DegreesToRadians(vector.X) / 2f) * MathF.Cos(MathHelper.DegreesToRadians(vector.Y) / 2f) * MathF.Sin(MathHelper.DegreesToRadians(vector.Z) / 2f) -
            MathF.Sin(MathHelper.DegreesToRadians(vector.X) / 2f) * MathF.Sin(MathHelper.DegreesToRadians(vector.Y) / 2f) * MathF.Cos(MathHelper.DegreesToRadians(vector.Z) / 2f),

            MathF.Cos(MathHelper.DegreesToRadians(vector.X) / 2f) * MathF.Cos(MathHelper.DegreesToRadians(vector.Y) / 2f) * MathF.Cos(MathHelper.DegreesToRadians(vector.Z) / 2f) +
            MathF.Sin(MathHelper.DegreesToRadians(vector.X) / 2f) * MathF.Sin(MathHelper.DegreesToRadians(vector.Y) / 2f) * MathF.Sin(MathHelper.DegreesToRadians(vector.Z) / 2f)
        );
    }
}