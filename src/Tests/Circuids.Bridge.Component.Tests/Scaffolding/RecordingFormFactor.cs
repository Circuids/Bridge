namespace Circuids.Bridge.Component.Tests.Scaffolding;

internal sealed class RecordingFormFactor : IBridgeFormFactor
{
    private readonly InitializationRecorder _recorder;

    public RecordingFormFactor(InitializationRecorder recorder)
    {
        _recorder = recorder;
    }

    public FormFactorInfo FormFactor { get; private set; } = FormFactorInfo.Unknown();

    public ResizeMode LastResizeMode { get; private set; }

    public int InitializeCallCount { get; private set; }

    public event EventHandler<FormFactorInfo>? FormFactorChanged;

    public Task InitializeAsync(ResizeMode resizeMode = ResizeMode.None)
    {
        InitializeCallCount++;
        _recorder.Calls.Add("FormFactor");
        LastResizeMode = resizeMode;
        FormFactor = new FormFactorInfo(Circuids.Bridge.FormFactor.Desktop, 1280, 720);
        FormFactorChanged?.Invoke(this, FormFactor);
        return Task.CompletedTask;
    }

    public Task CreateListenerAsync() => Task.CompletedTask;

    public ValueTask DisposeListenerAsync() => ValueTask.CompletedTask;
}
