namespace Editor.Data;

// this is used for serializing etc. for actual real config
public class EditorConfig
{
    public static string LastOpenedProject = "";
    public static CameraLock CameraLockMode = CameraLock.Hold;
    
    public enum CameraLock 
    {
        Hold,
        Toggle
    }
}