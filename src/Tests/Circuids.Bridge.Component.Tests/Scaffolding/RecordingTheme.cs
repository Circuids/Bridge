namespace Circuids.Bridge.Component.Tests.Scaffolding;

internal sealed class RecordingTheme : IBridgeTheme
{
    private readonly InitializationRecorder _recorder;

    public RecordingTheme(InitializationRecorder recorder)
    {
        _recorder = recorder;
    }

    public ThemeMode Theme { get; private set; } = ThemeMode.Light;

    public int InitializeCallCount { get; private set; }

    public event EventHandler<ThemeMode>? ThemeChanged;

    public Task InitializeAsync()
    {
        InitializeCallCount++;
        _recorder.Calls.Add("Theme");
        ThemeChanged?.Invoke(this, Theme);
        return Task.CompletedTask;
    }
}
