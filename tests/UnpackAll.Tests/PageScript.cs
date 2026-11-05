using Jint;

namespace UnpackAll.Tests;

/// <summary>
/// page.js exactly as the plugin embeds it, run in Jint with Exiled's Toolkit's page API (api.js, from the Toolkit's
/// source) on top of FakePage.js (Root.html, the click menu and the vuplex bridge). The calls into the script are the
/// ones the plugin makes through window.__slunpackall.
/// </summary>
internal sealed class PageScript
{
    private readonly Engine _engine = new();

    /// <param name="isToolkitFirst">
    /// The game runs the Toolkit's script first, or with false Unpack All's first; then the Toolkit's runs only at
    /// <see cref="LoadToolkit"/>.
    /// </param>
    public PageScript(bool isToolkitFirst = true)
    {
        _engine.Execute(Resource("UnpackAll.Tests.FakePage.js"));

        if (isToolkitFirst) LoadToolkit();

        _engine.Execute(Resource("UnpackAll.page.js"));
    }

    /// <summary>The Toolkit's page API arriving, after Unpack All's script when the page was built with isToolkitFirst false.</summary>
    public void LoadToolkit() => _engine.Execute(Resource("ExiledToolkit.api.js"));

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
