using Engine.Components;
using Engine.Core.ECS;
using Engine.Core.Types;
using Engine.Data;
using Latibule.Data.Texture;
using OpenTK.Mathematics;

namespace Latibule.Objects;

public class Corridor : GameObject, IPrefab
{
    public override void OnLoad()
    {
        var uvScale = new Vector2(1, 1);

        var component = new TextureComponent(Asseteer.GetTexture(Textures.Material.stone), uvScale);

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