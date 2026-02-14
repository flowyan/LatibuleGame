namespace Latibule.Models;

public interface IGuiScreen
{
    public void Initialize();
    public void Draw(float deltaTime);
}