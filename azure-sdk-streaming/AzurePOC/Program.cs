using Azure.AI.OpenAI;

internal class Program
{
    private static async Task Main(string[] args)
    {
        string nonAzureOpenAIApiKey = "sk-***";
        var client = new OpenAIClient(nonAzureOpenAIApiKey, new OpenAIClientOptions());

        var options = new CompletionsOptions()
        {
            ChoicesPerPrompt = 3,
            GenerationSampleCount = 3,
            MaxTokens = 150,
            DeploymentName = "text-davinci-003",
            Prompts = { "Write me a phrase about morphology" }
        };

        await foreach (var completions in client.GetCompletionsStreaming(options)) 
        {
            Console.WriteLine(completions.Choices[0].Text);
            Console.WriteLine(completions.Choices[1].Text);
            Console.WriteLine(completions.Choices[2].Text);
        };
    }
}