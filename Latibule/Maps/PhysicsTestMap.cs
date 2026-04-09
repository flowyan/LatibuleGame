using Engine.Components;
using Engine.Core.ECS;
using Engine.Core.Types;
using Engine.Data;
using Engine.Data.Sound;
using Engine.Data.Textures;
using Engine.Physics;
using Engine.Rendering;
using FontStashSharp;
using Latibule.Components;
using Latibule.Data.Sound;
using Latibule.Entities;
using Latibule.Objects;
using OpenTK.Mathematics;
using Textures = Latibule.Data.Texture.Textures;

namespace Latibule.Maps;

public static class PhysicsTestMap
{
    public static GameMap Create()
    {
        var map = new GameMap();

        // floor
        // map.AddObject(new PlaneObject { Transform = { Position = new Vector3(0, 0, 0), Scale = new Vector3(10, 0, 10) } }
        //     .WithComponent(new TextureComponent(Asseteer.GetTexture(EngineTextures.Dev.dev_measuregeneric01b), new Vector2(10, 10))));

        // // Walls
        // map.AddObject(new PlaneObject { Transform = { Position = new Vector3(10, 2, 0), Scale = new Vector3(2, 0, 10), Rotation = new Vector3(0, 0, 90) } }
        //     .WithComponent(new TextureComponent(Asseteer.GetTexture(EngineTextures.Dev.dev_measuregeneric01), new Vector2(2, 10), 90f)));
        // map.AddObject(new PlaneObject { Transform = { Position = new Vector3(-10, 2, 0), Scale = new Vector3(2, 0, 10), Rotation = new Vector3(0, 0, 270) } }
        //     .WithComponent(new TextureComponent(Asseteer.GetTexture(EngineTextures.Dev.dev_measuregeneric01), new Vector2(2, 10), -90f)));
        // map.AddObject(new PlaneObject { Transform = { Position = new Vector3(0, 2, 10), Scale = new Vector3(2, 0, 10), Rotation = new Vector3(-90, 0, 90) } }
        //     .WithComponent(new TextureComponent(Asseteer.GetTexture(EngineTextures.Dev.dev_measuregeneric01), new Vector2(2, 10), 90f)));
        // map.AddObject(new PlaneObject { Transform = { Position = new Vector3(0, 2, -10), Scale = new Vector3(2, 0, 10), Rotation = new Vector3(90, 0, -90) } }
        //     .WithComponent(new TextureComponent(Asseteer.GetTexture(EngineTextures.Dev.dev_measuregeneric01), new Vector2(2, 10), -90f)));

        // Text ui object
        map.AddObject(new GameObject { Transform = { Position = new(0, 3, 5) } }
            .WithComponents(new WorldTextRendererComponent(new TextRendererOptions
            {
                text = "physics test map",
                fontSize = 32,
                color = FSColor.White,
                fontSystemEffect = FontSystemEffect.Stroked,
                effectAmount = 4,
                billboard = BillboardEnum.YLocked
            })));

        // Lights
        map.AddPointLight(new PointLight { Position = new Vector3(0, 2, 0), Color = new Vector3(1f, 1f, 1f) });

        // map.AddObject(new Maxwell(false) { Transform = { Position = new Vector3(0, 0.5f, 8.5f) } });

        Asseteer.PlaySound(EngineSounds.Dev.tada, 0.5f);

        LatibuleGame.Player = new Player { Transform = { Position = new Vector3(0, 0.1f, 0) } };
        LatibuleGame.Player.IsNoclip = true;
        LatibuleGame.Player.WithComponents(new DebugInfoOverlay(), new DebugBoundingBoxOutlineOverlay(), new DebugPointLightRendererOverlay());
        map.AddObject(LatibuleGame.Player); // Player should always be added last
        return map;
    }
}