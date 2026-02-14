using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Latibule.Core;

public static class Input
{
    private static KeyboardState _ks = null!;

    private static readonly List<KeyCombo> _comboPressed = new();
    private static readonly Dictionary<Keys, List<Action>> _pressed = new();
    private static readonly Dictionary<Keys, List<Action>> _released = new();

    public static void Initialize(KeyboardState keyboardState) => _ks = keyboardState;

    public static void Update(KeyboardState keyboardState)
    {
        _ks = keyboardState;

        foreach (var (key, actions) in _pressed)
            if (_ks.IsKeyPressed(key))
                foreach (var t in actions)
                    t();

        foreach (var (key, actions) in _released)
            if (_ks.IsKeyReleased(key))
                foreach (var t in actions)
                    t();

        foreach (var combo in _comboPressed)
        {
            if (!_ks.IsKeyPressed(combo.Trigger))
                continue;

            bool allHeld = true;
            for (int i = 0; i < combo.Held.Length; i++)
            {
                if (!_ks.IsKeyDown(combo.Held[i]))
                {
                    allHeld = false;
                    break;
                }
            }

            if (allHeld)
                combo.Action();
        }
    }

    public static void BindKeyPressed(Keys key, Action action) => Add(_pressed, key, action);
    public static void BindKeyReleased(Keys key, Action action) => Add(_released, key, action);

    public static void BindComboPressed(Keys triggerKey, Action action, params Keys[] mustBeHeld)
    {
        _comboPressed.Add(new KeyCombo(triggerKey, mustBeHeld, action));
    }


    public static bool IsKeyDown(Keys key) => _ks.IsKeyDown(key);
    public static bool IsKeyPressed(Keys key) => _ks.IsKeyPressed(key);
    public static bool IsKeyReleased(Keys key) => _ks.IsKeyReleased(key);

    private static void Add(Dictionary<Keys, List<Action>> map, Keys key, Action action)
    {
        if (!map.TryGetValue(key, out var list))
        {
            list = new List<Action>(1);
            map[key] = list;
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