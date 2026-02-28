using OpenTK.Windowing.Common;

namespace Latibule.Core.Types;

public interface IGuiScreen
{
    public void Initialize();
    public void OnRenderFrame(FrameEventArgs args);
}