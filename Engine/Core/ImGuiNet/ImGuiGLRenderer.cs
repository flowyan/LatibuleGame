using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Engine.Core.ImGuiNet;

// For ImGui.NET 1.91.x and OpenTK 4.x
public sealed class ImGuiGLRenderer : IDisposable
{
    private readonly int _program, _projLocation, _vao, _vbo, _ebo, _fontTexture;

    private const string VertexSource = @"#version 330 core
layout(location = 0) in vec2 aPos;
layout(location = 1) in vec2 aUV;
layout(location = 2) in vec4 aColor;
uniform mat4 uProj;
out vec2 vUV;
out vec4 vColor;
void main() { vUV = aUV; vColor = aColor; gl_Position = uProj * vec4(aPos, 0.0, 1.0); }";

    private const string FragmentSource = @"#version 330 core
in vec2 vUV;
in vec4 vColor;
uniform sampler2D uTexture;
out vec4 fragColor;
void main() { fragColor = vColor * texture(uTexture, vUV); }";

    public ImGuiGLRenderer()
    {
        var io = ImGui.GetIO();
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

        int vs = Compile(ShaderType.VertexShader, VertexSource);
        int fs = Compile(ShaderType.FragmentShader, FragmentSource);
        _program = GL.CreateProgram();
        GL.AttachShader(_program, vs);
        GL.AttachShader(_program, fs);
        GL.LinkProgram(_program);
        GL.GetProgram(_program, GetProgramParameterName.LinkStatus, out int linked);
        if (linked == 0) throw new Exception(GL.GetProgramInfoLog(_program));
        GL.DeleteShader(vs);
        GL.DeleteShader(fs);
        _projLocation = GL.GetUniformLocation(_program, "uProj");

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        _ebo = GL.GenBuffer();
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);

        int stride = Unsafe.SizeOf<ImDrawVert>();
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 8);
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, 16);
        GL.BindVertexArray(0);

        io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out _);
        _fontTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _fontTexture);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0,
            PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
        io.Fonts.SetTexID((IntPtr)_fontTexture);
        io.Fonts.ClearTexData();
    }

    private static int Compile(ShaderType type, string source)
    {
        int shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int ok);
        if (ok == 0) throw new Exception(GL.GetShaderInfoLog(shader));
        return shader;
    }

    public void Render(ImDrawDataPtr data)
    {
        var pos = data.DisplayPos;
        var size = data.DisplaySize;
        var scale = data.FramebufferScale;
        int fbWidth = (int)(size.X * scale.X);
        int fbHeight = (int)(size.Y * scale.Y);
        if (fbWidth <= 0 || fbHeight <= 0) return;

        // Note: this changes GL state. Set what your scene needs again each frame.
        GL.Viewport(0, 0, fbWidth, fbHeight);
        GL.Enable(EnableCap.Blend);
        GL.BlendEquation(BlendEquationMode.FuncAdd);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Disable(EnableCap.CullFace);
        GL.Disable(EnableCap.DepthTest);
        GL.Enable(EnableCap.ScissorTest);

        var proj = Matrix4.CreateOrthographicOffCenter(
            pos.X, pos.X + size.X, pos.Y + size.Y, pos.Y, -1f, 1f);
        GL.UseProgram(_program);
        GL.UniformMatrix4(_projLocation, false, ref proj);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

        for (int n = 0; n < data.CmdListsCount; n++)
        {
            var list = data.CmdLists[n];
            GL.BufferData(BufferTarget.ArrayBuffer, list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(),
                list.VtxBuffer.Data, BufferUsageHint.StreamDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer, list.IdxBuffer.Size * sizeof(ushort),
                list.IdxBuffer.Data, BufferUsageHint.StreamDraw);

            for (int i = 0; i < list.CmdBuffer.Size; i++)
            {
                var cmd = list.CmdBuffer[i];
                if (cmd.UserCallback != IntPtr.Zero) continue;

                float x1 = (cmd.ClipRect.X - pos.X) * scale.X;
                float y1 = (cmd.ClipRect.Y - pos.Y) * scale.Y;
                float x2 = (cmd.ClipRect.Z - pos.X) * scale.X;
                float y2 = (cmd.ClipRect.W - pos.Y) * scale.Y;
                if (x2 <= x1 || y2 <= y1) continue;

                // GL's scissor origin is bottom-left
                GL.Scissor((int)x1, (int)(fbHeight - y2), (int)(x2 - x1), (int)(y2 - y1));
                GL.BindTexture(TextureTarget.Texture2D, (int)cmd.GetTexID());
                GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)cmd.ElemCount,
                    DrawElementsType.UnsignedShort,
                    (IntPtr)(cmd.IdxOffset * sizeof(ushort)), (int)cmd.VtxOffset);
            }
        }

        GL.Disable(EnableCap.ScissorTest);
        GL.BindVertexArray(0);
        GL.UseProgram(0);
    }

    public void Dispose()
    {
        GL.DeleteTexture(_fontTexture);
        GL.DeleteBuffer(_vbo);
        GL.DeleteBuffer(_ebo);
        GL.DeleteVertexArray(_vao);
        GL.DeleteProgram(_program);
    }
}