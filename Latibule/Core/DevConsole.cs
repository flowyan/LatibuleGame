using ImGuiNET;
using Latibule.Models;
using OpenTK.Windowing.Common;
using ICommand = Latibule.Models.ICommand;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace Latibule.Core;

public class DevConsole : IGuiScreen
{
    private static List<ConsoleMessage> _messages = [];
    private static List<string> _commandHistory = [];
    public static List<ICommand?>? ConsoleCommands = [];

    private static string _command = "";

    public void Initialize()
    {
    }

    public void OnRenderFrame(FrameEventArgs args)
    {
        if (args.Time <= 0) return;

        ImGui.Begin("Dev Console", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings);

        var sizeX = GameStates.GameWindow.Size.X - 500;
        var sizeY = GameStates.GameWindow.Size.Y - 200;
        var x = ImGui.GetIO().DisplaySize.X / 2 - sizeX / 2;
        var y = ImGui.GetIO().DisplaySize.Y / 2 - sizeY / 2;

        ImGui.SetWindowPos(new Vector2(x, y), ImGuiCond.Appearing);
        ImGui.SetWindowSize(new Vector2(sizeX, sizeY), ImGuiCond.Once);
        ImGui.SetWindowFocus();

        ImGui.BeginChild("##messages", new Vector2(0, -ImGui.GetFrameHeightWithSpacing()));

        var messages = string.Join("\n", _messages);
        var textSize = ImGui.CalcTextSize(messages);
        textSize.X = ImGui.GetWindowWidth();
        textSize.Y += 5;

        ImGui.PushID(0);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0, 0, 0, 0));
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 1, 1));

        ImGui.InputTextMultiline(
            "",
            ref messages,
            (uint)messages.Length + 1,
            textSize,
            ImGuiInputTextFlags.ReadOnly | ImGuiInputTextFlags.NoHorizontalScroll
        );

        ImGui.PopStyleColor();
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
        ImGui.PopID();

        // Autoscroll
        if (ImGui.GetScrollY() >= ImGui.GetScrollMaxY()) ImGui.SetScrollHereY(1);

        ImGui.EndChild();
        if (ImGui.IsWindowAppearing()) ImGui.SetKeyboardFocusHere();
        if (ImGui.IsKeyPressed(ImGuiKey.Enter)) ImGui.SetKeyboardFocusHere();
        if (ImGui.IsKeyPressed(ImGuiKey.GraveAccent)) ImGui.SetKeyboardFocusHere();

        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0, 0, 0, 1));
        // max width
        ImGui.SetNextItemWidth(-1);
        var input = ImGui.InputTextWithHint("##input", "Enter command here", ref _command, 256,
            ImGuiInputTextFlags.EnterReturnsTrue);
        if (input && _command.Length > 0) ExecuteCommand(_command);
        ImGui.PopStyleColor();

        ImGui.End();
    }

    public static void Log(ConsoleMessage message) => _messages.Add(message);

    public static void InfoLog(string message) => Log(new ConsoleMessage(message, ConsoleMessageType.Info));

    public static void ErrorLog(string message) =>
        Log(new ConsoleMessage($"[ERROR] {message}", ConsoleMessageType.Error));

    public static void WarnLog(string message) =>
        Log(new ConsoleMessage($"[WARN] {message}", ConsoleMessageType.Warning));

    public static void CommandLog(string message, Vector4? color = null) =>
        Log(new ConsoleMessage(message, ConsoleMessageType.CommandOutput, color));

    private static void ExecuteCommand(string command)
    {
        try
        {
            command = command.ToLower();
            Logger.LogInfo($"Executing command: {command}", false);
            InfoLog($"] {command}");
            _commandHistory.Add(command);

            // if (!PetrichorEngine.HasDeveloperKey && command.Split(" ")[0] != "sv_cheats")
            // {
            //     Logger.LogError($"Can't use cheat command '{command.Split(" ")[0]}', unless the game has sv_cheats set to 1.");
            //     return;
            // }

            // Support for multiple commands separated by semicolons
            if (command.Contains(';'))
            {
                var commands = command.Split(";");
                foreach (var cmd in commands)
                {
                    var args = cmd.Split(" ");
                    var consoleCommand = ConsoleCommands?.Find(c => c.Name == args[0] || c.Aliases.Contains(args[0]));
                    if (consoleCommand != null) consoleCommand.Execute(args);
                    else Logger.LogError($"Command '{args[0]}' not found");
                }
            }
            else
            {
                var args = command.Split(" ");
                var consoleCommand = ConsoleCommands?.Find(c => c.Name == args[0] || c.Aliases.Contains(args[0]));
                if (consoleCommand != null) consoleCommand.Execute(args);
                else Logger.LogError($"Command '{args[0]}' not found");
            }

            // Clear the command input after execution
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Logger.LogError($"An error occurred while executing command: {e.Message}");
        }

        _command = "";
    }
}