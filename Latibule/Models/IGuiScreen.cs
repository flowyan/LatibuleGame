using Microsoft.Xna.Framework;

namespace Latibule.Models;

public interface IGuiScreen
{
    public void Initialize();
    public void Draw(GameTime gameTime);
}