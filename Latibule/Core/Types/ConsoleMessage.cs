using System.Numerics;

namespace Latibule.Core.Types;

public class ConsoleMessage
{
    public string Content { get; }
    public ConsoleMessageType Type { get; }
    public Vector4 Color { get; }

    public ConsoleMessage(string content, ConsoleMessageType type, Vector4? color = null)
    {
        color ??= type switch
        {
            ConsoleMessageType.Error => new Vector4(1, 0, 0, 1),
            ConsoleMessageType.Warning => new Vector4(1, 1, 0, 1),
            ConsoleMessageType.Debug => new Vector4(1, 0, 1, 1),
            ConsoleMessageType.Info => new Vector4(1, 1, 1, 1),
            ConsoleMessageType.CommandOutput => new Vector4(1, 1, 1, 1),
            _ => new Vector4(1, 1, 1, 1)
        };

        Content = content;
        Type = type;
        Color = color.Value;
    }

    public override string ToString()
    {
        return Content;
    }
}