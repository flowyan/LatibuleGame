using Engine.Core;
using Engine.Data;
using Engine.Data.Sound;
using SDL3;

namespace Engine.Audio;

public static class AudioEngine
{
    private static SDL.AudioFormat NativeAudioF32 => BitConverter.IsLittleEndian
        ? SDL.AudioFormat.AudioF32LE
        : SDL.AudioFormat.AudioF32BE;

    public static UIntPtr AudioDevice;
    public static IntPtr MixerDevice;

    public static SDL.AudioSpec DefaultAudioSpec = new()
    {
        Channels = 2,
        Format = NativeAudioF32,
        Freq = 44100
    };

    public static void Initialize()
    {
        if (!Mixer.Init())
            throw new InvalidOperationException($"Mixer init failed: {SDL.GetError()}");

        MixerDevice = Mixer.CreateMixerDevice(SDL.AudioDeviceDefaultPlayback, IntPtr.Zero);
        AudioDevice = SDL.OpenAudioDevice(SDL.AudioDeviceDefaultPlayback, DefaultAudioSpec);
        if (AudioDevice == UIntPtr.Zero) throw new InvalidOperationException($"Could not open SDL audio device: {SDL.GetError()}");
        if (MixerDevice == IntPtr.Zero) throw new InvalidOperationException($"Could not open Mixer audio device: {SDL.GetError()}");
    }

    public static void PlayAudio(IntPtr audio)
    {
        var track = Mixer.CreateTrack(MixerDevice);
        if (track == IntPtr.Zero || !Mixer.SetTrackAudio(track, audio))
            throw new InvalidOperationException($"Could not set audio track: {SDL.GetError()}");

        if (!Mixer.PlayTrack(track, 0)) Console.WriteLine($"Play failed: {SDL.GetError()}");
    }

    public static void Update()
    {
        // if (Input.IsKeyPressed(SDL.Scancode.K)) Asseteer.PlaySound(EngineSounds.Dev.pingas);
        // if (Input.IsKeyPressed(SDL.Scancode.L)) Asseteer.PlaySound(EngineSounds.Dev.tada);
    }

    public static void Dispose()
    {
        Mixer.DestroyMixer(MixerDevice);
        Mixer.Quit();
    }
}