using Latibule.Core.Rendering;
using OpenTK.Windowing.Common;

namespace Latibule.Core.ECS;

public interface IComponent : IDisposable
{
    public GameObject Parent { get; internal set; }
    public RenderLayer RenderLayer { get; }

    public void OnLoad(GameObject parent);

    public void OnUpdateFrame(FrameEventArgs args);

    public void OnRenderFrame(FrameEventArgs args);

    void IDisposable.Dispose()
    {
        GC.SuppressFinalize(this);
    }
}