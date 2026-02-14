using OpenTK.Windowing.Common;

namespace Latibule.Models;

public interface IGuiScreen
{
    public void Initialize();
    public void OnRenderFrame(FrameEventArgs args);
}