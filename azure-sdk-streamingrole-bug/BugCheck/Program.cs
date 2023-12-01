using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace BugCheck;

internal class Program
{
    static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
        var key = config["Key"];

        var client = new OpenAIClient(key!);

        var getWeatherFuntionDefinition = new FunctionDefinition()
        {
            Name = "get_current_weather",
            Description = "Get the current weather in a given location",
            Parameters = BinaryData.FromObjectAsJson(
    new
    {
        Type = "object",
        Properties = new
        {
            Location = new
            {
                Type = "string",
                Description = "The city and state, e.g. San Francisco, CA",
            },
            Unit = new
            {
                Type = "string",
                Enum = new[] { "celsius", "fahrenheit" },
            }
        },
        Required = new[] { "location" },
    },
    new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
        };

        #region Snippet:ChatFunctions:RequestWithFunctions
        var conversationMessages = new List<ChatMessage>()
            {
                new(ChatRole.User, "What is the weather like in Boston?"),
            };

        var chatCompletionsOptions = new ChatCompletionsOptions()
        {
            DeploymentName = "gpt-3.5-turbo",
        };
        foreach (ChatMessage chatMessage in conversationMessages)
        {
            chatCompletionsOptions.Messages.Add(chatMessage);
        }
        chatCompletionsOptions.Functions.Add(getWeatherFuntionDefinition);

        string? functionName = null;
        StringBuilder contentBuilder = new();
        StringBuilder functionArgumentsBuilder = new();
        ChatRole streamedRole = default;
        CompletionsFinishReason finishReason = default;

        await foreach (StreamingChatCompletionsUpdate update
            in client.GetChatCompletionsStreaming(chatCompletionsOptions))
        {
            contentBuilder.Append(update.ContentUpdate);
            functionName ??= update.FunctionName;
            functionArgumentsBuilder.Append(update.FunctionArgumentsUpdate);
            streamedRole = update.Role ?? default;
            finishReason = update.FinishReason ?? default;
        }

        if (finishReason == CompletionsFinishReason.FunctionCall)
        {
            string lastContent = contentBuilder.ToString();
            string unvalidatedArguments = functionArgumentsBuilder.ToString();
            ChatMessage chatMessageForHistory = new(streamedRole, lastContent)
            {
                FunctionCall = new(functionName, unvalidatedArguments),
            };
            conversationMessages.Add(chatMessageForHistory);
            chatCompletionsOptions.Messages.Add(chatMessageForHistory);
            // Handle from here just like the non-streaming case
        }
        #endregion

#region BUGCHECK
        await foreach (StreamingChatCompletionsUpdate update
            in client.GetChatCompletionsStreaming(chatCompletionsOptions))
        {
            contentBuilder.Append(update.ContentUpdate);
            functionName ??= update.FunctionName;
            functionArgumentsBuilder.Append(update.FunctionArgumentsUpdate);
            streamedRole = update.Role ?? default;
            finishReason = update.FinishReason ?? default;
        }

        if (finishReason == CompletionsFinishReason.FunctionCall)
        {
            string lastContent = contentBuilder.ToString();
            string unvalidatedArguments = functionArgumentsBuilder.ToString();
            ChatMessage chatMessageForHistory = new(streamedRole, lastContent)
            {
                FunctionCall = new(functionName, unvalidatedArguments),
            };
            conversationMessages.Add(chatMessageForHistory);

            // Handle from here just like the non-streaming case
        }
    }
#endregion
}
