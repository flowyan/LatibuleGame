using Engine;
using Engine.Core;
using Engine.Core.ImGuiNet;
using Engine.Data;
using ImGuiNET;
using Latibule.Entities;
using Latibule.Services;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using static Engine.Core.Logger;
using Metadata = Latibule.Core.Metadata;

namespace Latibule;

public class LatibuleGame : EngineWindow
{
    public static Player Player { get; internal set; }

    public LatibuleGame(NativeWindowSettings nativeWindowSettings) : base(
        new GameWindowSettings
        {
            UpdateFrequency = GameOptions.TargetFPS,
            Win32SuspendTimerOnDrag = true, // Turning this off gives the player physics a bunch of issues when dragging the window
        },
        nativeWindowSettings
    )
    {
        LogInfo($"Initializing {Metadata.GAME_NAME} version: {Metadata.GAME_VERSION}");
        GameStateManager.Initialize(this);
        CenterWindow();
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        // Load the essential assets
        CursorState = CursorState.Grabbed;
        LatibuleEngine.Map = TestingMap.Create();
        LatibuleEngine.Map.OnLoad();

        // Asseteer.PlaySteamAudioSound(SoundAsset.scarletfire, new Vector3(0, 1, -7.5f), 0.75f);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        if (!Asseteer.Loaded) return;
        base.OnUpdateFrame(args);

        GameStateManager.Update(this);
        LatibuleEngine.Map.OnUpdateFrame(args);

        // Core.SteamAudio.SetListenerPosition(Player.Transform.Position, Player.Camera.Direction, Vector3Direction.Up);
    }

    protected override void OnRenderFrameAfterQueue(FrameEventArgs args)
    {
        base.OnRenderFrameAfterQueue(args);
        RenderGui(args);
    }

    private static void RenderGui(FrameEventArgs args)
    {
        if (GameStates.CurrentGui is null) return;

        ImguiImplOpenGL3.NewFrame();
        ImguiImplOpenTK4.NewFrame();
        ImGui.NewFrame();

        GameStates.CurrentGui.OnRenderFrame(args);

        ImGui.Render();
        ImguiImplOpenGL3.RenderDrawData(ImGui.GetDrawData());
    }
}