using System.Numerics;
using System.Reflection;
using ImGuiNET;
using Latibule.Core;
using Latibule.Core.ImGuiNet;
using Latibule.Models;
using Microsoft.Xna.Framework.Input;

namespace Latibule.Services;

public static class DevConsoleService
{
    public static void Initialize()
    {
        // Check for a developer key
        var keyBase64 = Convert.FromBase64String("aWFtdGhlb25ld2hva25vY2tz");
        var key = System.Text.Encoding.UTF8.GetString(keyBase64);
        if (File.Exists($"key.txt")) GameStates.HasDeveloperKey = File.ReadAllText("key.txt") == key;
        if (Environment.GetEnvironmentVariable("DEV_KEY") != null) GameStates.HasDeveloperKey = Environment.GetEnvironmentVariable("DEV_KEY") == key;
        if (GameStates.HasDeveloperKey)
            DevConsole.Log(new ConsoleMessage("Developer key found. Developer tools granted.", ConsoleMessageType.Info,
                new Vector4(0, 1, 0, 1)));

        // Assign commands for DevConsole
        DevConsole.ConsoleCommands = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.GetInterfaces().Contains(typeof(ICommand)))
            .Select(t => Activator.CreateInstance(t) as ICommand)
            .ToList();

        Logger.LogInfo($"Loaded {DevConsole.ConsoleCommands.Count} console commands.");
    }
}