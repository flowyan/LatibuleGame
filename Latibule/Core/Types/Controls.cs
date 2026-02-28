namespace Latibule.Core.Types;

public static class Controls
{
    // cooldown for left and right mouse buttons
    private static DateTime _lastClickTime = DateTime.MinValue; // Initialize to MinValue instead of Now

    public static bool Cooldown(int ms)
    {
        var now = DateTime.Now;
        if ((now - _lastClickTime).TotalMilliseconds > ms)
        {
            _lastClickTime = now;
            return true;
        }

        return false;
    }

    public static void ResetCooldown()
    {
        _lastClickTime = DateTime.MinValue; // Reset to MinValue
    }
}