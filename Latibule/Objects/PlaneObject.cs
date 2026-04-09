using Engine.Components;
using Engine.Core.ECS;
using Engine.Data;
using Engine.Data.Shaders;
using Engine.Rendering.Shapes;
using OpenTK.Mathematics;

namespace Latibule.Objects;

/// <summary>
/// A flat plane for ground/floor rendering and collision.
/// </summary>
public class PlaneObject : GameObject
{
    public PlaneObject()
    {
        // Default plane transform
        Transform = new Transform(Vector3.Zero, new Vector3(1, 0, 1), Vector3.Zero);
    }

    public override void OnLoad()
    {
        base.OnLoad();

        WithComponents([
            new ShaderComponent(Asseteer.GetShader(EngineShaders.DefaultShader)),
            new ShapeRendererComponent(new PlaneShape()),
            new BoundingBoxComponent()
        ]);
    }
}