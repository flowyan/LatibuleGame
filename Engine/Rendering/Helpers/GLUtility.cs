using OpenTK.Graphics.OpenGL4;

namespace Engine.Rendering.Helpers;

internal static class GLUtility
{
    public static void CheckError([System.Runtime.CompilerServices.CallerMemberName] string where = "")
    {
        ErrorCode e;
        bool any = false;

        while ((e = GL.GetError()) != ErrorCode.NoError)
        {
            any = true;
            throw new Exception($"OpenGL error at {where}: {e}");
        }
    }
}