using Engine.Data;
using Engine.Rendering;
using Latibule.Core.Data;
using Latibule.Core.Gameplay;
using Latibule.Core.Types;
using OpenTK.Mathematics;

namespace Latibule.Items;

public class ItemMaxwell : IItem
{
    public ViewModelChungus ViewModel { get; } = new(
        Asseteer.GetModel(ModelAsset.maxwell),
        Asseteer.GetTextures([TextureAsset.maxwell_maxwell, TextureAsset.maxwell_whiskers]),
        new Vector3(0, -0.5f, -2),
        new Vector3(0, 180, 0),
        new Vector3(1f)
    );

    public void Use()
    {
        Asseteer.PlaySound(SoundAsset.meow, 0.8f);
    }

    public void SecondaryUse()
    {
    }
}