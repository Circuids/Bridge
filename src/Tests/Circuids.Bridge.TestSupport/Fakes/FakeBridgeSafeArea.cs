namespace Circuids.Bridge.TestSupport.Fakes;

public sealed class FakeBridgeSafeArea : IBridgeSafeArea
{
    public SafeAreaInsets SafeArea { get; set; } = SafeAreaInsets.Zero;
    public int InitializeCallCount { get; private set; }

    public event EventHandler<SafeAreaInsets>? SafeAreaChanged;

    public Task InitializeAsync()
    {
        InitializeCallCount++;
        SafeAreaChanged?.Invoke(this, SafeArea);
        return Task.CompletedTask;
    }

    public void RaiseSafeAreaChanged(SafeAreaInsets insets)
    {
        SafeArea = insets;
        SafeAreaChanged?.Invoke(this, insets);
    }
}
