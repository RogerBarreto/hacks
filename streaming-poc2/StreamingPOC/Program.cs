
// Ignore this, it's just shorthand for the rest of the code
var myService = new MyCustomConnectorModalityService();
DefaultServiceSelector.Register(myService);

var function = new SemanticFunction(new SemanticConfig()
{
    InputType = "text/plain",
    OutputType = "image/png"
});

var kernel = new Kernel();

var variables = new ContextVariables();

await foreach(string chunk in kernel.StreamingRunAsync(variables, function))
{
    Console.WriteLine(chunk);
}