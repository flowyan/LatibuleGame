using OpenTK.Windowing.Common;

namespace Engine.Core.Types;

public interface IGuiScreen
{
    public void Initialize();
    public void OnRenderFrame(FrameEventArgs args);
}