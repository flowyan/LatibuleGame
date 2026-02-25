using Latibule.Core.Components;
using Latibule.Core.Rendering;
using OpenTK.Windowing.Common;

namespace Latibule.Core.ECS;

public class GameObject() : IDisposable
{
    public Transform Transform { get; set; } = new();
    public GameObject? Parent { get; private set; }
    public GameObject[] Children { get; private set; } = Array.Empty<GameObject>();

    private readonly Dictionary<Type, IComponent> _byType = new();

    public IComponent[] Components
    {
        get;
        private set
        {
            field = value;
            _byType.Clear();
            foreach (var c in field)
                _byType[c.GetType()] = c; // last one wins
        }
    } = [];

    public virtual void OnLoad()
    {
        foreach (var component in Components) component.OnLoad(this);
        foreach (var child in Children) child.OnLoad();

        LatibuleGame.GameWorld.AddObject(this);
    }

    public virtual void OnUpdateFrame(FrameEventArgs args)
    {
        foreach (var component in Components) component.OnUpdateFrame(args);
        foreach (var child in Children) child.OnUpdateFrame(args);
    }

    public virtual void OnRenderFrame(FrameEventArgs args, RenderLayer layer)
    {
        foreach (var component in Components)
            if (component.RenderLayer == layer)
                component.OnRenderFrame(args);
        foreach (var child in Children) child.OnRenderFrame(args, layer);
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

    public GameObject WithComponent<T>(T component) where T : IComponent
    {
        component.Parent = this;
        component.OnLoad(this);
        _byType[typeof(T)] = component;
        Components = _byType.Values.ToArray();
        return this;
    }

    public GameObject WithComponents(params IComponent[] components)
    {
        foreach (var component in components)
        {
            component.Parent = this;
            component.OnLoad(this);
            _byType[component.GetType()] = component;
        }

        Components = _byType.Values.ToArray();
        return this;
    }

    public GameObject UpdateComponent<T>(Action<T> updateAction) where T : IComponent
    {
        if (_byType.TryGetValue(typeof(T), out var c) && c is T component)
        {
            updateAction(component);
            _byType[typeof(T)] = component; // Update the stored component
            Components = _byType.Values.ToArray(); // Refresh the Components array
        }
        else
        {
            throw new InvalidOperationException($"Component of type {typeof(T).Name} not found.");
        }

        return this;
    }

    public T? Get<T>() where T : IComponent => _byType.TryGetValue(typeof(T), out var c) ? (T)c : default;

    public T Require<T>() where T : IComponent => Get<T>() ?? throw new InvalidOperationException($"Missing required component: {typeof(T).Name}");
}