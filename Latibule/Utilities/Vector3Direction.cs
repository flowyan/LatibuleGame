
using OpenTK.Mathematics;

namespace Latibule.Utilities;

public static class Vector3Direction
{
    public static readonly Vector3 Forward  = new(0, 0, -1);
    public static readonly Vector3 Backward = new(0, 0, 1);
    public static readonly Vector3 Up       = new(0, 1, 0);
    public static readonly Vector3 Down     = new(0, -1, 0);
    public static readonly Vector3 Left     = new(-1, 0, 0);
    public static readonly Vector3 Right    = new(1, 0, 0);
}