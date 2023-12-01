using System.Media;
using NAudio;
using NAudio.Wave;

internal class Program
{
    private static void Main(string[] args)
    {
        var audioData = GetAudioDataAsync(); // Your IAsyncEnumerable<byte[]> source
        using (var ms = new MemoryStream())
        {
            await foreach (var byteChunk in audioData)
            {
                ms.Write(byteChunk, 0, byteChunk.Length);
            }

            ms.Seek(0, SeekOrigin.Begin);
            PlayMemoryStream(ms); // Call the playback method
        }
    }

    static void PlayMemoryStream(MemoryStream memoryStream)
    {
        // Assuming the data in the memoryStream is in WAV format
        memoryStream.Position = 0; // Reset the position to the start of the stream

        using (var waveOut = new WaveOutEvent())
        using (var waveProvider = new RawSourceWaveStream(memoryStream, new WaveFormat()))
        {
            waveOut.Init(waveProvider);
            waveOut.Play();

            // This loop is necessary to keep the method alive while the audio is playing
            // You might want to replace this with a more sophisticated method of synchronization
            while (waveOut.PlaybackState == PlaybackState.Playing)
            {
                System.Threading.Thread.Sleep(100);
            }
        }
    }
}

private sealed class MyNativePlugin
{
    [SKFunction]
    public async IAsyncEnumerable<byte[]> PlayMusic(string filePath)
    {
        int bufferSize = 4096;
        byte[] buffer = new byte[bufferSize];

        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true))
        {
            int read;
            while ((read = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                yield return buffer[..read]; // Slice the buffer to the actual number of bytes read
            }
        }
    }
}