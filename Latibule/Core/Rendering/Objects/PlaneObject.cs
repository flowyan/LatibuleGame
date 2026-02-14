using Latibule.Core.Components;
using Latibule.Core.ECS;
using Latibule.Core.Rendering.Mesh;

namespace Latibule.Core.Rendering.Models;

/// <summary>
/// A flat plane for ground/floor rendering and collision.
/// </summary>
public class PlaneObject() : GameObject
{
    public override void OnLoad()
    {
        base.OnLoad();
        WithComponents([
            new PlaneRendererComponent(),
            new CollisionComponent()
        ]);
    }
}