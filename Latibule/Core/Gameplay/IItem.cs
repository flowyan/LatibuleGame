using Engine.Rendering;
using Latibule.Core.Types;

namespace Latibule.Core.Gameplay;

public interface IItem
{
    public ViewModelChungus? ViewModel { get; }

    public void Use();
    public void SecondaryUse();
}