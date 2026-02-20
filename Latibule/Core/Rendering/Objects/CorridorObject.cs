using Latibule.Core.Components;
using Latibule.Core.Data;
using Latibule.Core.ECS;
using Latibule.Core.Rendering.Models;
using OpenTK.Mathematics;

namespace Latibule.Core.Rendering.Objects;

public class CorridorObject(Shader shader) : GameObject()
{
    public override void OnLoad()
    {
        var uvScale = new Vector2(1, 1);

        var component = new TextureComponent(Asseteer.GetTexture(TextureAsset.material_stone), uvScale);

        AddChildren([
            PlaneMaker(CorridorFace.Floor).WithComponent(component),
            PlaneMaker(CorridorFace.Ceiling).WithComponent(component),
            PlaneMaker(CorridorFace.LeftWall).WithComponent(component),
            PlaneMaker(CorridorFace.RightWall).WithComponent(component)
        ]);

        base.OnLoad();
    }

    private enum CorridorFace
    {
        Floor,
        Ceiling,
        LeftWall,
        RightWall,
    }

    private PlaneObject PlaneMaker(CorridorFace face)
    {
        var localScale = new Vector3(2, 0, 2);

        return face switch
        {
            CorridorFace.Floor => new PlaneObject() { Transform = { Position = new Vector3(0, 0, 0) + Transform.Position, Rotation = new Vector3(0, 0, 0), Scale = localScale } },
            CorridorFace.Ceiling => new PlaneObject() { Transform = { Position = new Vector3(0, 4, 0) + Transform.Position, Rotation = new Vector3(180, 0, 0), Scale = localScale } },
            CorridorFace.LeftWall => new PlaneObject() { Transform = { Position = new Vector3(0, 2, -2) + Transform.Position, Rotation = new Vector3(90, 0, 0), Scale = localScale } },
            CorridorFace.RightWall => new PlaneObject() { Transform = { Position = new Vector3(0, 2, 2) + Transform.Position, Rotation = new Vector3(270, 0, 0), Scale = localScale } },
            _ => throw new ArgumentOutOfRangeException(nameof(face), face, null)
        };
    }
}