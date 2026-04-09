using Engine.Components;
using Engine.Core.ECS;
using Engine.Core.Types;
using Engine.Data;
using Engine.Data.Shaders;
using Engine.Rendering;
using Engine.Rendering.Shapes;
using FontStashSharp;
using Latibule.Components;
using Latibule.Data.Texture;
using Latibule.Entities;
using Latibule.Items;
using Latibule.Objects;
using OpenTK.Mathematics;

namespace Latibule.Maps;

public static class TestingMap
{
    public static GameMap Create()
    {
        var meshShader = Asseteer.GetShader(EngineShaders.DefaultShader);
        var world = new GameMap();

        world.AddObject(new GameObject
        {
            Transform =
            {
                Position = new Vector3(-7.5f, 4, 0),
                Scale = new Vector3(2, 2, 2),
                Rotation = new Vector3(0, 270, 0)
            }
        }.WithComponents(new ShaderComponent(meshShader), new ShapeRendererComponent(new IsoSphere(8)), new BoundingBoxComponent(), new TextureComponent(Asseteer.GetTexture(Textures.Misc.tequila))));

        world.AddObject(new GameObject
        {
            Transform = { Position = new Vector3(0, 1, -7.5f), Scale = new Vector3(0.5f) }
        }.WithComponents(new ShaderComponent(meshShader), new ShapeRendererComponent(new Cube()), new BoundingBoxComponent(), new TextureComponent(Asseteer.GetTexture(Textures.Misc.speaker))));

        world.AddPointLight(new PointLight { Position = new Vector3(5, 1.5f, 0), Color = new Vector3(0f, 1f, 0f), Intensity = 0.5f });
        world.AddObject(new GameObject
        {
            Transform = { Position = new Vector3(5, 0.5f, 0), Scale = new Vector3(0.5f) }
        }.WithComponents(new ShaderComponent(meshShader), new ShapeRendererComponent(new Cube()), new BoundingBoxComponent(), new TextureComponent(Asseteer.GetTexture(Textures.Misc.greensquare))));


        // floor
        world.AddObject(new PlaneObject
            {
                Transform = { Position = new Vector3(0, 0, 0), Scale = new Vector3(10, 0, 10) }
            }
            .WithComponent(new TextureComponent(Asseteer.GetTexture(Textures.Material.tiles), new Vector2(10, 10))));

        // Walls
        world.AddObject(new PlaneObject { Transform = { Position = new Vector3(10, 2, 6), Scale = new Vector3(2, 0, 4), Rotation = new Vector3(0, 0, 90) } }
            .WithComponent(new TextureComponent(Asseteer.GetTexture(Textures.Material.concrete), new Vector2(2, 4), 90f)));
        world.AddObject(new PlaneObject { Transform = { Position = new Vector3(10, 2, -6), Scale = new Vector3(2, 0, 4), Rotation = new Vector3(0, 0, 90) } }
            .WithComponent(new TextureComponent(Asseteer.GetTexture(Textures.Material.concrete), new Vector2(2, 4), 90f)));
        world.AddObject(new PlaneObject { Transform = { Position = new Vector3(-10, 2, 0), Scale = new Vector3(2, 0, 10), Rotation = new Vector3(0, 0, 270) } }
            .WithComponent(new TextureComponent(Asseteer.GetTexture(Textures.Material.concrete), new Vector2(2, 10), -90f)));
        world.AddObject(new PlaneObject { Transform = { Position = new Vector3(0, 2, 10), Scale = new Vector3(2, 0, 10), Rotation = new Vector3(-90, 0, 90) } }
            .WithComponent(new TextureComponent(Asseteer.GetTexture(Textures.Material.concrete), new Vector2(2, 10), 90f)));
        world.AddObject(new PlaneObject { Transform = { Position = new Vector3(0, 2, -10), Scale = new Vector3(2, 0, 10), Rotation = new Vector3(90, 0, -90) } }
            .WithComponent(new TextureComponent(Asseteer.GetTexture(Textures.Material.concrete), new Vector2(2, 10), -90f)));

        // Corridor
        world.AddObject(new Corridor { Transform = { Position = new Vector3(12, 0, 0) } });
        world.AddObject(new Corridor { Transform = { Position = new Vector3(16, 0, 0) } });
        world.AddObject(new Corridor { Transform = { Position = new Vector3(20, 0, 0) } });
        world.AddObject(new Corridor { Transform = { Position = new Vector3(24, 0, 0) } });
        world.AddObject(new Corridor { Transform = { Position = new Vector3(28, 0, 0) } });

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
        world.AddPointLight(new PointLight { Position = new Vector3(0, 2, 0), Color = new Vector3(1f, 0.8f, 0.6f) });
        world.AddPointLight(new PointLight { Position = new Vector3(20, 2, 0), Color = new Vector3(1f, 0.8f, 0.6f) });
        world.AddPointLight(new PointLight { Position = new Vector3(0, 2f, 7.5f), Color = new Vector3(1f, 0.8f, 0.6f) });
        world.AddPointLight(new PointLight { Position = new Vector3(50f, 0, 0), Color = new Vector3(1f, 0.8f, 0.6f) });

        world.AddObject(new Maxwell { Transform = { Position = new Vector3(-2, 0.5f, 8.5f) } });
        world.AddObject(new Maxwell(true) { Transform = { Position = new Vector3(0, 0.5f, 8.5f) } });
        world.AddObject(new Maxwell { Transform = { Position = new Vector3(2, 0.5f, 8.5f) } });

        // world.AddObject(new GameObject { Transform = { Position = new Vector3(50, 0, 0), Scale = new Vector3(0.01f) } }.WithComponents(
        //     new ShaderComponent(meshShader),
        //     new TextureComponent(Asseteer.GetTexture(Textures.Material.concrete)),
        //     new ModelRendererComponent(Asseteer.GetModel(ModelAsset.sponza))
        // ));

        LatibuleGame.Player = new Player { Transform = { Position = new Vector3(0, 0.1f, 0) } };
        // Starting items
        Player.Inventory.Items = [new ItemMaxwell()];
        LatibuleGame.Player.WithComponents(new DebugInfoOverlay(), new DebugBoundingBoxOutlineOverlay(), new DebugPointLightRendererOverlay());
        world.AddObject(LatibuleGame.Player); // Player should always be added last
        return world;
    }
}