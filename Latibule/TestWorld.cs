using Latibule.Core;
using Latibule.Core.Components;
using Latibule.Core.Data;
using Latibule.Core.ECS;
using Latibule.Core.Rendering;
using Latibule.Core.Rendering.Objects;
using Latibule.Core.Rendering.Shapes;
using Latibule.Entities;
using OpenTK.Mathematics;

namespace Latibule;

public static class TestWorld
{
    public static World Create()
    {
        var shader = new Shader(
            $"{Metadata.ASSETS_ROOT_DIRECTORY}/{Metadata.ASSETS_SHADER_PATH}/mesh/shader.vert",
            $"{Metadata.ASSETS_ROOT_DIRECTORY}/{Metadata.ASSETS_SHADER_PATH}/mesh/shader.frag"
        );
        var world = new World();
        LatibuleGame.Player = new Player { Transform = { Position = new Vector3(0, 0.1f, 0) } };
        world.AddObject(LatibuleGame.Player);

        world.AddObject(new GameObject
        {
            Transform =
            {
                Position = new Vector3(-7.5f, 4, 0),
                Scale = new Vector3(2, 2, 2),
                Rotation = new Vector3(0, 270, 0)
            }
        }.WithComponents(new ShaderComponent(shader), new ShapeRendererComponent(new IsoSphere(8, 16)), new CollisionComponent(), new TextureComponent(Asseteer.GetTexture(TextureAsset.misc_tequila))));

        world.AddObject(new GameObject
        {
            Transform = { Position = new Vector3(0, 1, -7.5f), Scale = new Vector3(0.5f) }
        }.WithComponents(new ShaderComponent(shader), new ShapeRendererComponent(new Cube()), new CollisionComponent(), new TextureComponent(Asseteer.GetTexture(TextureAsset.misc_speaker))));

        // floor
        world.AddObject(new PlaneObject
            {
                Transform = { Position = new Vector3(0, 0, 0), Scale = new Vector3(10, 0, 10) }
            }
            .WithComponent(new TextureComponent(Asseteer.GetTexture(TextureAsset.material_tiles), new Vector2(10, 10))));

        // Walls
        world.AddObject(new PlaneObject { Transform = { Position = new Vector3(10, 2, 6), Scale = new Vector3(2, 0, 4), Rotation = new Vector3(0, 0, 90) } }
            .WithComponent(new TextureComponent(Asseteer.GetTexture(TextureAsset.material_concrete), new Vector2(2, 4), 90f)));
        world.AddObject(new PlaneObject { Transform = { Position = new Vector3(10, 2, -6), Scale = new Vector3(2, 0, 4), Rotation = new Vector3(0, 0, 90) } }
            .WithComponent(new TextureComponent(Asseteer.GetTexture(TextureAsset.material_concrete), new Vector2(2, 4), 90f)));
        world.AddObject(new PlaneObject { Transform = { Position = new Vector3(-10, 2, 0), Scale = new Vector3(2, 0, 10), Rotation = new Vector3(0, 0, 270) } }
            .WithComponent(new TextureComponent(Asseteer.GetTexture(TextureAsset.material_concrete), new Vector2(2, 10), -90f)));
        world.AddObject(new PlaneObject { Transform = { Position = new Vector3(0, 2, 10), Scale = new Vector3(2, 0, 10), Rotation = new Vector3(-90, 0, 90) } }
            .WithComponent(new TextureComponent(Asseteer.GetTexture(TextureAsset.material_concrete), new Vector2(2, 10), 90f)));
        world.AddObject(new PlaneObject { Transform = { Position = new Vector3(0, 2, -10), Scale = new Vector3(2, 0, 10), Rotation = new Vector3(90, 0, -90) } }
            .WithComponent(new TextureComponent(Asseteer.GetTexture(TextureAsset.material_concrete), new Vector2(2, 10), -90f)));

        // Corridor
        world.AddObject(new CorridorObject(shader) { Transform = { Position = new Vector3(12, 0, 0) } });
        world.AddObject(new CorridorObject(shader) { Transform = { Position = new Vector3(16, 0, 0) } });
        world.AddObject(new CorridorObject(shader) { Transform = { Position = new Vector3(20, 0, 0) } });
        world.AddObject(new CorridorObject(shader) { Transform = { Position = new Vector3(24, 0, 0) } });
        world.AddObject(new CorridorObject(shader) { Transform = { Position = new Vector3(28, 0, 0) } });

        // Text ui object
        world.AddObject(new GameObject { Transform = { Position = new(0, 2, 0) } }
            .WithComponents(new TextRendererComponent("W FAPS", 0.1f)));

        // Lights
        world.AddPointLight(new PointLight() { Position = new Vector3(0, 2, 0), Color = new Vector3(1f, 0.8f, 0.6f) });
        world.AddPointLight(new PointLight() { Position = new Vector3(20, 2, 0), Color = new Vector3(1f, 0.8f, 0.6f) });
        world.AddPointLight(new PointLight() { Position = new Vector3(0, 2f, 7.5f), Color = new Vector3(1f, 0.8f, 0.6f) });


        world.AddObject(new GameObject { Transform = { Position = new Vector3(0, 0.5f, 7.5f) } }.WithComponents(
            new ShaderComponent(shader),
            new ModelRendererComponent(Asseteer.GetModel(ModelAsset.maxwell)),
            new CollisionComponent(scale: new Vector3(0.7f, 0.4f, 0.5f)),
            new TextureComponent(Asseteer.GetTextures([TextureAsset.maxwell_maxwell, TextureAsset.maxwell_whiskers]))
        ));

        return world;
    }
}