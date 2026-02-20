using System.Runtime.InteropServices;
using SteamAudio;

namespace Latibule.Core.Audio;

public static class SteamAudioBufferAccess
{
    // Steam Audio buffers are deinterleaved.
    // buffer.Data points to an array of pointers: float* channelPtr[numChannels]
    public static unsafe Span<float> GetChannelSpan(ref IPL.AudioBuffer buffer, int channel)
    {
        if (channel < 0 || channel >= buffer.NumChannels)
            throw new ArgumentOutOfRangeException(nameof(channel));

        // Read the channel pointer from the pointer array.
        IntPtr channelPtr = Marshal.ReadIntPtr(buffer.Data, channel * IntPtr.Size);

        return new Span<float>((void*)channelPtr, buffer.NumSamples);
    }
}