using OpenTK.Graphics.OpenGL4;
using static Latibule.Core.Logger;

namespace Latibule.Core.Rendering.Helpers;

internal static class GLUtility
{
    public static void CheckError()
    {
        var error = GL.GetError();
        if (error != ErrorCode.NoError)
            LogError($"GL.GetError() returned {error.ToString()}");
            // throw new Exception("GL.GetError() returned " + error.ToString());
    }
}