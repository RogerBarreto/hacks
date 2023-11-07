#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

class SKContext
{
    public SKContext(ContextVariables variables, IAIServiceSelector serviceSelector)
    {
        Variables = variables;
        ServiceSelector = serviceSelector;
    }

    public ContextVariables Variables { get; }
    internal IAIServiceSelector ServiceSelector { get; }
}

class ContextVariables : Dictionary<string, object>
{

}

partial class Kernel : IKernel
{
    private readonly IAIServiceSelector _serviceSelector;

    public Kernel()
    {
        this._serviceSelector = new DefaultServiceSelector();
    }
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
        return RegisteredServices.FirstOrDefault(s => s.InputTypes.Contains(inputMimetype) && s.OutputTypes.Contains(outputMimetype));
    }
}
