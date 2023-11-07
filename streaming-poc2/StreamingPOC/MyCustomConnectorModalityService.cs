#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
class MyCustomConnectorModalityService : IConnectorModalityService<string, Image, ImageBit>
{
    public IReadOnlyCollection<string> InputTypes => new[] { "text/plain" };

    public IReadOnlyCollection<string> OutputTypes => new[] { "image/png" };

    public async IAsyncEnumerable<byte[]> GetByteStreamingResultAsync(string prompt)
    {
        //Conversion here can be from byte[] to string or from string to byte[] depending on the underlying API support.
        await foreach (var base64bit in GetStringStreamingResultAsync(prompt))
        {
            yield return Convert.FromBase64String(base64bit);
        }
    }

    public async IAsyncEnumerable<string> GetStringStreamingResultAsync(string prompt)
    {
        //Conversion here can be from byte[] to string or from string to byte[] depending on the underlying API support.
        await foreach(var imageBit in this.GetStreamingResultAsync(prompt))
        {
            yield return imageBit.Base64Content;
        }
    }

    public IAsyncEnumerable<ImageBit> GetStreamingResultAsync(string prompt)
    {
        var itens = new[]
        {
            new ImageBit { Base64Content = "Rmlyc3QgQ2h1bms=" }, // First Chunk
            new ImageBit { Base64Content = "U2Vjb25kIENodW5r" }, // Second Chunk
            new ImageBit { Base64Content = "VGhpcmQgQ2h1bms=" } // Third Chunk
        };

        return itens.ToAsyncEnumerable();
    }

    public Task<Image> GetResultAsync(string prompt)
    {
        return Task.FromResult(new Image { Base64Content = "RnVsbCBDb250ZW50" });
    }

    public async Task<string> GetStringResultAsync(string prompt)
    {
        return (await GetResultAsync(prompt)).Base64Content;
    }

    public async Task<byte[]> GetByteResultAsync(string prompt)
    {
        var image = await GetResultAsync(prompt);
        return Convert.FromBase64String(image.Base64Content);
    }
}

record Image
{
    public string Base64Content { set; get; }
}

record ImageBit
{
    public string Base64Content { set; get; }
}
