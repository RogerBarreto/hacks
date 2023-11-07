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

    public abstract object BuildRaw();
}

abstract class StreamingResultBuilder<TComplete, TBit> : StreamingResultBuilder
{
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
    public SemanticFunction(SemanticFunctionConfig config)
    {
        Config = config;
    }

    public SemanticFunctionConfig Config { get; }

    public Task<StreamingFunctionResult> StreamingInvokeAsync(SKContext context)
    {
        var service = context.ServiceSelector.SelectAIServiceByModality(this.Config.InputMimetype, this.Config.OutputMimetype);

        var builder = service.GetStreamingResultBuilder();

        var result = new StreamingFunctionResult(builder, service.OutputMimetype, service.GetStreamingResult(context));

        return Task.FromResult(result);
    }
}

record SemanticFunctionConfig(string InputMimetype = "text/plain", string OutputMimetype = "text/plain");

interface IKernel
{
    IAsyncEnumerable<KernelResultBit> StreamingRunAsync(ContextVariables variables, ISKFunction[] pipeline);
}

class Kernel : IKernel
{
    public async IAsyncEnumerable<KernelResultBit> StreamingRunAsync(ContextVariables variables, ISKFunction[] pipeline)
    {
        var context = new SKContext(variables);

        foreach(var function in pipeline)
        {
            var result = await function.StreamingInvokeAsync(context);
            var builder = result.Builder;
            await foreach(var bit in result.StreamingResult)
            {
                builder.Append(bit);
                yield return new(bit, builder);
            }
            var completeResult = builder.BuildRaw();

            context.Variables["input"] = completeResult;
        }
    }
}

record KernelResultBit
{
    public string Mimetype => Result.Mimetype;
    public StreamingResultBit Result { get; }
    public StreamingResultBuilder Builder { get; }
    
    public KernelResultBit(StreamingResultBit result, StreamingResultBuilder builder)
    {
        Result = result;
        Builder = builder;
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

    public override object BuildRaw()
    {
        return Build();
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

    public override bool Append(StreamingResultBit bit)
    {
        return Append(bit);
    }
}

class SpecificTextToImageConnectorModalityService : IConnectorModalityService<string, ImageBit>
{
    public IReadOnlyList<string> InputMimetypes { get; } = new[] { "text/plain" };
    public string OutputMimetype { get; }

    public IAsyncEnumerable<StreamingResultBit> GetStreamingResult(object input)
    {
        return GetStreamingResult(input);
    }

    public async IAsyncEnumerable<StreamingResultBit<ImageBit>> GetStreamingResultAsync(string input)
    {
        yield return new StreamingResultBit<ImageBit>("image/png", new ImageBit { Content = "1" });
        yield return new StreamingResultBit<ImageBit>("image/png", new ImageBit { Content = "2" });
        yield return new StreamingResultBit<ImageBit>("image/png", new ImageBit { Content = "3" });
    }

    public StreamingResultBuilder GetStreamingResultBuilder()
    {
        return new ImageBuilder();
    }
}

interface IConnectorModalityService<TInput, TOutput> : IConnectorModalityService
{
    IAsyncEnumerable<StreamingResultBit<TOutput>> GetStreamingResultAsync(TInput input);
}