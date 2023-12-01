using System.Net.Http;
using System.Text;

namespace ConsoleApp1;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        HttpClient client = new HttpClient();
        client.BaseAddress = new Uri("https://api-inference.huggingface.co/models/");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "HuggingFaceH4/zephyr-7b-beta");

        httpRequest.Headers.Add("User-Agent", "Semantic-Kernel");
        httpRequest.Headers.Add("Authorization", $"Bearer xx");

        httpRequest.Content = new StringContent("{\r\n    \"inputs\": \"Question: What is New York?; Answer:\",\r\n    \"stream\": true\r\n}", Encoding.UTF8, "application/json");

        using var result = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseContentRead);
        
        Console.WriteLine("started");
    }
}

