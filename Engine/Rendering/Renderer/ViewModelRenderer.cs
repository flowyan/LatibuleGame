using Assimp;
using Engine.Core.ECS;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace Engine.Rendering.Renderer;

public class ViewModelRenderer : ModelRenderer
{
    private readonly Shader _shader;
    private readonly Scene _scene;
    private readonly Transform _transform;

    private static float _viewmodelfov = 50f;
    private static Matrix4 Projection { get; set; }

    private const float NearPlaneDistance = 0.0001f; // Near plane distance for projection matrix
    private const float FarPlaneDistance = 20f; // Far plane distance for projection matrix

    public ViewModelRenderer(
        Shader shader,
        Scene model,
        Transform transform,
        IReadOnlyList<Texture>? texturesByMaterial,
        Texture? forceAllTexture = null
    ) : base(shader, model, transform, texturesByMaterial, forceAllTexture)
    {
        _shader = shader;
        _scene = model;
        _transform = transform;

        // _fallback = Asseteer.GetTexture(TextureAsset.missing);
        _texturesByMaterial = texturesByMaterial;
        _forceAllTexture = forceAllTexture;

        UpdateProjectionMatrix();
        BuildGpuMeshes(_scene);
    }

    public static void UpdateProjectionMatrix()
    {
        Projection = Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(_viewmodelfov),
            LatibuleEngine.Camera.AspectRatio,
            NearPlaneDistance,
            FarPlaneDistance);
    }

    public override void Render()
    {
        var camera = LatibuleEngine.Camera;

        // Keep the viewmodel fully in camera space.
        var rootModel =
            Matrix4.CreateScale(_transform.Scale) *
            Matrix4.CreateRotationX(MathHelper.DegreesToRadians(_transform.Rotation.X)) *
            Matrix4.CreateRotationY(MathHelper.DegreesToRadians(_transform.Rotation.Y)) *
            Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(_transform.Rotation.Z)) *
            Matrix4.CreateTranslation(_transform.Position);

        _shader.Use();

        // viewmodel lives in view-space, so view is identity
        _shader.SetUniform("view", Matrix4.Identity);
        _shader.SetUniform("projection", Projection);
        _shader.SetUniform("viewPos", Vector3.Zero);

        _shader.SetUniform("material.diffuse", 0);
        _shader.SetUniform("material.specular", 0);
        _shader.SetUniform("material.shininess", 0.0f);

        // Convert world lights to camera/view space so they match viewmodel FragPos space.
        LightRenderer.Render(_shader, camera.View);

        _opaque.Clear();
        _transparent.Clear();

        // collect
        CollectNode(_scene.RootNode, Matrix4.Identity, rootModel, Vector3.Zero);

        // ===== FORCE VIEWMODEL ON TOP =====
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Always);
        GL.DepthMask(false);

        // draw opaque
        GL.Disable(EnableCap.Blend);
        foreach (var cmd in _opaque)
            Draw(cmd.mesh, cmd.model);

        // draw transparent
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _transparent.Sort((a, b) => b.sortKey.CompareTo(a.sortKey));
        foreach (var cmd in _transparent)
            Draw(cmd.mesh, cmd.model);

        // ===== RESTORE NORMAL DEPTH =====
        GL.DepthFunc(DepthFunction.Less);
        GL.DepthMask(true);
    }
}