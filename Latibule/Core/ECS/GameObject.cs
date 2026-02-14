using Microsoft.Xna.Framework;

namespace Latibule.Core.ECS;

public class GameObject(Game game)
{
    public Vector3 Position { get; set; } = Vector3.Zero;

    public Vector3 Rotation { get; set; } = Vector3.Zero;

    public Vector3 Scale { get; set; } = Vector3.One;
    public Vector2 UVScale { get; set; } = Vector2.One;

    public BoundingBox BoundingBox { get; protected set; }

    public GameObject? Parent { get; protected set; } = null;
    public GameObject[] Children { get; private set; } = Array.Empty<GameObject>();

    private readonly Dictionary<Type, BaseComponent> _byType = new();
    private BaseComponent[] _components = Array.Empty<BaseComponent>();

    public BaseComponent[]? Components
    {
        get => _components;
        set
        {
            _components = value ?? Array.Empty<BaseComponent>();
            _byType.Clear();
            foreach (var c in _components)
                _byType[c.GetType()] = c; // last one wins
        }
    }

    public virtual void Initialize()
    {
        foreach (var component in Components) component.Initialize();
        foreach (var child in Children) child.Initialize();

        LatibuleGame.GameWorld.AddObject(this);
    }

    public virtual void Update(GameTime gameTime)
    {
        foreach (var component in Components) component.Update(gameTime);
        foreach (var child in Children) child.Update(gameTime);
    }

    public virtual void Draw(GameTime gameTime)
    {
        foreach (var component in Components) component.Draw(gameTime);
        foreach (var child in Children) child.Draw(gameTime);
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