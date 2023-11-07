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
