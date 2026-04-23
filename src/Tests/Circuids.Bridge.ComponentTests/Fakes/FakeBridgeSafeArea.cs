namespace Circuids.Bridge.ComponentTests.Fakes;

internal sealed class FakeBridgeSafeArea : IBridgeSafeArea
{
    public SafeAreaInsets SafeArea { get; set; } = SafeAreaInsets.Zero;
    public int InitializeCallCount { get; private set; }

    public event EventHandler<SafeAreaInsets>? SafeAreaChanged;

    public Task InitializeAsync()
    {
        InitializeCallCount++;
        return Task.CompletedTask;
    }

    public void RaiseSafeAreaChanged(SafeAreaInsets insets)
    {
        SafeArea = insets;
        SafeAreaChanged?.Invoke(this, insets);
    }
}
