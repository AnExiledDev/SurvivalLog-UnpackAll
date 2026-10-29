using Jint;

namespace UnpackAll.Tests;

/// <summary>
/// page.js exactly as the plugin embeds it, run in Jint on top of FakePage.js (Root.html, the Mods menu and the vuplex
/// bridge). The calls into the script are the ones the plugin makes through window.__slunpackall.
/// </summary>
internal sealed class PageScript
{
    private readonly Engine _engine = new();

    public PageScript()
    {
        _engine.Execute(Resource("UnpackAll.Tests.FakePage.js"));
        _engine.Execute(Resource("UnpackAll.page.js"));
    }

    public void Run(string statements) => _engine.Execute(statements);

    public string Text(string expression) => _engine.Evaluate($"String({expression})").AsString();

    public bool Is(string expression) => _engine.Evaluate($"!!({expression})").AsBoolean();

    public void Answered() => Run("window.__slunpackall.answered()");

    private static string Resource(string name)
    {
        using var stream = typeof(PageScript).Assembly.GetManifestResourceStream(name) ?? throw new InvalidOperationException(name + " is not embedded in the test assembly");
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}
