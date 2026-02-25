using System.Numerics;
using Latibule.Core.Rendering.Helpers;
using Latibule.Utilities;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace Latibule.Core.Rendering;

public class Shader : IDisposable
{
    private readonly int _handle;

    public Shader(string vertexPath, string fragmentPath)
    {
        var vertex = LoadShader(ShaderType.VertexShader, vertexPath);
        var fragment = LoadShader(ShaderType.FragmentShader, fragmentPath);

        _handle = GL.CreateProgram();
        GLUtility.CheckError();

        GL.AttachShader(_handle, vertex);
        GLUtility.CheckError();

        GL.AttachShader(_handle, fragment);
        GLUtility.CheckError();

        GL.LinkProgram(_handle);
        GL.GetProgram(_handle, GetProgramParameterName.LinkStatus, out var status);
        if (status == 0) throw new Exception($"Program failed to link with error: {GL.GetProgramInfoLog(_handle)}");

        GL.DetachShader(_handle, vertex);
        GL.DetachShader(_handle, fragment);

        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);
    }

    public void Use()
    {
        GL.UseProgram(_handle);
        GLUtility.CheckError();
    }

    public void SetUniform(string name, int value)
    {
        var location = GL.GetUniformLocation(_handle, name);
        if (CheckLocation(location, name)) return;
        GL.Uniform1(location, value);
        GLUtility.CheckError();
    }

    public void SetUniform(string name, float value)
    {
        var location = GL.GetUniformLocation(_handle, name);
        if (CheckLocation(location, name)) return;
        GL.Uniform1(location, value);
        GLUtility.CheckError();
    }

    public void SetUniform(string name, Matrix4 value)
    {
        var location = GL.GetUniformLocation(_handle, name);
        if (CheckLocation(location, name)) return;
        GL.ProgramUniformMatrix4(_handle, location, false, ref value);
        GLUtility.CheckError();
    }

    public unsafe void SetUniform(string name, Matrix4x4 value)
    {
        var location = GL.GetUniformLocation(_handle, name);
        if (CheckLocation(location, name)) return;
        GL.ProgramUniformMatrix4(_handle, location, 1, true, (float*)&value);
        GLUtility.CheckError();
    }

    public void SetUniform(string name, Vector3 value)
    {
        var location = GL.GetUniformLocation(_handle, name);
        if (CheckLocation(location, name)) return;
        GL.Uniform3(location, value.X, value.Y, value.Z);
        GLUtility.CheckError();
    }

    private static bool CheckLocation(int location, string name)
    {
        if (location != -1) return false;
        Logger.LogWarning($"Uniform {name} not found in shader.");
        return true;
    }

    public int GetAttribLocation(string attribName)
    {
        var result = GL.GetAttribLocation(_handle, attribName);
        GLUtility.CheckError();
        return result;
    }

    private static int LoadShader(ShaderType type, string path)
    {
        var src = File.ReadAllText(path);
        var handle = GL.CreateShader(type);
        GLUtility.CheckError();

        GL.ShaderSource(handle, src);
        GLUtility.CheckError();

        GL.CompileShader(handle);
        var infoLog = GL.GetShaderInfoLog(handle);
        return !string.IsNullOrWhiteSpace(infoLog) ? throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}") : handle;
    }

    public void Dispose()
    {
        GL.DeleteProgram(_handle);
        GLUtility.CheckError();
        GC.SuppressFinalize(this);
    }
}