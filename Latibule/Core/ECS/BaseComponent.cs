using Latibule.Core.Rendering;
using OpenTK.Windowing.Common;

namespace Latibule.Core.ECS;

public abstract class BaseComponent : IComponent
{
    /// <summary>
    /// Gets set during OnLoad, will be null otherwise!!!
    /// </summary>
    public GameObject Parent { get; set; } = null!;

    public RenderLayer RenderLayer { get; set; } = RenderLayer.World;

    public virtual void OnLoad(GameObject gameObject)
    {
        Parent = gameObject;
    }

    public virtual void OnUpdateFrame(FrameEventArgs args)
    {
    }

    public virtual void OnRenderFrame(FrameEventArgs args)
    {
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}