using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0211:Convert to 'Program.Main' style program", Justification = "<Pending>")]

class SecurityService : IPromptInjectionModule, IPersonalInformationScanModule, ICredentialScanModule
{
    public string RemoveCredentials(string prompt)
    {
        var fixedPrompt = prompt;
        foreach (var credential in this.ScanCredentials(prompt))
        {
            fixedPrompt = fixedPrompt.Replace(credential.Target, new string('*', credential.Target.Length));
        }

        return fixedPrompt;
    }

    public IEnumerable<ScanResult> ScanCredentials(string prompt)
    {
        var credIndexOf = prompt.IndexOf("password");
        if (credIndexOf != -1)
        {
            yield return new ScanResult("password", credIndexOf);
        }
    }

    public IEnumerable<ScanResult> ScanInjections(string prompt)
    {
        var injectionIndexOf = prompt.IndexOf("system");
        if (injectionIndexOf != -1)
        {
            yield return new ScanResult("system", injectionIndexOf);
        }
    }

    public IEnumerable<ScanResult> ScanPII(string prompt)
    {
        var piiIndexOf = prompt.IndexOf("email");
        if (piiIndexOf != -1)
        {
            yield return new ScanResult("email", piiIndexOf);
        }
    }
}
record ScanResult(string Target, int StartIndexPosition);

interface IPromptInjectionModule
{
    IEnumerable<ScanResult> ScanInjections(string prompt);
}

interface IPersonalInformationScanModule
{
    IEnumerable<ScanResult> ScanPII(string prompt);
}

interface ICredentialScanModule
{
    IEnumerable<ScanResult> ScanCredentials(string prompt);
    string RemoveCredentials(string prompt);
}
