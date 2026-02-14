using Latibule.Core.Components;
using Latibule.Core.ECS;
using Latibule.Utilities;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using MathHelper = OpenTK.Mathematics.MathHelper;
using static Latibule.Core.Logger;

namespace Latibule.Core.Rendering.Models;

/// <summary>
/// A flat plane for ground/floor rendering and collision.
/// </summary>
public class PlaneModel() : GameObject
{
    private int _vertexBufferObject;
    private int _vertexArrayObject;
    private int _elementBufferObject;

    private Shader _shader = null!;
    private Texture? _texture;

    private readonly uint[] _indices =
    [
        0, 1, 2, // first triangle
        1, 3, 2 // second triangle
    ];

    public override void OnLoad()
    {
        base.OnLoad();

        var component = Require<ShaderComponent>();
        _shader = component.Shader;
        _texture = component.Texture;
        if (_texture == null) LogError("PlaneModel requires a texture in its ShaderComponent!");

        float[] _vertices =
        [
            -1f, 0f, -1f, 0f, UVScale.Y, // bottom left
            1f, 0f, -1f, UVScale.X, UVScale.Y, // bottom right
            -1f, 0f, 1f, 0f, 0f, // top left
            1f, 0f, 1f, UVScale.X, 0f // top right
        ];

        BoundingBox = AabbHelper.CreateFromCenterRotationScale(
            Position,
            new Vector3(Scale.X, 0, Scale.Z),
            Rotation
        );

        _vertexBufferObject = GL.GenBuffer();
        _elementBufferObject = GL.GenBuffer();
        _vertexArrayObject = GL.GenVertexArray();

        GL.BindVertexArray(_vertexArrayObject);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

        var location = _shader.GetAttribLocation("aPosition");
        GL.EnableVertexAttribArray(location);
        GL.VertexAttribPointer(location, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

        int texCoordLocation = _shader.GetAttribLocation("aTexCoord");
        GL.EnableVertexAttribArray(texCoordLocation);
        GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
    }

    public override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        var camera = LatibuleGame.Player.Camera;
        var model =
            Matrix4.CreateScale(Scale) *
            Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Rotation.X)) *
            Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Rotation.Y)) *
            Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rotation.Z)) *
            Matrix4.CreateTranslation(Position); // Must be last, order matters in matrices

        _shader.Use();
        _shader.SetMatrix4("model", model);
        _shader.SetMatrix4("view", camera.View);
        _shader.SetMatrix4("projection", camera.Projection);
        _shader.SetInt("texture0", 0); // Set texture sampler to use texture unit 0

        _texture?.Use();

        GL.BindVertexArray(_vertexArrayObject);
        GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
    }
}