using Engine.Data;
using Engine.Data.Shaders;
using Engine.Rendering;
using JoltPhysicsSharp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Vector3 = System.Numerics.Vector3;

namespace Engine.Physics;

public class JoltPhysicsDebugRenderer : DebugRenderer, IDisposable
{
    private Shader _lineShader;

    private const int VertexCount = 24;
    private const int FloatsPerVertex = 6; // pos.xyz + color.rgb
    private const int LineVertexCount = 2;
    private const int LineFloatCount = LineVertexCount * FloatsPerVertex;
    private const float LineWidth = 1.0f; // Line width in pixels
    private readonly float[] _data = new float[VertexCount * FloatsPerVertex];

    private int _vao;
    private int _vbo;

    public JoltPhysicsDebugRenderer()
    {
        _lineShader = Asseteer.GetShader(EngineShaders.DebugUi);

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();

        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

        GL.BufferData(
            BufferTarget.ArrayBuffer,
            (IntPtr)(_data.Length * sizeof(float)),
            IntPtr.Zero,
            BufferUsageHint.DynamicDraw
        );

        int stride = FloatsPerVertex * sizeof(float);

        int posLoc = _lineShader.GetAttribLocation("aPosition");
        int colLoc = _lineShader.GetAttribLocation("aColor");

        if (posLoc >= 0)
        {
            GL.EnableVertexAttribArray(posLoc);
            GL.VertexAttribPointer(posLoc, 3, VertexAttribPointerType.Float, false, stride, 0);
        }

        if (colLoc >= 0)
        {
            GL.EnableVertexAttribArray(colLoc);
            GL.VertexAttribPointer(colLoc, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
    }

    protected override void DrawLine(Vector3 from, Vector3 to, JoltColor color)
    {
        GL.Enable(EnableCap.DepthTest); // occluded by world
        // Setup shader and matrices
        _lineShader.Use();

        // For debug lines, we need to transform from world space to clip space
        // This requires: Projection * View * Position
        var camera = LatibuleEngine.Camera;

        // Build the MVP matrix
        Matrix4 projection = camera.Projection;
        Matrix4 view = camera.View;

        // OpenTK uses row-major matrices, so the order is: view * projection
        // When passed to the shader (which is column-major), this becomes the correct MVP
        Matrix4 mvp = view * projection;

        _lineShader.SetUniform("uMVP", mvp);

        // Set line width
        GL.LineWidth(LineWidth);

        // Update vertex data for the line
        _data[0] = from.X;
        _data[1] = from.Y;
        _data[2] = from.Z;
        _data[3] = color.R;
        _data[4] = color.G;
        _data[5] = color.B;

        _data[6] = to.X;
        _data[7] = to.Y;
        _data[8] = to.Z;
        _data[9] = color.R;
        _data[10] = color.G;
        _data[11] = color.B;

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr)(LineFloatCount * sizeof(float)), _data);

        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Lines, 0, LineVertexCount);
        GL.BindVertexArray(0);
    }

    protected override void DrawText3D(Vector3 position, string? text, JoltColor color, float height = 0.5f)
    {
        // Not implemented
    }

    public new void Dispose()
    {
        if (_vbo != 0) GL.DeleteBuffer(_vbo);
        if (_vao != 0) GL.DeleteVertexArray(_vao);

        _vbo = _vao = 0;
        GC.SuppressFinalize(this);
    }
}