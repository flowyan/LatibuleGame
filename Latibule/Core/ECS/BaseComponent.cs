using OpenTK.Windowing.Common;

namespace Latibule.Core.ECS;

public abstract class BaseComponent : IComponent
{
    public GameObject Parent { get; set; }

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