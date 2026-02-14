namespace Latibule.Models;

public interface IGuiScreen
{
    public void Initialize();
    public void OnRenderFrame(float deltaTime);
}