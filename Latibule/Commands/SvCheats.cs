using System.Diagnostics;
using Latibule.Core;
using Latibule.Models;
using OpenTK.Mathematics;
using Vector4 = System.Numerics.Vector4;

namespace Latibule.Commands;

public class SvCheats : ICommand
{
    public string Name { get; } = "sv_cheats";
    public List<string> Aliases { get; } = [];
    public string Usage { get; } = "sv_cheats 1 or 0";

    public async Task Execute(string[] args)
    {
        if (args.Length == 1)
        {
            DevConsole.ErrorLog("sv_cheats: missing argument");
            await Task.CompletedTask;
        }

        var skull = @"
                                .,od88888888888bo,.
                            .d88888888888888888888888b.
                         .d88888888888888888888888888888b.
                       .d888888888888888888888888888888888b.
                     .d8888888888888888888888888888888888888b.
                    d88888888888888888888888888888888888888888b
                   d8888888888888888888888888888888888888888888b
                  d888888888888888888888888888888888888888888888
                  8888888888888888888888888888888888888888888888
                  8888888888888888888888888888888888888888888888
                  8888888888888888888888888888888888888888888888
                  Y88888888888888888888888888888888888888888888P
                  ""8888888888P'   ""Y8888888888P""    ""Y888888888""
                   88888888P        Y88888888P        Y88888888
                   Y8888888          ]888888P          8888888P
                    Y888888          d888888b          888888P
                     Y88888b        d88888888b        d88888P
                      Y888888b.   .d88888888888b.   .d888888
                       Y8888888888888888P Y8888888888888888
                        888888888888888P   Y88888888888888
                        ""8888888888888[     ]888888888888""
                           ""Y888888888888888888888888P""
                                ""Y88888888888888P""
                             888b  Y8888888888P  d888
                             ""888b              d888""
                              Y888bo.        .od888P
                               Y888888888888888888P
                                ""Y88888888888888P""
                                  ""Y8888888888P""
          d8888bo.                  ""Y888888P""                  .od888b
         888888888bo.                  """"""""                  .od8888888
         ""88888888888b.                                   .od888888888[
         d8888888888888bo.                              .od888888888888
       d88888888888888888888bo.                     .od8888888888888888b
       ]888888888888888888888888bo.            .od8888888888888888888888b=
       888888888P"" ""Y888888888888888bo.     .od88888888888888P"" ""Y888888P=
        Y8888P""           ""Y888888888888bd888888888888P""            ""Y8P
          """"                   ""Y8888888888888888P""
                                 .od8888888888bo.
                             .od888888888888888888bo.
                         .od8888888888P""  ""Y8888888888bo.
                      .od8888888888P""        ""Y8888888888bo.
                  .od88888888888P""              ""Y88888888888bo.
        .od888888888888888888P""                    ""Y8888888888888888bo.
       Y8888888888888888888P""                         ""Y8888888888888888b=
       888888888888888888P""                            ""Y8888888888888888=
        ""Y888888888888888                               ""Y88888888888888P=
             """"Y8888888P                                  ""Y888888P""
                ""Y8888P                                     Y888P""
                   """"                                        """"""";

        // DevConsole.DevConsoleOpen = false;
        // Petrichor.Player.CanPause = false;

        await Task.Delay(500);
        // GameNode.SoundManager.CreateSound("res://assets/sounds/scaryglitch.mp3", false, 0, true);

        // GameNode.Player.Camera.Environment = new Environment
        // {
        //     BackgroundMode = Environment.BGMode.Sky,
        //     Sky = new Sky
        //     {
        //         SkyMaterial = new ProceduralSkyMaterial
        //         {
        //             SkyTopColor = new Color(1, 0, 0),
        //             SkyHorizonColor = new Color(1, 0, 0),
        //             GroundBottomColor = new Color(1, 0, 0),
        //             GroundHorizonColor = new Color(1, 0, 0),
        //         }
        //     }
        // };

        foreach (var gameWorldObject in LatibuleGame.GameWorld.Objects)
        {
            // gameWorldObject.Get<ShaderComponent>().Shader = AssetManager.GetTexture(TextureAsset.missing);
        }

        // AssetManager.PlaySound(SoundAsset.missing, randomPitch: false);

        // slowly writes the skull to the console
        foreach (var line in skull.Split("\n"))
        {
            LatibuleGame.Player.LookEnabled = false;
            GameStates.CurrentGui = new DevConsole();
            DevConsole.Log(new ConsoleMessage(line, ConsoleMessageType.CommandOutput, new Vector4(1, 0, 0, 1)));
            LatibuleGame.Player.Camera.View *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(5f));
            await Task.Delay(50);
        }

        // GameNode.SoundManager.CreateSound("res://assets/sounds/knocking.mp3", false, 0, true);

        DevConsole.CommandLog("fuck you");
        await Task.Delay(100);

        var username = System.Environment.UserName;
        var command =
            "$ErrorActionPreference = \\\"Stop\\\"; " +
            $"$notificationTitle = \\\"i am inside your house {username}\\\"; " +
            "[Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] > $null; " +
            "$template = [Windows.UI.Notifications.ToastNotificationManager]::GetTemplateContent([Windows.UI.Notifications.ToastTemplateType]::ToastText01); " +
            "$toastXml = [xml]$template.GetXml(); " +
            "$toastXml.GetElementsByTagName(\\\"text\\\")[0].AppendChild($toastXml.CreateTextNode($notificationTitle)) > $null; " +
            "$xml = New-Object Windows.Data.Xml.Dom.XmlDocument; " +
            "$xml.LoadXml($toastXml.OuterXml); " +
            "$toast = [Windows.UI.Notifications.ToastNotification]::new($xml); " +
            "$toast.Tag = \\\"Latibule\\\"; " +
            "$toast.Group = \\\"Latibule\\\"; " +
            "$toast.ExpirationTime = [DateTimeOffset]::Now.AddSeconds(5); " +
            "$notifier = [Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier(\\\"Latibule\\\"); " +
            "$notifier.Show($toast);";

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-Command \"{command}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true
        };

        using var process = new Process();
        process.StartInfo = processStartInfo;
        process.Start();

        Environment.Exit(666);
    }
}