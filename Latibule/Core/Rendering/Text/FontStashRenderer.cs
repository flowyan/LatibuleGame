// https://gitlab.acidiclight.dev/sociallydistant/sociallydistant/-/blob/master/vendor/FontStashSharp/samples/FontStashSharp.Samples.OpenTK/Platform/Renderer.cs

using System.Numerics;
using FontStashSharp.Interfaces;
using Latibule.Core.Data;
using Latibule.Core.ECS;
using Latibule.Core.Rendering.Helpers;
using Latibule.Core.Types;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Vector4 = OpenTK.Mathematics.Vector4;

namespace Latibule.Core.Rendering.Text;

public class FontStashRenderer : IFontStashRenderer2, IDisposable
{
    private const int MAX_SPRITES = 2048;
    private const int MAX_VERTICES = MAX_SPRITES * 4;
    private const int MAX_INDICES = MAX_SPRITES * 6;

    private readonly Shader _shader;
    private readonly BufferObject<VertexPositionColorTexture> _vertexBuffer;
    private readonly BufferObject<short> _indexBuffer;
    private readonly VertexArrayObject _vao;
    private readonly VertexPositionColorTexture[] _vertexData = new VertexPositionColorTexture[MAX_VERTICES];
    private object _lastTexture;
    private int _vertexIndex = 0;

    private readonly Texture2DManager _textureManager;

    public ITexture2DManager TextureManager => _textureManager;

    private static readonly short[] indexData = GenerateIndexArray();

    public unsafe FontStashRenderer()
    {
        _textureManager = new Texture2DManager();

        _vertexBuffer = new BufferObject<VertexPositionColorTexture>(MAX_VERTICES, BufferTarget.ArrayBuffer, true);
        _indexBuffer = new BufferObject<short>(indexData.Length, BufferTarget.ElementArrayBuffer, false);
        _indexBuffer.SetData(indexData, 0, indexData.Length);

        _shader = Asseteer.GetShader(ShaderAsset.text_shader);
        _shader.Use();

        _vao = new VertexArrayObject(sizeof(VertexPositionColorTexture));
        _vao.Bind();

        var location = _shader.GetAttribLocation("aPos");
        _vao.VertexAttribPointer(location, 3, VertexAttribPointerType.Float, false, 0);

        location = _shader.GetAttribLocation("aColor");
        _vao.VertexAttribPointer(location, 4, VertexAttribPointerType.UnsignedByte, true, 12);

        location = _shader.GetAttribLocation("aTexCoords");
        _vao.VertexAttribPointer(location, 2, VertexAttribPointerType.Float, false, 16);
    }

    ~FontStashRenderer() => Dispose(false);
    public void Dispose() => Dispose(true);

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _vao.Dispose();
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        _shader.Dispose();
    }

    public void Begin()
    {
        GL.Disable(EnableCap.DepthTest);
        GLUtility.CheckError();
        GL.DepthMask(false); // Disable depth writing
        GLUtility.CheckError();
        GL.Enable(EnableCap.Blend);
        GLUtility.CheckError();
        GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
        GLUtility.CheckError();

        _shader.Use();
        _shader.SetUniform("flipGlyphY", 0);
        _shader.SetUniform("textureSampler", 0);

        var projection = Matrix4.CreateOrthographicOffCenter(
            0,
            GameStates.GameWindow.ClientSize.X,
            GameStates.GameWindow.ClientSize.Y,
            0,
            -1f,
            1f);

        var view = Matrix4.Identity;
        var model = Matrix4.Identity;

        _shader.SetUniform("projection", projection);
        _shader.SetUniform("view", view);
        _shader.SetUniform("model", model);

        _vao.Bind();
        _indexBuffer.Bind();
        _vertexBuffer.Bind();
    }

    public void BeginWorld(Transform transform, BillboardEnum? billboard = null)
    {
        GL.DepthMask(false);
        GLUtility.CheckError();
        GL.Enable(EnableCap.Blend);
        GLUtility.CheckError();
        GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
        GLUtility.CheckError();

        if (billboard.HasValue) GL.Enable(EnableCap.CullFace);
        else GL.Disable(EnableCap.CullFace);
        GLUtility.CheckError();

        var camera = LatibuleGame.Player.Camera;

        const float fontPixelToWorld = 0.01f;
        var model =
            Matrix4.CreateScale(fontPixelToWorld) *
            Matrix4.CreateRotationX(MathHelper.DegreesToRadians(transform.Rotation.X)) *
            Matrix4.CreateRotationY(MathHelper.DegreesToRadians(transform.Rotation.Y)) *
            Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(transform.Rotation.Z)) *
            Matrix4.CreateTranslation(transform.Position);

        var invView = camera.View.Inverted();
        var right = invView.Row0.Xyz;
        var up = invView.Row1.Xyz;
        var fwd = invView.Row2.Xyz;

        var fullBillboard = new Matrix4(
            new Vector4(right, 0),
            new Vector4(up, 0),
            new Vector4(fwd, 0),
            Vector4.UnitW
        );

        var yLockedBillboard = new Matrix4(
            new Vector4(right, 0),
            Vector4.UnitY,
            new Vector4(fwd, 0),
            Vector4.UnitW
        );

        _shader.Use();
        _shader.SetUniform("flipGlyphY", 1);

        var modelToUse = billboard switch
        {
            BillboardEnum.Full => fullBillboard * model,
            BillboardEnum.YLocked => yLockedBillboard * model,
            _ => model
        };
        _shader.SetUniform("model", modelToUse);
        _shader.SetUniform("view", camera.View);
        _shader.SetUniform("projection", camera.Projection);

        _vao.Bind();
        _indexBuffer.Bind();
        _vertexBuffer.Bind();
    }

    public void DrawQuad(object texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight, ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
    {
        if (_lastTexture != texture)
        {
            FlushBuffer();
        }

        _vertexData[_vertexIndex++] = topLeft;
        _vertexData[_vertexIndex++] = topRight;
        _vertexData[_vertexIndex++] = bottomLeft;
        _vertexData[_vertexIndex++] = bottomRight;

        _lastTexture = texture;
    }

    public void End()
    {
        FlushBuffer();

        // Restore depth state
        GL.Enable(EnableCap.DepthTest);
        GLUtility.CheckError();
        GL.DepthMask(true);
        GLUtility.CheckError();
    }

    private void FlushBuffer()
    {
        if (_vertexIndex == 0) return;
        _vertexBuffer.SetData(_vertexData, 0, _vertexIndex);
        var texture = (Texture)_lastTexture;
        texture.Bind();
        GL.DrawElements(PrimitiveType.Triangles, _vertexIndex * 6 / 4, DrawElementsType.UnsignedShort, IntPtr.Zero);
        _vertexIndex = 0;
    }

    private static short[] GenerateIndexArray()
    {
        short[] result = new short[MAX_INDICES];
        for (int i = 0, j = 0; i < MAX_INDICES; i += 6, j += 4)
        {
            // CCW
            result[i + 0] = (short)(j + 0); // TL
            result[i + 1] = (short)(j + 2); // BL
            result[i + 2] = (short)(j + 1); // TR

            result[i + 3] = (short)(j + 1); // TR
            result[i + 4] = (short)(j + 2); // BL
            result[i + 5] = (short)(j + 3); // BR
        }

        return result;
    }
}