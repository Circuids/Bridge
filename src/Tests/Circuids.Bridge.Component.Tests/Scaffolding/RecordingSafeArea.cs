namespace Circuids.Bridge.Component.Tests.Scaffolding;

internal sealed class RecordingSafeArea : IBridgeSafeArea
{
    private readonly InitializationRecorder _recorder;

    public RecordingSafeArea(InitializationRecorder recorder)
    {
        _recorder = recorder;
    }

    public SafeAreaInsets SafeArea { get; private set; } = SafeAreaInsets.Zero;

    public int InitializeCallCount { get; private set; }

    public event EventHandler<SafeAreaInsets>? SafeAreaChanged;

    public Task InitializeAsync()
    {
        InitializeCallCount++;
        _recorder.Calls.Add("SafeArea");
        SafeAreaChanged?.Invoke(this, SafeArea);
        return Task.CompletedTask;
    }
}
