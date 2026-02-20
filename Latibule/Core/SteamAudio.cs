using System;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using SteamAudio;

namespace Latibule.Core;

public static unsafe class SteamAudio
{
    public const int SamplingRate = 44100;
    public const int AudioFrameSize = 4096;

    private static IPL.Context iplContext;
    private static IPL.Hrtf iplHrtf;
    private static IPL.AudioSettings iplAudioSettings;

    // listener state (simple; if you want orientation later we can add it)
    private static Vector3 _listenerPos;
    private static Vector3 _listenerForward;
    private static Vector3 _listenerUp;

    public static void SetListenerPosition(Vector3 pos, Vector3 forward, Vector3 up)
    {
        _listenerPos = pos;
        _listenerForward = forward;
        _listenerUp = up;
    }

    public static void PrepareSteamAudio()
    {
        var contextSettings = new IPL.ContextSettings { Version = IPL.Version };
        IPL.ContextCreate(in contextSettings, out iplContext);

        iplAudioSettings = new IPL.AudioSettings { SamplingRate = SamplingRate, FrameSize = AudioFrameSize };

        var hrtfSettings = new IPL.HrtfSettings
        {
            Type = IPL.HrtfType.Default,
            Volume = 1f,
            NormType = IPL.HrtfNormType.None
        };

        IPL.HrtfCreate(iplContext, in iplAudioSettings, in hrtfSettings, out iplHrtf);

        Console.WriteLine("SteamAudio is ready.");
    }

    public static void UnloadSteamAudio()
    {
        IPL.HrtfRelease(ref iplHrtf);
        IPL.ContextRelease(ref iplContext);
    }

    public sealed class Voice : IDisposable
    {
        public IPL.BinauralEffect Effect;
        public IPL.AudioBuffer Input; // mono
        public IPL.AudioBuffer Output; // stereo

        public Voice()
        {
            var binauralEffectSettings = new IPL.BinauralEffectSettings { Hrtf = iplHrtf };
            IPL.BinauralEffectCreate(iplContext, in iplAudioSettings, in binauralEffectSettings, out Effect);

            IPL.AudioBufferAllocate(iplContext, 1, iplAudioSettings.FrameSize, ref Input);
            IPL.AudioBufferAllocate(iplContext, 2, iplAudioSettings.FrameSize, ref Output);
        }

        public void Dispose()
        {
            IPL.AudioBufferFree(iplContext, ref Input);
            IPL.AudioBufferFree(iplContext, ref Output);
            IPL.BinauralEffectRelease(ref Effect);
        }
    }

    public static Voice CreateVoice() => new Voice();

    // Writes interleaved stereo float samples (L,R,L,R...) into dest (length = AudioFrameSize*2)
    public static void ProcessFrame(Voice voice, float* monoIn, Vector3 sourceWorldPos, float* interleavedStereoOut)
    {
        // copy mono into steam input channel 0
        float* inputCh0 = ((float**)voice.Input.Data)[0];
        for (int i = 0; i < AudioFrameSize; i++)
            inputCh0[i] = monoIn[i];

        var dirWorld = sourceWorldPos - _listenerPos;
        if (dirWorld.LengthSquared < 1e-8f) dirWorld = _listenerForward;
        else dirWorld = Vector3.Normalize(dirWorld);

// Listener basis
        var f = _listenerForward;
        var u = _listenerUp;

// Make u orthogonal to f (important if you pass UnitY always)
        u = Vector3.Normalize(u - f * Vector3.Dot(u, f));

        var r = Vector3.Normalize(Vector3.Cross(f, u)); // right
// recompute u to be perfectly orthonormal
        u = Vector3.Normalize(Vector3.Cross(r, f));

// Convert world direction into listener-local coordinates
        var dirLocal = new Vector3(
            Vector3.Dot(dirWorld, r),
            Vector3.Dot(dirWorld, u),
            Vector3.Dot(dirWorld, f)
        );

        if (dirLocal.LengthSquared < 1e-8f) dirLocal = new Vector3(0, 0, 1);
        else dirLocal = Vector3.Normalize(dirLocal);

        var p = new IPL.BinauralEffectParams
        {
            Hrtf = iplHrtf,
            Direction = new IPL.Vector3(dirLocal.X, dirLocal.Y, dirLocal.Z),
            Interpolation = IPL.HrtfInterpolation.Nearest,
            SpatialBlend = 1f,
            PeakDelays = IntPtr.Zero
        };

        IPL.BinauralEffectApply(voice.Effect, ref p, ref voice.Input, ref voice.Output);

        // deinterleaved -> interleaved
        IPL.AudioBufferInterleave(iplContext, in voice.Output, in Unsafe.AsRef<float>(interleavedStereoOut));
    }
}