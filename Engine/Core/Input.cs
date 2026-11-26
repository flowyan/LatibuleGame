using SDL3;

namespace Engine.Core;

public static class Input
{
    private static readonly HashSet<SDL.Scancode> _keysDown = new();
    private static readonly HashSet<SDL.Scancode> _keysPressed = new();
    private static readonly HashSet<SDL.Scancode> _keysReleased = new();

    private static readonly HashSet<MouseButton> _mouseDown = new();
    private static readonly HashSet<MouseButton> _mousePressed = new();
    private static readonly HashSet<MouseButton> _mouseReleased = new();

    private static readonly List<KeyCombo> _comboPressed = new();
    private static readonly Dictionary<SDL.Scancode, List<Action>> _keyPressed = new();
    private static readonly Dictionary<SDL.Scancode, List<Action>> _keyReleased = new();

    private static readonly Dictionary<MouseButton, List<Action>> _mousePressedActions = new();
    private static readonly Dictionary<MouseButton, List<Action>> _mouseReleasedActions = new();
    
    public static float MouseDeltaX { get; private set; }
    public static float MouseDeltaY { get; private set; }

    public static void EndFrame()
    {
        _keysPressed.Clear();
        _keysReleased.Clear();
        _mousePressed.Clear();
        _mouseReleased.Clear();
        
        MouseDeltaX = 0;
        MouseDeltaY = 0;
    }

    // Call this from SDLNativeWindow.AppEvent.
    public static void HandleEvent(ref SDL.Event @event)
    {
        switch ((SDL.EventType)@event.Type)
        {
            case SDL.EventType.KeyDown:
            {
                var key = @event.Key.Scancode;

                // Ignore repeat events; a held key isn't a new press.
                if (!_keysDown.Contains(key) && !@event.Key.Repeat)
                    _keysPressed.Add(key);

                _keysDown.Add(key);
                break;
            }

            case SDL.EventType.KeyUp:
            {
                var key = @event.Key.Scancode;

                if (_keysDown.Remove(key))
                    _keysReleased.Add(key);

                break;
            }

            case SDL.EventType.MouseButtonDown:
            {
                var button = (MouseButton)@event.Button.Button;

                if (_mouseDown.Add(button))
                    _mousePressed.Add(button);

                break;
            }

            case SDL.EventType.MouseButtonUp:
            {
                var button = (MouseButton)@event.Button.Button;

                if (_mouseDown.Remove(button))
                    _mouseReleased.Add(button);

                break;
            }
            
            case SDL.EventType.MouseMotion:
                MouseDeltaX += @event.Motion.XRel;
                MouseDeltaY += @event.Motion.YRel;
                break;
        }
    }

    // Call once per frame, after SDL has processed its events.
    public static void Update()
    {
        foreach (var (key, actions) in _keyPressed)
            if (_keysPressed.Contains(key))
                foreach (var action in actions)
                    action();

        foreach (var (key, actions) in _keyReleased)
            if (_keysReleased.Contains(key))
                foreach (var action in actions)
                    action();

        foreach (var combo in _comboPressed)
        {
            if (!_keysPressed.Contains(combo.Trigger))
                continue;

            if (combo.Held.All(_keysDown.Contains))
                combo.Action();
        }

        foreach (var (button, actions) in _mousePressedActions)
            if (_mousePressed.Contains(button))
                foreach (var action in actions)
                    action();

        foreach (var (button, actions) in _mouseReleasedActions)
            if (_mouseReleased.Contains(button))
                foreach (var action in actions)
                    action();
    }

    public static bool IsKeyDown(SDL.Scancode key) => _keysDown.Contains(key);
    public static bool IsKeyPressed(SDL.Scancode key) => _keysPressed.Contains(key);
    public static bool IsKeyReleased(SDL.Scancode key) => _keysReleased.Contains(key);

    public static bool IsMouseDown(MouseButton button) => _mouseDown.Contains(button);
    public static bool IsMousePressed(MouseButton button) => _mousePressed.Contains(button);
    public static bool IsMouseReleased(MouseButton button) => _mouseReleased.Contains(button);

    public static void BindKeyPressed(SDL.Scancode key, Action action) =>
        KeyAdd(_keyPressed, key, action);

    public static void BindKeyReleased(SDL.Scancode key, Action action) =>
        KeyAdd(_keyReleased, key, action);

    public static void BindComboPressed(
        SDL.Scancode triggerKey,
        Action action,
        params SDL.Scancode[] mustBeHeld)
    {
        _comboPressed.Add(new KeyCombo(triggerKey, mustBeHeld, action));
    }

    public static void BindMousePressed(MouseButton button, Action action) =>
        MouseButtonAdd(_mousePressedActions, button, action);

    public static void BindMouseReleased(MouseButton button, Action action) =>
        MouseButtonAdd(_mouseReleasedActions, button, action);

    private static void KeyAdd(
        Dictionary<SDL.Scancode, List<Action>> map,
        SDL.Scancode key,
        Action action)
    {
        if (!map.TryGetValue(key, out var actions))
            map[key] = actions = new List<Action>();

        actions.Add(action);
    }

    private static void MouseButtonAdd(
        Dictionary<MouseButton, List<Action>> map,
        MouseButton button,
        Action action)
    {
        if (!map.TryGetValue(button, out var actions))
            map[button] = actions = new List<Action>();

        actions.Add(action);
    }

    private readonly record struct KeyCombo(
        SDL.Scancode Trigger,
        SDL.Scancode[] Held,
        Action Action);
}

public enum MouseButton : byte
{
    Left = 1,
    Middle = 2,
    Right = 3,
    X1 = 4,
    X2 = 5
}