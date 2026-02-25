namespace Latibule.Core.Rendering;

public interface IRenderable
{
    public RenderLayer Layer { get; }
    public void Render();
}