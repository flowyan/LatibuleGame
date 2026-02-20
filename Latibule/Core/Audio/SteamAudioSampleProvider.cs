using System;
using NAudio.Wave;
using OpenTK.Mathematics;

namespace Latibule.Core.Audio;

public sealed unsafe class SteamAudioSampleProvider : ISampleProvider, IDisposable
{
    private readonly ISampleProvider _monoSource;
    private readonly WaveFormat _format;
    private readonly SteamAudio.Voice _voice;

    private Vector3 _sourcePos;
    private float _volume;

    private readonly float[] _monoFrame;          // size = frame
    private readonly float[] _stereoInterleaved;  // size = frame*2

    private int _stereoIndex;
    private int _stereoValid;

    public SteamAudioSampleProvider(ISampleProvider monoSource, Vector3 initialPos, float volume = 1f)
    {
        if (monoSource.WaveFormat.SampleRate != SteamAudio.SamplingRate)
            throw new ArgumentException($"Expected {SteamAudio.SamplingRate} Hz, got {monoSource.WaveFormat.SampleRate} Hz");
        if (monoSource.WaveFormat.Channels != 1)
            throw new ArgumentException("SteamAudioSpatializerSampleProvider requires MONO input.");

        _monoSource = monoSource;
        _sourcePos = initialPos;
        _volume = volume;

        _format = WaveFormat.CreateIeeeFloatWaveFormat(SteamAudio.SamplingRate, 2);

        _voice = SteamAudio.CreateVoice();
        _monoFrame = new float[SteamAudio.AudioFrameSize];
        _stereoInterleaved = new float[SteamAudio.AudioFrameSize * 2];
    }

    public WaveFormat WaveFormat => _format;

    public void SetSourcePosition(Vector3 pos) => _sourcePos = pos;
    public void SetVolume(float volume) => _volume = volume;

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

            Array.Copy(_stereoInterleaved, _stereoIndex, buffer, offset + written, toCopy);

            _stereoIndex += toCopy;
            written += toCopy;
        }

        return written;
    }

    private bool ProcessNextFrame()
    {
        int got = 0;
        while (got < _monoFrame.Length)
        {
            int n = _monoSource.Read(_monoFrame, got, _monoFrame.Length - got);
            if (n == 0) break;
            got += n;
        }

        if (got == 0)
            return false;

        // pad remainder
        for (int i = got; i < _monoFrame.Length; i++)
            _monoFrame[i] = 0f;

        // apply volume
        for (int i = 0; i < _monoFrame.Length; i++)
            _monoFrame[i] *= _volume;

        fixed (float* monoPtr = _monoFrame)
        fixed (float* stereoPtr = _stereoInterleaved)
        {
            SteamAudio.ProcessFrame(_voice, monoPtr, _sourcePos, stereoPtr);
        }

        _stereoIndex = 0;
        _stereoValid = Math.Min(_stereoInterleaved.Length, got * 2); // only output "real" samples
        return true;
    }

    public void Dispose() => _voice.Dispose();
}