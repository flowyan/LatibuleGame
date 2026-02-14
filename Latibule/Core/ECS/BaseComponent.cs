using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Latibule.Core.ECS;

public abstract class BaseComponent()
{
    public virtual void OnLoad()
    {
    }

    public virtual void OnUpdateFrame(FrameEventArgs args)
    {
    }

    public virtual void Dispose()
    {
    }

    public virtual void OnRenderFrame(FrameEventArgs args)
    {
    }
}