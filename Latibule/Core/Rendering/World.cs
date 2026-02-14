using Latibule.Core.ECS;
using Microsoft.Xna.Framework;

namespace Latibule.Core.Rendering;

public class World
{
    public List<GameObject> Objects { get; set; } = [];
    private BoundingBox[] _boundingBoxes = Array.Empty<BoundingBox>();
    private readonly Queue<GameObject> _pendingAdds = [];
    private readonly Queue<GameObject> _pendingRemoves = [];
    private bool _isIterating = false;

    public void Initialize()
    {
        _isIterating = true;
        foreach (var obj in Objects) obj.Initialize();
        _isIterating = false;
        ApplyDeferredOperations();
    }

    public void Update(GameTime gameTime)
    {
        if (GameStates.CurrentGui is DevConsole) return;
        _isIterating = true;
        foreach (var obj in Objects) obj.Update(gameTime);
        _isIterating = false;
        ApplyDeferredOperations();
    }

    public void Draw(GameTime gameTime)
    {
        _isIterating = true;
        foreach (var obj in Objects) obj.Draw(gameTime);
        _isIterating = false;
        ApplyDeferredOperations();
    }

    public void Dispose()
    {
        _isIterating = true;
        foreach (var obj in Objects) obj.Dispose();
        _isIterating = false;
        ApplyDeferredOperations();
    }

    public void AddObject(GameObject obj)
    {
        if (_isIterating) _pendingAdds.Enqueue(obj);
        else if (!Objects.Contains(obj))
        {
            RemoveObject(obj);
            Objects.Add(obj);
        }
    }

    public void RemoveObject(GameObject obj)
    {
        if (_isIterating)
            _pendingRemoves.Enqueue(obj);
        else
            Objects.Remove(obj);
    }

    public void RemoveAllObjects()
    {
        if (_isIterating)
            _pendingRemoves.Clear();
        else
            Objects.Clear();
    }

    private void ApplyDeferredOperations()
    {
        while (_pendingRemoves.Count > 0)
            Objects.Remove(_pendingRemoves.Dequeue());
        while (_pendingAdds.Count > 0)
        {
            var obj = _pendingAdds.Dequeue();
            if (!Objects.Contains(obj))
                Objects.Add(obj);
        }
    }

    public BoundingBox[] GetBoundingBoxes()
    {
        if (_boundingBoxes.Length != Objects.Count)
            _boundingBoxes = new BoundingBox[Objects.Count];

        for (int i = 0; i < Objects.Count; i++)
            _boundingBoxes[i] = Objects[i].BoundingBox;

        return _boundingBoxes;
    }
}