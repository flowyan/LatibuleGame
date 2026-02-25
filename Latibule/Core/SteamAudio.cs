using System;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using SteamAudio;

namespace Latibule.Core;

public static unsafe class SteamAudio
{
    public const int SamplingRate = 44100;
    public const int AudioFrameSize = 512;

    private static IPL.Context iplContext;
    private static IPL.Hrtf iplHrtf;
    private static IPL.AudioSettings iplAudioSettings;

    private static IPL.DistanceAttenuationModel _distanceModel;

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

        _distanceModel = new IPL.DistanceAttenuationModel
        {
            Type = IPL.DistanceAttenuationModelType.Default,
        };

        Logger.LogInfo("SteamAudio is ready!");
    }

    public static void UnloadSteamAudio()
    {
        IPL.HrtfRelease(ref iplHrtf);
        IPL.ContextRelease(ref iplContext);
    }

    public sealed class Voice : IDisposable
    {
        public IPL.DirectEffect Direct; // NEW
        public IPL.BinauralEffect Effect;
        public IPL.AudioBuffer Input; // mono
        public IPL.AudioBuffer Output; // stereo

        public Voice()
        {
            // Direct effect (mono->mono)
            var directSettings = new IPL.DirectEffectSettings { NumChannels = 1 };
            IPL.DirectEffectCreate(iplContext, in iplAudioSettings, in directSettings, out Direct);

            // Binaural effect (mono->stereo)
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
            IPL.DirectEffectRelease(ref Direct); // NEW
        }
    }

    public static Voice CreateVoice() => new();

    public static void ProcessFrame(
        Voice voice,
        float* monoIn,
        Vector3 sourceWorldPos,
        float* interleavedStereoOut)
    {
        // Copy mono input (channel 0)
        float* inputCh0 = ((float**)voice.Input.Data)[0];

        for (int i = 0; i < AudioFrameSize; i++)
            inputCh0[i] = monoIn[i];

        // --- Distance attenuation (Steam Audio) ---
        float distanceAttenuation = IPL.DistanceAttenuationCalculate(
            iplContext,
            new IPL.Vector3(sourceWorldPos.X, sourceWorldPos.Y, sourceWorldPos.Z),
            new IPL.Vector3(_listenerPos.X, _listenerPos.Y, _listenerPos.Z),
            _distanceModel);

        // Apply DirectEffect (mono -> mono) in-place
        var directParams = new IPL.DirectEffectParams
        {
            Flags = IPL.DirectEffectFlags.ApplyDistanceAttenuation,
            DistanceAttenuation = distanceAttenuation
        };

        IPL.DirectEffectApply(voice.Direct, ref directParams, ref voice.Input, ref voice.Input);

        // Compute direction to listener
        var dirWorld = sourceWorldPos - _listenerPos;

        if (dirWorld.LengthSquared < 1e-8f)
            dirWorld = _listenerForward;
        else
            dirWorld = Vector3.Normalize(dirWorld);

        // Build orthonormal listener basis
        var f = _listenerForward;
        var u = _listenerUp;

        // Make up orthogonal to forward
        u = Vector3.Normalize(u - f * Vector3.Dot(u, f));

        var r = Vector3.Normalize(Vector3.Cross(f, u));
        u = Vector3.Normalize(Vector3.Cross(r, f));

        // Convert to listener-local space
        var dirLocal = new Vector3(
            Vector3.Dot(dirWorld, r), // right
            Vector3.Dot(dirWorld, u), // up
            Vector3.Dot(dirWorld, f) // forward
        );

        if (dirLocal.LengthSquared < 1e-8f)
            dirLocal = new Vector3(0, 0, 1);
        else
            dirLocal = Vector3.Normalize(dirLocal);

        // Apply binaural (always stereo)
        var p = new IPL.BinauralEffectParams
        {
            Hrtf = iplHrtf,
            Direction = new IPL.Vector3(dirLocal.X, dirLocal.Y, dirLocal.Z),
            Interpolation = IPL.HrtfInterpolation.Nearest,
            SpatialBlend = 1f,
            PeakDelays = IntPtr.Zero
        };

        IPL.BinauralEffectApply(
            voice.Effect,
            ref p,
            ref voice.Input,
            ref voice.Output);

        // Deinterleaved -> Interleaved stereo
        IPL.AudioBufferInterleave(
            iplContext,
            in voice.Output,
            in Unsafe.AsRef<float>(interleavedStereoOut));
    }
}