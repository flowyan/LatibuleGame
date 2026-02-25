using System;
using NAudio.Wave;
using OpenTK.Mathematics;

namespace Latibule.Core.Audio;

public sealed unsafe class SteamAudioSampleProvider(ISampleProvider stereoSource, Vector3 initialPos, float volume = 1f)
    : ISampleProvider, IDisposable
{
    private readonly WaveFormat _format = WaveFormat.CreateIeeeFloatWaveFormat(SteamAudio.SamplingRate, 2);
    private readonly SteamAudio.Voice _voice = SteamAudio.CreateVoice();

    private Vector3 _sourcePos = initialPos;
    private float _volume = volume;

    // Input: interleaved stereo frame (L,R,L,R...) length = frames*2
    private readonly float[] _stereoInInterleaved = new float[SteamAudio.AudioFrameSize * 2];

    // Temp mono frame (length = frames)
    private readonly float[] _monoFrame = new float[SteamAudio.AudioFrameSize];

    // Output: interleaved stereo frame (L,R,L,R...) length = frames*2
    private readonly float[] _stereoOutInterleaved = new float[SteamAudio.AudioFrameSize * 2];

    private int _stereoIndex;
    private int _stereoValid;

    // Output format is stereo float at SteamAudio sampling rate

    public WaveFormat WaveFormat => _format;

    public int Read(float[] buffer, int offset, int count)
    {
        int written = 0;

        while (written < count)
        {
            if (_stereoIndex >= _stereoValid)
            {
                if (!ProcessNextFrame())
                    break;
            }

            int available = _stereoValid - _stereoIndex;
            int toCopy = Math.Min(available, count - written);

            Array.Copy(_stereoOutInterleaved, _stereoIndex, buffer, offset + written, toCopy);

            _stereoIndex += toCopy;
            written += toCopy;
        }

        return written;
    }

    private bool ProcessNextFrame()
    {
        // Read interleaved stereo floats
        int got = 0;
        while (got < _stereoInInterleaved.Length)
        {
            int n = stereoSource.Read(_stereoInInterleaved, got, _stereoInInterleaved.Length - got);
            if (n == 0) break;
            got += n;
        }

        if (got == 0)
            return false;

        // Pad remainder
        for (int i = got; i < _stereoInInterleaved.Length; i++)
            _stereoInInterleaved[i] = 0f;

        // Apply volume (on stereo input before downmix)
        if (Math.Abs(_volume - 1f) > 1e-6f)
        {
            for (int i = 0; i < _stereoInInterleaved.Length; i++)
                _stereoInInterleaved[i] *= _volume;
        }

        // Downmix stereo -> mono for binaural point-source
        // gotFrames = how many *stereo frames* we actually got (each frame = 2 floats)
        int gotFrames = got / 2;

        for (int i = 0; i < SteamAudio.AudioFrameSize; i++)
        {
            if (i < gotFrames)
            {
                float l = _stereoInInterleaved[i * 2 + 0];
                float r = _stereoInInterleaved[i * 2 + 1];
                _monoFrame[i] = 0.5f * (l + r);
            }
            else
            {
                _monoFrame[i] = 0f;
            }
        }

        fixed (float* monoPtr = _monoFrame)
        fixed (float* stereoPtr = _stereoOutInterleaved)
        {
            SteamAudio.ProcessFrame(_voice, monoPtr, _sourcePos, stereoPtr);
        }

        _stereoIndex = 0;

        // Output only as many stereo floats as we had real input frames
        // (each frame = 2 floats)
        _stereoValid = Math.Min(_stereoOutInterleaved.Length, gotFrames * 2);

        return true;
    }

    public void Dispose() => _voice.Dispose();
}