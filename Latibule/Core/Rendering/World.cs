using Latibule.Models;
using Microsoft.Xna.Framework;

namespace Latibule.Core.Rendering;

public class World
{
    public List<GameObject> Objects { get; init; } = [];

    public void Initialize()
    {
        foreach (var obj in Objects) obj.Initialize();
    }

    public void Update(GameTime gameTime)
    {
        foreach (var obj in Objects) obj.Update(gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        foreach (var obj in Objects) obj.Draw(gameTime);
    }

    public void AddObject(GameObject obj)
    {
        if (!Objects.Contains(obj)) Objects.Add(obj);
    }

    public void RemoveObject(GameObject obj)
    {
        Objects.Remove(obj);
    }

    public BoundingBox[] GetBoundingBoxes()
    {
        return Objects.Select(obj => obj.BoundingBox).ToArray();
    }
}