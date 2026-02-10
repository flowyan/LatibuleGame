using Latibule.Models;
using Microsoft.Xna.Framework;

namespace Latibule.Core.Rendering;

public class World
{
    public List<GameObject> Objects { get; init; } = [];
    public List<BoundingBox> BoundingBoxes { get; init; } = [];

    public void Initialize()
    {
        foreach (var obj in Objects) obj.Initialize();
    }

    public void Update(GameTime gameTime)
    {
    }

    public void Draw(GameTime gameTime)
    {
        foreach (var obj in Objects) obj.Draw(gameTime);
    }

    public void AddObject(GameObject obj)
    {
        Objects.Add(obj);
    }

    public void RemoveObject(GameObject obj)
    {
        Objects.Remove(obj);
    }
}