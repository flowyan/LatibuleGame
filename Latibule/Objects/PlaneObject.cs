using Latibule.Core.Components;
using Latibule.Core.ECS;
using Latibule.Core.Rendering.Shapes;
using OpenTK.Mathematics;

namespace Latibule.Core.Rendering.Objects;

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

        var shader = new Shader(
        $"{Metadata.ASSETS_ROOT_DIRECTORY}/{Metadata.ASSETS_SHADER_PATH}/mesh/shader.vert",
            $"{Metadata.ASSETS_ROOT_DIRECTORY}/{Metadata.ASSETS_SHADER_PATH}/mesh/shader.frag"
        );

        WithComponents([
            new ShaderComponent(shader),
            new ShapeRendererComponent(new PlaneShape()),
            new CollisionComponent()
        ]);
    }
}