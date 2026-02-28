using FontStashSharp;
using Latibule.Core;
using Latibule.Core.Components;
using Latibule.Core.Components.Dev;
using Latibule.Core.Data;
using Latibule.Core.ECS;
using Latibule.Core.Rendering;
using Latibule.Core.Rendering.Renderer;
using Latibule.Core.Rendering.Shapes;
using Latibule.Core.Types;
using Latibule.Entities;
using Latibule.Objects;
using OpenTK.Mathematics;

namespace Latibule;

public static class TestWorld
{
    public static World Create()
    {
        var meshShader = Asseteer.GetShader(ShaderAsset.mesh_shader);
        var world = new World();
        LatibuleGame.Player = new Player { Transform = { Position = new Vector3(0, 0.1f, 0) } };
        LatibuleGame.Player.WithComponents([
            new DebugInfoOverlay(),
            new DebugBoundingBoxOutlineOverlay(),
            new DebugPointLightRendererOverlay()
        ]);
        world.AddObject(LatibuleGame.Player);

        world.AddObject(new GameObject
        {
            Transform =
            {
                Position = new Vector3(-7.5f, 4, 0),
                Scale = new Vector3(2, 2, 2),
                Rotation = new Vector3(0, 270, 0)
            }
        }.WithComponents(new ShaderComponent(meshShader), new ShapeRendererComponent(new IsoSphere(8, 16)), new BoundingBoxComponent(), new TextureComponent(Asseteer.GetTexture(TextureAsset.misc_tequila))));

        world.AddObject(new GameObject
        {
            Transform = { Position = new Vector3(0, 1, -7.5f), Scale = new Vector3(0.5f) }
        }.WithComponents(new ShaderComponent(meshShader), new ShapeRendererComponent(new Cube()), new BoundingBoxComponent(), new TextureComponent(Asseteer.GetTexture(TextureAsset.misc_speaker))));

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
        world.AddObject(new Corridor(meshShader) { Transform = { Position = new Vector3(12, 0, 0) } });
        world.AddObject(new Corridor(meshShader) { Transform = { Position = new Vector3(16, 0, 0) } });
        world.AddObject(new Corridor(meshShader) { Transform = { Position = new Vector3(20, 0, 0) } });
        world.AddObject(new Corridor(meshShader) { Transform = { Position = new Vector3(24, 0, 0) } });
        world.AddObject(new Corridor(meshShader) { Transform = { Position = new Vector3(28, 0, 0) } });

        // Text ui object
        world.AddObject(new GameObject { Transform = { Position = new(0, 2, 8.5f) } }
            .WithComponents(new WorldTextRendererComponent(new TextRendererOptions
            {
                text = "AWESOME FUCKING MAXWELLS",
                fontSize = 32,
                color = FSColor.Purple,
                fontSystemEffect = FontSystemEffect.Stroked,
                effectAmount = 4,
                billboard = BillboardEnum.YLocked
            })));

        // Lights
        world.AddPointLight(new PointLight() { Position = new Vector3(0, 2, 0), Color = new Vector3(1f, 0.8f, 0.6f) });
        world.AddPointLight(new PointLight() { Position = new Vector3(20, 2, 0), Color = new Vector3(1f, 0.8f, 0.6f) });
        world.AddPointLight(new PointLight() { Position = new Vector3(0, 2f, 7.5f), Color = new Vector3(1f, 0.8f, 0.6f) });
        world.AddPointLight(new PointLight() { Position = new Vector3(50f, 0, 0), Color = new Vector3(1f, 0.8f, 0.6f) });

        world.AddObject(new Maxwell { Transform = { Position = new Vector3(-2, 0.5f, 8.5f) } });
        world.AddObject(new Maxwell(true) { Transform = { Position = new Vector3(0, 0.5f, 8.5f) } });
        world.AddObject(new Maxwell { Transform = { Position = new Vector3(2, 0.5f, 8.5f) } });

        world.AddObject(new GameObject { Transform = { Position = new Vector3(50, 0, 0), Scale = new Vector3(0.01f) } }.WithComponents(
            new ShaderComponent(meshShader),
            new TextureComponent(Asseteer.GetTexture(TextureAsset.material_concrete)),
            new ModelRendererComponent(Asseteer.GetModel(ModelAsset.sponza))
        ));

        return world;
    }
}