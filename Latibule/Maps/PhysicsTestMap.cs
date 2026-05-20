using Engine.Components;
using Engine.Core.Types;
using Engine.Data;
using Engine.Data.Textures;
using Engine.Rendering;
using Latibule.Components;
using Latibule.Entities;
using Latibule.Objects;
using OpenTK.Mathematics;

namespace Latibule.Maps;

public static class PhysicsTestMap
{
    public static GameMap Create()
    {
        var map = new GameMap();

        // floor
        map.AddObject(new PlaneObject { Transform = { Position = new Vector3(0, 0, 0), Scale = new Vector3(10, 0, 10) } }
            .WithComponent(new TextureComponent(Asseteer.GetTexture(EngineTextures.Dev.dev_measuregeneric01b), new Vector2(10, 10))));

        // // Walls
        map.AddObject(new PlaneObject { Transform = { Position = new Vector3(10, 2, 0), Scale = new Vector3(2, 0, 10), Rotation = new Vector3(0, 0, 90) } }
            .WithComponent(new TextureComponent(Asseteer.GetTexture(EngineTextures.Dev.dev_measuregeneric01), new Vector2(2, 10), 90f)));
        map.AddObject(new PlaneObject { Transform = { Position = new Vector3(-10, 2, 0), Scale = new Vector3(2, 0, 10), Rotation = new Vector3(0, 0, 270) } }
            .WithComponent(new TextureComponent(Asseteer.GetTexture(EngineTextures.Dev.dev_measuregeneric01), new Vector2(2, 10), -90f)));
        map.AddObject(new PlaneObject { Transform = { Position = new Vector3(0, 2, 10), Scale = new Vector3(2, 0, 10), Rotation = new Vector3(-90, 0, 90) } }
            .WithComponent(new TextureComponent(Asseteer.GetTexture(EngineTextures.Dev.dev_measuregeneric01), new Vector2(2, 10), 90f)));
        map.AddObject(new PlaneObject { Transform = { Position = new Vector3(0, 2, -10), Scale = new Vector3(2, 0, 10), Rotation = new Vector3(90, 0, -90) } }
            .WithComponent(new TextureComponent(Asseteer.GetTexture(EngineTextures.Dev.dev_measuregeneric01), new Vector2(2, 10), -90f)));

        // Text ui object
        map.AddObject(new BillboardText("physics test map", BillboardEnum.Full) { Transform = { Position = new(0, 3, 5) } });

        // Y coordinate helper
        const float textX = 9.9f;
        map.AddObjects([
            new BillboardText("Y 0.0", fontSize: 16) { Transform = { Position = new Vector3(textX, 0, 0) } },
            new BillboardText("Y 0.1", fontSize: 16) { Transform = { Position = new Vector3(textX, 0.1f, 0) } },
            new BillboardText("Y 0.2", fontSize: 16) { Transform = { Position = new Vector3(textX, 0.2f, 0) } },
            new BillboardText("Y 0.3", fontSize: 16) { Transform = { Position = new Vector3(textX, 0.3f, 0) } },
            new BillboardText("Y 0.4", fontSize: 16) { Transform = { Position = new Vector3(textX, 0.4f, 0) } },
            new BillboardText("Y 0.5", fontSize: 16) { Transform = { Position = new Vector3(textX, 0.5f, 0) } },
            new BillboardText("Y 0.6", fontSize: 16) { Transform = { Position = new Vector3(textX, 0.6f, 0) } },
            new BillboardText("Y 0.7", fontSize: 16) { Transform = { Position = new Vector3(textX, 0.7f, 0) } },
            new BillboardText("Y 0.8", fontSize: 16) { Transform = { Position = new Vector3(textX, 0.8f, 0) } },
            new BillboardText("Y 0.9", fontSize: 16) { Transform = { Position = new Vector3(textX, 0.9f, 0) } },
            new BillboardText("Y 1.0", fontSize: 16) { Transform = { Position = new Vector3(textX, 1.0f, 0) } },
            new BillboardText("Y 1.1", fontSize: 16) { Transform = { Position = new Vector3(textX, 1.1f, 0) } },
            new BillboardText("Y 1.2", fontSize: 16) { Transform = { Position = new Vector3(textX, 1.2f, 0) } },
            new BillboardText("Y 1.3", fontSize: 16) { Transform = { Position = new Vector3(textX, 1.3f, 0) } },
            new BillboardText("Y 1.4", fontSize: 16) { Transform = { Position = new Vector3(textX, 1.4f, 0) } },
            new BillboardText("Y 1.5", fontSize: 16) { Transform = { Position = new Vector3(textX, 1.5f, 0) } },
            new BillboardText("Y 1.6", fontSize: 16) { Transform = { Position = new Vector3(textX, 1.6f, 0) } },
            new BillboardText("Y 1.7", fontSize: 16) { Transform = { Position = new Vector3(textX, 1.7f, 0) } },
            new BillboardText("Y 1.8", fontSize: 16) { Transform = { Position = new Vector3(textX, 1.8f, 0) } },
            new BillboardText("Y 1.9", fontSize: 16) { Transform = { Position = new Vector3(textX, 1.9f, 0) } },
            new BillboardText("Y 2.0", fontSize: 16) { Transform = { Position = new Vector3(textX, 2.0f, 0) } }
        ]);

        // Lights
        map.AddPointLight(new PointLight { Position = new Vector3(0, 2, 0), Color = new Vector3(1f, 1f, 1f) });

        map.AddObject(new Maxwell(true) { Transform = { Position = new Vector3(0, 0.5f, 8.5f) } });

        // Asseteer.PlaySound(EngineSounds.Dev.tada, 0.5f);

        LatibuleGame.Player = new Player { Transform = { Position = new Vector3(0, 0.1f, 0) } };
        // LatibuleGame.Player.IsNoclip = true;
        LatibuleGame.Player.WithComponents(new DebugInfoOverlay(), new DebugBoundingBoxOutlineOverlay(), new DebugPointLightRendererOverlay());
        map.AddObject(LatibuleGame.Player); // Player should always be added last
        return map;
    }
}