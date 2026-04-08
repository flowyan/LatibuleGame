using Assimp;
using Engine.Core.ECS;
using Engine.Data;
using Engine.Rendering;
using Engine.Rendering.Renderer;
using Latibule.Core.Data;
using OpenTK.Mathematics;

namespace Latibule.Core.Types;

public class ViewModelChungus
{
    public ViewModelRenderer? Renderer { get; set; }

    public Scene Model { get; set; }
    public Texture[] Textures { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Scale { get; set; }

    public ViewModelChungus(Scene model, Texture[] textures, Vector3 position, Vector3 rotation, Vector3 scale)
    {
        var _shader = Asseteer.GetShader(InternalShaderAsset.mesh_shader);
        Model = model;
        Textures = textures;
        Position = position;
        Rotation = rotation;
        Scale = scale;

        // TODO: this shit dont render after selecting a different item :(
        Renderer = new ViewModelRenderer(
            _shader,
            model,
            new Transform(position, scale, rotation),
            textures, textures.Length == 1 ? textures[0] : null
        );
    }
}