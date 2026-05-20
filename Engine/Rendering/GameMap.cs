using Engine.Components;
using Engine.Core;
using Engine.Core.ECS;
using Engine.Rendering.Helpers;
using Engine.Rendering.Renderer;
using JoltPhysicsSharp;
using OpenTK.Windowing.Common;
using BoundingBox = Engine.Physics.BoundingBox;

namespace Engine.Rendering;

public class GameMap
{
    public List<GameObject> Objects { get; } = [];

    public PointLight?[] Lights { get; set; } = new PointLight?[LightRenderer.MAX_POINT_LIGHTS];

    private BoundingBox[] _boundingBoxes = Array.Empty<BoundingBox>();
    private MutableTuple<BodyID, JoltColor>[] _bodyIds = [];
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
        if (DevConsole.IsOpen) return;
        LatibuleEngine.Physics.OnUpdateFrame(args);
        
        _isIterating = true;
        foreach (var obj in Objects) obj.OnUpdateFrame(args);
        _isIterating = false;
        ApplyDeferredOperations();
    }

    public void OnRenderFrame(FrameEventArgs args, RenderLayer layer)
    {
        _isIterating = true;
        foreach (var obj in Objects) obj.OnRenderFrame(args, layer);
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

    public void AddObject(GameObject obj, bool load = false)
    {
        if (_isIterating) _pendingAdds.Enqueue(obj);
        else if (!Objects.Contains(obj))
        {
            RemoveObject(obj);
            Objects.Add(obj);
            if (load) obj.OnLoad();
        }

        if (obj.PhysicsBodyID is not null) LatibuleEngine.Physics.RegisterBodyId(obj.PhysicsBodyID.Value);
    }

    public void AddObjects(IEnumerable<GameObject> objs)
    {
        foreach (var obj in objs) AddObject(obj);
    }

    public void RemoveObject(GameObject obj)
    {
        if (_isIterating)
            _pendingRemoves.Enqueue(obj);
        else
            Objects.Remove(obj);

        if (obj.PhysicsBodyID is not null) LatibuleEngine.Physics.UnregisterBody(obj.PhysicsBodyID.Value);
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
            var box = Objects[i].Get<BoundingBoxComponent>()?.BoundingBox;
            if (box != null) _boundingBoxes[i] = box.Value;
        }

        return _boundingBoxes;
    }

    public MutableTuple<BodyID, JoltColor>[] GetBodyIDs()
    {
        if (_bodyIds.Length != Objects.Count)
            _bodyIds = new MutableTuple<BodyID, JoltColor>[Objects.Count];

        for (int i = 0; i < Objects.Count; i++)
        {
            _bodyIds[i] = new MutableTuple<BodyID, JoltColor>(default, default);
            var box = Objects[i].PhysicsBodyID;
            if (box != null) _bodyIds[i].Item1 = box.Value;
            _bodyIds[i].Item2 = Objects[i].DebugColor;
        }

        return _bodyIds;
    }

    public void AddPointLight(PointLight light)
    {
        var currentAmount = Lights.Count(l => l is not null);
        if (currentAmount < LightRenderer.MAX_POINT_LIGHTS) Lights[currentAmount] = light;
        else Logger.LogWarning($"ADDING MORE THAN MAX_POINT_LIGHTS ({LightRenderer.MAX_POINT_LIGHTS}). UNABLE TO RENDER MORE POINT LIGHTS!!!");
    }
}