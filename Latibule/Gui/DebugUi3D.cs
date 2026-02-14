using System.Drawing;
using Latibule.Core.Physics;
using Latibule.Core.Rendering;
using Latibule.Entities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Latibule.Gui;

public sealed class DebugUi3D : IDisposable
{
    public bool ShowBoundingBoxes { get; set; } = true;
    public Dictionary<Tuple<BoundingBox, Color>, bool> DrawBox { get; set; } = new();

    private readonly Shader _lineShader;

    private const int VertexCount = 24;
    private const int FloatsPerVertex = 6; // pos.xyz + color.rgb
    private readonly float[] _data = new float[VertexCount * FloatsPerVertex];
    private const float LineWidth = 2.0f; // Line width in pixels

    private int _vao;
    private int _vbo;

    public DebugUi3D(Shader lineShader)
    {
        _lineShader = lineShader ?? throw new ArgumentNullException(nameof(lineShader));

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

    public void OnRenderFrame(Player player)
    {
        if (!ShowBoundingBoxes) return;

        // Pick your preference:
        GL.Enable(EnableCap.DepthTest);   // occluded by world
        // GL.Disable(EnableCap.DepthTest);  // always visible

        DrawBoundingBoxOutline(player.BoundingBox, Color.White);

        foreach (var boundingBox in LatibuleGame.GameWorld.GetBoundingBoxes())
            DrawBoundingBoxOutline(boundingBox, Color.Yellow);

        foreach (var box in DrawBox.Where(b => b.Value))
            DrawBoundingBoxOutline(box.Key.Item1, box.Key.Item2);

        DrawBox.Clear();
    }

    public void DrawBoundingBoxOutline(BoundingBox box, Color color)
    {
        Vector3[] c =
        [
            new(box.Min.X, box.Min.Y, box.Min.Z),
            new(box.Max.X, box.Min.Y, box.Min.Z),
            new(box.Max.X, box.Min.Y, box.Max.Z),
            new(box.Min.X, box.Min.Y, box.Max.Z),
            new(box.Min.X, box.Max.Y, box.Min.Z),
            new(box.Max.X, box.Max.Y, box.Min.Z),
            new(box.Max.X, box.Max.Y, box.Max.Z),
            new(box.Min.X, box.Max.Y, box.Max.Z),
        ];

        Span<Vector3> pts =
        [
            // bottom
            c[0], c[1],
            c[1], c[2],
            c[2], c[3],
            c[3], c[0],

            // top
            c[4], c[5],
            c[5], c[6],
            c[6], c[7],
            c[7], c[4],

            // vertical
            c[0], c[4],
            c[1], c[5],
            c[2], c[6],
            c[3], c[7]
        ];

        var rgb = new Vector3(color.R / 255f, color.G / 255f, color.B / 255f);

        int o = 0;
        for (int i = 0; i < pts.Length; i++)
        {
            var p = pts[i];
            _data[o++] = p.X;
            _data[o++] = p.Y;
            _data[o++] = p.Z;
            _data[o++] = rgb.X;
            _data[o++] = rgb.Y;
            _data[o++] = rgb.Z;
        }

        // Save current GL state
        float savedLineWidth = 1.0f;
        GL.GetFloat(GetPName.LineWidth, out savedLineWidth);
        int savedProgram = 0;
        GL.GetInteger(GetPName.CurrentProgram, out savedProgram);

        // Bind VAO and VBO before updating buffer data
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr)(_data.Length * sizeof(float)), _data);

        // Setup shader and matrices
        _lineShader.Use();

        // For debug lines, we need to transform from world space to clip space
        // This requires: Projection * View * Position
        var camera = LatibuleGame.Player.Camera;

        // Build the MVP matrix
        Matrix4 projection = camera.Projection;
        Matrix4 view = camera.View;

        // OpenTK uses row-major matrices, so the order is: view * projection
        // When passed to the shader (which is column-major), this becomes the correct MVP
        Matrix4 mvp = view * projection;

        _lineShader.SetMatrix4("uMVP", mvp);

        // Set line width for visibility
        GL.LineWidth(LineWidth);

        // Enable line smoothing for better visual quality
        GL.Enable(EnableCap.LineSmooth);
        GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);

        // Draw the lines
        GL.DrawArrays(PrimitiveType.Lines, 0, pts.Length);

        // Disable line smoothing
        GL.Disable(EnableCap.LineSmooth);

        // Restore GL state
        GL.LineWidth(savedLineWidth);
        if (savedProgram != 0)
            GL.UseProgram(savedProgram);
        GL.BindVertexArray(0);
    }

    public void Dispose()
    {
        DrawBox.Clear();

        if (_vbo != 0) GL.DeleteBuffer(_vbo);
        if (_vao != 0) GL.DeleteVertexArray(_vao);

        _vbo = _vao = 0;
        GC.SuppressFinalize(this);
    }
}
