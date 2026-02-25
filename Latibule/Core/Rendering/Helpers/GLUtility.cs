using OpenTK.Graphics.OpenGL4;
using static Latibule.Core.Logger;

namespace Latibule.Core.Rendering.Helpers;

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