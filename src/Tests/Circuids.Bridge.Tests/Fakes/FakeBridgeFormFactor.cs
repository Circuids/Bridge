namespace Circuids.Bridge.Tests.Fakes;

internal sealed class FakeBridgeFormFactor : IBridgeFormFactor
{
    public FormFactorInfo FormFactor { get; set; } = FormFactorInfo.Unknown();
    public int InitializeCallCount { get; private set; }
    public ResizeMode LastResizeMode { get; private set; }
    public int CreateListenerCallCount { get; private set; }
    public int DisposeListenerCallCount { get; private set; }

    public event EventHandler<FormFactorInfo>? FormFactorChanged;

    public Task InitializeAsync(ResizeMode resizeMode = ResizeMode.None)
    {
        InitializeCallCount++;
        LastResizeMode = resizeMode;
        FormFactorChanged?.Invoke(this, FormFactor);
        return Task.CompletedTask;
    }

    public Task CreateListenerAsync()
    {
        CreateListenerCallCount++;
        return Task.CompletedTask;
    }

    public ValueTask DisposeListenerAsync()
    {
        DisposeListenerCallCount++;
        return ValueTask.CompletedTask;
    }

    public void RaiseFormFactorChanged(FormFactorInfo info)
    {
        FormFactor = info;
        FormFactorChanged?.Invoke(this, info);
    }
}
