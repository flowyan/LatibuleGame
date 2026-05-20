using Engine.Core;
using Engine.Physics;
using Engine.Rendering;

namespace Engine;

public class LatibuleEngine
{
    public static GameMap Map { get; set; } = new();
    public static Camera Camera { get; set; }
    public static JoltPhysics Physics { get; private set; } = new();
}