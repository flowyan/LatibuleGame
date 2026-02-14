using Latibule.Core.Components;
using Latibule.Core.ECS;
using Latibule.Core.Physics;
using Latibule.Entities;
using OpenTK.Windowing.Common;

namespace Latibule.Core.Rendering;

public class World
{
    public List<GameObject> Objects { get; init; } = [];

    private BoundingBox[] _boundingBoxes = Array.Empty<BoundingBox>();
    private readonly Queue<GameObject> _pendingAdds = [];
    private readonly Queue<GameObject> _pendingRemoves = [];
    private bool _isIterating = false;

    public void OnLoad()
    {
        _isIterating = true;
        foreach (var obj in Objects) obj.OnLoad();
        _isIterating = false;
        ApplyDeferredOperations();
    }

    public void OnUpdateFrame(FrameEventArgs args)
    {
        if (GameStates.CurrentGui is DevConsole) return;
        _isIterating = true;
        foreach (var obj in Objects) obj.OnUpdateFrame(args);
        _isIterating = false;
        ApplyDeferredOperations();
    }

    public void OnRenderFrame(FrameEventArgs args)
    {
        _isIterating = true;
        foreach (var obj in Objects) obj.OnRenderFrame(args);
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
        {
            var box = Objects[i].Get<CollisionComponent>()?.BoundingBox;
            if (box != null) _boundingBoxes[i] = box.Value;
        }

        return _boundingBoxes;
    }

    /// <summary>
    /// Returns bounding boxes only for objects that have collision enabled.
    /// Used for player AABB collision detection.
    /// </summary>
    public BoundingBox[] GetCollidableBoxes()
    {
        var collidables = Objects.Where(o => (bool)o.Get<CollisionComponent>()?.HasCollision).ToList();
        var result = new BoundingBox[collidables.Count];
        for (int i = 0; i < collidables.Count; i++)
        {
            var box = Objects[i].Get<CollisionComponent>()?.BoundingBox;
            if (box != null) result[i] = box.Value;
        }

        return result;
    }
}