using Microsoft.Xna.Framework.Input;

namespace Latibule.Core;

public static class Input
{
    /// <summary>
    /// Checks if the specified key is pressed now.
    ///
    /// Will make it, so if you hold down the key, it won't trigger multiple times.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the specified key is pressed now; otherwise, false.</returns>
    public static bool IsKeyPressedNow(Keys key) => GameStates.KState.IsKeyDown(key) && GameStates.PreviousKState.IsKeyUp(key);

    /// <summary>
    /// Checks if all keys are pressed at the same time
    ///
    /// First key must be pressed first.
    /// </summary>
    /// <param name="keys">Keys to check</param>
    /// <returns>True if all keys are pressed</returns>
    public static bool AreKeysPressedNow(params Keys[] keys)
    {
        var first = IsKeyPressed(keys[0]);
        var rest = keys.Skip(1).All(IsKeyPressedNow);

        return first && rest;
    }

    /// <summary>
    /// Checks if the specified key is currently pressed.
    /// </summary>
    /// <param name="key">The key to check for a pressed state.</param>
    /// <returns>True if the specified key is pressed; otherwise, false.</returns>
    public static bool IsKeyPressed(Keys key) => GameStates.KState.IsKeyDown(key);

    /// <summary>
    /// Checks if a key is released.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the specified key has been released; otherwise, false.</returns>
    public static bool IsKeyReleased(Keys key) => GameStates.PreviousKState.IsKeyDown(key);

    /// <summary>
    /// Checks if all specified keys are currently being pressed.
    /// </summary>
    /// <param name="keys">An array of keys to check.</param>
    /// <returns>True if all specified keys are pressed; otherwise, false.</returns>
    public static bool AreKeysPressed(params Keys[] keys) => keys.All(IsKeyPressed);
}