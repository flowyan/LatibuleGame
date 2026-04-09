using Engine.Data;
using Engine.Rendering;
using Latibule.Core.Gameplay;
using Latibule.Core.Types;
using Latibule.Data;
using Latibule.Data.Model;
using Latibule.Data.Sound;
using Latibule.Data.Texture;
using OpenTK.Mathematics;

namespace Latibule.Items;

public class ItemMaxwell : IItem
{
    public ViewModelChungus ViewModel { get; } = new(
        Asseteer.GetModel(Models.Misc.maxwell),
        Asseteer.GetTextures([Textures.Models.Maxwell.maxwell, Textures.Models.Maxwell.whiskers]),
        new Vector3(0, -0.5f, -2),
        new Vector3(0, 180, 0),
        new Vector3(1f)
    );

    public void Use()
    {
        Asseteer.PlaySound(Sound.Misc.meow, 0.8f);
    }

    public void SecondaryUse()
    {
    }
}