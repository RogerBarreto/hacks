#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using System.Buffers.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

interface IConnectorModalityService<TRequest, TCompleteResponse, TBitResponse> : IConnectorModalityService
{
    IAsyncEnumerable<TBitResponse> GetStreamingResultAsync(TRequest prompt);
    Task<TCompleteResponse> GetResultAsync(TRequest prompt);
}

interface IConnectorModalityService
{
    IReadOnlyCollection<string> InputTypes { get; }
    IReadOnlyCollection<string> OutputTypes { get; }
    IAsyncEnumerable<string> GetStringStreamingResultAsync(string prompt);
    IAsyncEnumerable<byte[]> GetByteStreamingResultAsync(string prompt);
    Task<string> GetStringResultAsync(string prompt);
    Task<byte[]> GetByteResultAsync(string prompt);
}

interface IAIServiceSelector
{
    IConnectorModalityService? SelectAIServiceByModality(string? inputMimetype = "text/plain", string? outputMimetype = "text/plain");
}

interface ISKFunction
{
    IAsyncEnumerable<string> StreamingInvokeAsync(SKContext context);
}

class SemanticFunction : ISKFunction
{
    public SemanticFunction(SemanticConfig config)
    {
        Config = config;
    }

    public SemanticConfig Config { get; }

    public IAsyncEnumerable<string> StreamingInvokeAsync(SKContext context)
    {
        var renderedPrompt = "MyPrompt...";
        var service = context.ServiceSelector.SelectAIServiceByModality(this.Config.InputType, this.Config.OutputType);
        
        return service?.GetStringStreamingResultAsync(renderedPrompt) 
            ?? throw new Exception("No service found");
    }
}

record SemanticConfig(string InputType = "text/plain", string OutputType = "text/plain");

interface IKernel
{
    IAsyncEnumerable<string> StreamingRunAsync(ContextVariables variables, ISKFunction function);
}

partial class Kernel : IKernel
{
    public IAsyncEnumerable<string> StreamingRunAsync(ContextVariables variables, ISKFunction function)
    {
        var context = new SKContext(variables, this._serviceSelector);
        return function.StreamingInvokeAsync(context);
    }
}
