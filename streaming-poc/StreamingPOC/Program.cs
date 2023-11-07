#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

Console.WriteLine("Hello, World!");

interface IConnectorModalityService 
{ 
    IReadOnlyList<string> InputMimetypes { get; }
    string OutputMimetype { get; }

    IAsyncEnumerable<StreamingResultBit> GetStreamingResult(object input);

    StreamingResultBuilder GetStreamingResultBuilder();
}

abstract record StreamingResultBit(string Mimetype, object RawResult);

record StreamingResultBit<TInput> : StreamingResultBit
{
    public StreamingResultBit(string mimetype, TInput value) : base(mimetype, value!)
    {
        Result = value;
    }

    public TInput Result { get; }
}

abstract class StreamingResultBuilder
{
    public string Mimetype { get; }

    public abstract bool Append(StreamingResultBit bit);

    public abstract object Build();
}

abstract class StreamingResultBuilder<TComplete, TBit> 
{
    public string Mimetype { get; }

    public abstract bool Append(TBit bit);

    public abstract TComplete Build();
}

record StreamingFunctionResult
{
    public StreamingResultBuilder Builder { get; }

    public string Mimetype { get; }

    public IAsyncEnumerable<StreamingResultBit> StreamingResult { get; }

    public StreamingFunctionResult(StreamingResultBuilder builder, string mimetype, IAsyncEnumerable<StreamingResultBit> streamingResult)
    {
        Builder = builder;
        Mimetype = mimetype;
        StreamingResult = streamingResult;
    }
}

class SKContext
{
    public SKContext(ContextVariables variables)
    {
        Variables = variables;
    }

    public ContextVariables Variables { get; }
    internal IAIServiceSelector ServiceSelector { get; }
}

interface IAIServiceSelector
{
    IConnectorModalityService? SelectAIServiceByModality(string? inputMimetype = "text/plain", string? outputMimetype = "text/plain");
}

interface ISKFunction
{
    Task<StreamingFunctionResult> StreamingInvokeAsync(SKContext context);
}

class SemanticFunction : ISKFunction
{
    public SemanticFunction(SemanticConfig config)
    {
        Config = config;
    }

    public SemanticConfig Config { get; }

    public Task<StreamingFunctionResult> StreamingInvokeAsync(SKContext context)
    {
        var service = context.ServiceSelector.SelectAIServiceByModality(this.Config.InputMimetype, this.Config.OutputMimetype);

        var builder = service.GetStreamingResultBuilder();

        var result = new StreamingFunctionResult(builder, service.OutputMimetype, service.GetStreamingResult(context));

        return Task.FromResult(result);
    }
}

record SemanticConfig(string InputMimetype = "text/plain", string OutputMimetype = "text/plain");


interface IKernel
{
    IAsyncEnumerable<StreamingResultBit> StreamingRunAsync(ContextVariables variables, ISKFunction[] pipeline);
}

class Kernel : IKernel
{
    public async IAsyncEnumerable<StreamingResultBit> StreamingRunAsync(ContextVariables variables, ISKFunction[] pipeline)
    {
        var context = new SKContext(variables);

        foreach(var function in pipeline)
        {
            var result = await function.StreamingInvokeAsync(new SKContext(variables));
            var builder = result.Builder;
            await foreach(var bit in result.StreamingResult)
            {
                builder.Append(bit);
                yield return bit;
            }
            var finalResult = builder.Build();

            context.Variables["input"] = finalResult;
        }
    }
}

class ContextVariables : Dictionary<string, object>
{

}

class DefaultServiceSelector : IAIServiceSelector
{
    static List<IConnectorModalityService> RegisteredServices { get; } = new();

    static public bool Register(IConnectorModalityService service)
    {
        if (!RegisteredServices.Contains(service))
        {
            RegisteredServices.Add(service);
            return true;
        }

        return false;
    }

    public IConnectorModalityService? SelectAIServiceByModality(string? inputMimetype = "text/plain", string? outputMimetype = "text/plain")
    {
        return RegisteredServices.FirstOrDefault(s => s.InputMimetypes.Contains(inputMimetype) && s.OutputMimetype == outputMimetype);
    }
}

record Image
{
    public string Content { get; set; }
}

record ImageBit
{
    public string Content { set; get; }
}

class ImageBuilder : StreamingResultBuilder<Image, ImageBit>
{
    private List<ImageBit> imageBits = new();

    public override bool Append(ImageBit bit)
    {
        this.imageBits.Add(bit);

        return true;
    }

    public override Image Build()
    {
        var fullImageContent = string.Empty;
        foreach (var bit in  this.imageBits)
        {
            fullImageContent += bit.Content;
        }
        
        return new Image { Content = fullImageContent };
    }
}

class ConnectorModalityService : IConnectorModalityService<string, ImageBit>
{
    public ConnectorModalityService(string[] inputMimetypes, string outputMimetype)
    {
        InputMimetypes = inputMimetypes;
        OutputMimetype = outputMimetype;
    }

    public IReadOnlyList<string> InputMimetypes { get; }
    public string OutputMimetype { get; }

    public IAsyncEnumerable<StreamingResultBit> GetStreamingResult(object input)
    {
        return GetStreamingResult(input);
    }

    public IAsyncEnumerable<StreamingResultBit<ImageBit>> GetStreamingResult(string input)
    {
        throw new NotImplementedException();
    }

    public StreamingResultBuilder GetStreamingResultBuilder()
    {
        throw new NotImplementedException();
    }
}

interface IConnectorModalityService<TInput, TOutput> : IConnectorModalityService
{
    IAsyncEnumerable<StreamingResultBit<TOutput>> GetStreamingResult(TInput input);
}