using OpenTK;
using SDL3;

namespace Engine.Windowing;

public class SDLBindingsContext : IBindingsContext
{
    public IntPtr GetProcAddress(string procName)
    {
        return SDL.GLGetProcAddress(procName);
    }
}