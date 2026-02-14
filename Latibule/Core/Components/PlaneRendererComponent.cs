using Latibule.Core.ECS;
using Latibule.Core.Rendering;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Latibule.Core.Components;

public class PlaneRendererComponent : MeshComponent
{
    private Shader _shader = null!;
    private Texture? _texture;

    public override void OnLoad(GameObject gameObject)
    {
        base.OnLoad(gameObject);

        Vertices =
        [
            new(-1f, 0f, -1f),
            new(1f, 0f, -1f),
            new(-1f, 0f, 1f),
            new(1f, 0f, 1f)
        ];

        Indices =
        [
            0, 1, 2, // first triangle
            1, 3, 2 // second triangle
        ];

        var component = gameObject.Require<ShaderComponent>();
        _shader = component.Shader;
        _texture = component.Texture;
        var uvScale = component.UVScale;

        Vertices[0].SetUV(0, uvScale.Y);
        Vertices[1].SetUV(uvScale.X, uvScale.Y);
        Vertices[2].SetUV(0, 0);
        Vertices[3].SetUV(uvScale.X, 0);

        VertexBufferObject = GL.GenBuffer();
        ElementBufferObject = GL.GenBuffer();
        VertexArrayObject = GL.GenVertexArray();

        if (VertexBufferObject == 0 || ElementBufferObject == 0 || VertexArrayObject == 0 || Vertices == null || Indices == null)
        {
            throw new Exception("Failed to generate OpenGL buffers for PlaneMesh.");
        }

        var verticesArray = Vertices.SelectMany(v => v.ToArray()).ToArray();

        GL.BindVertexArray(VertexArrayObject);
        GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, verticesArray.Length * sizeof(float), verticesArray, BufferUsageHint.StaticDraw);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Length * sizeof(uint), Indices, BufferUsageHint.StaticDraw);
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
            Matrix4.CreateScale(Parent.Transform.Scale) *
            Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Parent.Transform.Rotation.X)) *
            Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Parent.Transform.Rotation.Y)) *
            Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Parent.Transform.Rotation.Z)) *
            Matrix4.CreateTranslation(Parent.Transform.Position); // Must be last, order matters in matrices

        _shader.Use();
        _shader.SetMatrix4("model", model);
        _shader.SetMatrix4("view", camera.View);
        _shader.SetMatrix4("projection", camera.Projection);
        _shader.SetInt("texture0", 0); // Set texture sampler to use texture unit 0

        _texture?.Use();

        GL.BindVertexArray(VertexArrayObject);
        GL.DrawElements(PrimitiveType.Triangles, Indices.Length, DrawElementsType.UnsignedInt, 0);
    }
}