using Latibule.Core.Components;
using Latibule.Core.ECS;
using Latibule.Core.Rendering.Models;
using OpenTK.Mathematics;

namespace Latibule.Core.Rendering.Objects;

public class CorridorModel(Shader shader) : GameObject()
{
    public override void OnLoad()
    {
        var localScale = new Vector3(2, 0, 2);
        var uvScale = new Vector2(1, 1);

        BaseComponent[] components =
        [
            new ShaderComponent(shader)
            {
                Texture = new Texture($"{Metadata.ASSETS_ROOT_DIRECTORY}/{Metadata.ASSETS_TEXTURE_PATH}/material/stone.jpg")
            }
        ];

        AddChildren([
            new PlaneModel() { Position = new Vector3(0, 0, 0) + Position, Rotation = new Vector3(0, 0, 0), Scale = localScale, UVScale = uvScale, Components = components },
            new PlaneModel() { Position = new Vector3(0, 2, -2) + Position, Rotation = new Vector3(90, 0, 0), Scale = localScale, UVScale = uvScale, Components = components },
            new PlaneModel() { Position = new Vector3(0, 2, 2) + Position, Rotation = new Vector3(270, 0, 0), Scale = localScale, UVScale = uvScale, Components = components },
            new PlaneModel() { Position = new Vector3(0, 4, 0) + Position, Rotation = new Vector3(180, 0, 0), Scale = localScale, UVScale = uvScale, Components = components },
        ]);

        base.OnLoad();
    }
}