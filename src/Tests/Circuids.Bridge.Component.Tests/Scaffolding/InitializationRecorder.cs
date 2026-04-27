namespace Circuids.Bridge.Component.Tests.Scaffolding;

internal sealed class InitializationRecorder
{
    public List<string> Calls { get; } = new();
}
