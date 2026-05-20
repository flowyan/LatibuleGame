using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Engine.Core;

public static class Input
{
    public static KeyboardState KeyboardState = null!;
    public static MouseState MouseState = null!;

    // Keyboard
    private static readonly List<KeyCombo> _comboPressed = new();
    private static readonly Dictionary<Keys, List<Action>> _keyPressed = new();
    private static readonly Dictionary<Keys, List<Action>> _keyReleased = new();

    // Mouse
    private static readonly Dictionary<MouseButton, List<Action>> _mousePressed = new();
    private static readonly Dictionary<MouseButton, List<Action>> _mouseReleased = new();

    public static void Initialize(KeyboardState keyboardState, MouseState mouseState)
    {
        KeyboardState = keyboardState;
        MouseState = mouseState;
    }

    public static void Update(KeyboardState keyboardState, MouseState mouseState)
    {
        KeyboardState = keyboardState;
        MouseState = mouseState;

        #region Keyboard State

        foreach (var (key, actions) in _keyPressed)
            if (KeyboardState.IsKeyPressed(key))
                foreach (var t in actions)
                    t();

        foreach (var (key, actions) in _keyReleased)
            if (KeyboardState.IsKeyReleased(key))
                foreach (var t in actions)
                    t();

        foreach (var combo in _comboPressed)
        {
            if (!KeyboardState.IsKeyPressed(combo.Trigger))
                continue;

            bool allHeld = true;
            for (int i = 0; i < combo.Held.Length; i++)
            {
                if (!KeyboardState.IsKeyDown(combo.Held[i]))
                {
                    allHeld = false;
                    break;
                }
            }

            if (allHeld)
                combo.Action();
        }

        #endregion

        #region Mouse State

        foreach (var (button, actions) in _mousePressed)
            if (MouseState.IsButtonPressed(button))
                foreach (var t in actions)
                    t();
        foreach (var (button, actions) in _mouseReleased)
            if (MouseState.IsButtonReleased(button))
                foreach (var t in actions)
                    t();

        #endregion
    }

    public static void BindKeyPressed(Keys key, Action action) => KeyAdd(_keyPressed, key, action);
    public static void BindKeyReleased(Keys key, Action action) => KeyAdd(_keyReleased, key, action);

    public static void BindComboPressed(Keys triggerKey, Action action, params Keys[] mustBeHeld)
    {
        _comboPressed.Add(new KeyCombo(triggerKey, mustBeHeld, action));
    }


    public static bool IsKeyDown(Keys key) => KeyboardState.IsKeyDown(key);
    public static bool IsKeyPressed(Keys key) => KeyboardState.IsKeyPressed(key);
    public static bool IsKeyReleased(Keys key) => KeyboardState.IsKeyReleased(key);

    private static void KeyAdd(Dictionary<Keys, List<Action>> map, Keys key, Action action)
    {
        if (!map.TryGetValue(key, out var list))
        {
            list = new List<Action>(1);
            map[key] = list;
        }

        list.Add(action);
    }

    public static void BindMousePressed(MouseButton mouseButton, Action action) => MouseButtonAdd(_mousePressed, mouseButton, action);
    public static void BindMouseReleased(MouseButton mouseButton, Action action) => MouseButtonAdd(_mouseReleased, mouseButton, action);

    public static bool IsMouseDown(MouseButton mouseButton) => MouseState.IsButtonDown(mouseButton);
    public static bool IsMousePressed(MouseButton mouseButton) => MouseState.IsButtonPressed(mouseButton);
    public static bool IsMouseReleased(MouseButton mouseButton) => MouseState.IsButtonReleased(mouseButton);

    private static void MouseButtonAdd(Dictionary<MouseButton, List<Action>> map, MouseButton button, Action action)
    {
        if (!map.TryGetValue(button, out var list))
        {
            list = new List<Action>(1);
            map[button] = list;
        }

        list.Add(action);
    }

    private readonly struct KeyCombo
    {
        public readonly Keys Trigger;
        public readonly Keys[] Held;
        public readonly Action Action;

        public KeyCombo(Keys trigger, Keys[] held, Action action)
        {
            Trigger = trigger;
            Held = held;
            Action = action;
        }
    }
}