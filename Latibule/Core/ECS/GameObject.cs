using Latibule.Core.Physics;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Latibule.Core.ECS;

public class GameObject() : IDisposable
{
    public Vector3 Position { get; set; } = Vector3.Zero;

    public Vector3 Rotation { get; set; } = Vector3.Zero;

    public Vector3 Scale { get; set; } = Vector3.One;
    public Vector2 UVScale { get; set; } = Vector2.One;
    public GameObject? Parent { get; protected set; } = null;
    public GameObject[] Children { get; private set; } = Array.Empty<GameObject>();

    /// <summary>
    /// Whether this object participates in AABB collision detection with the player.
    /// </summary>
    public bool HasCollision { get; set; } = true;

    public BoundingBox BoundingBox { get; protected set; }

    private readonly Dictionary<Type, BaseComponent> _byType = new();

    public BaseComponent[] Components
    {
        get;
        init
        {
            field = value;
            _byType.Clear();
            foreach (var c in field)
                _byType[c.GetType()] = c; // last one wins
        }
    } = [];

    public virtual void OnLoad()
    {
        foreach (var component in Components) component.OnLoad();
        foreach (var child in Children) child.OnLoad();

        LatibuleGame.GameWorld.AddObject(this);
    }

    public virtual void OnUpdateFrame(FrameEventArgs args)
    {
        foreach (var component in Components) component.OnUpdateFrame(args);
        foreach (var child in Children) child.OnUpdateFrame(args);
    }

    public virtual void OnRenderFrame(FrameEventArgs args)
    {
        foreach (var component in Components) component.OnRenderFrame(args);
        foreach (var child in Children) child.OnRenderFrame(args);
    }

    public virtual void Dispose()
    {
        foreach (var component in Components) component.Dispose();
    }

    public void AddChild(GameObject child)
    {
        child.Parent = this;
        Children = Children.Concat([child]).ToArray();
    }

    public void AddChildren(params GameObject[] children)
    {
        foreach (var child in children)
        {
            child.Parent = this;
        }

        Children = Children.Concat(children).ToArray();
    }

    public T? Get<T>() where T : BaseComponent
        => _byType.TryGetValue(typeof(T), out var c) ? (T)c : null;

    public T Require<T>() where T : BaseComponent
        => Get<T>() ?? throw new InvalidOperationException($"Missing required component: {typeof(T).Name}");
}