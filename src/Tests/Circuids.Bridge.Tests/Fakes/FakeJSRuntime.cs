using Microsoft.JSInterop;

namespace Circuids.Bridge.Tests.Fakes;

/// <summary>
/// Minimal stub for IJSObjectReference that returns configurable values per invocation key.
/// </summary>
internal sealed class FakeJSObjectReference : IJSObjectReference
{
    private readonly Dictionary<string, object?> _returnValues = new(StringComparer.Ordinal);
    private bool _disposed;

    public List<(string Identifier, object?[] Args)> Invocations { get; } = new();

    public void SetReturnValue(string identifier, object? value)
    {
        _returnValues[identifier] = value;
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        Invocations.Add((identifier, args ?? Array.Empty<object?>()));

        if (_returnValues.TryGetValue(identifier, out var rawValue))
        {
            if (rawValue is TValue typed)
                return ValueTask.FromResult(typed);
            if (rawValue is null)
                return ValueTask.FromResult(default(TValue)!);
        }

        return ValueTask.FromResult(default(TValue)!);
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        => InvokeAsync<TValue>(identifier, args);

    public ValueTask DisposeAsync()
    {
        _disposed = true;
        return ValueTask.CompletedTask;
    }

    public bool IsDisposed => _disposed;
}

/// <summary>
/// Minimal stub for IJSRuntime that returns a single FakeJSObjectReference on import.
/// </summary>
internal sealed class FakeJSRuntime : IJSRuntime
{
    public FakeJSObjectReference Module { get; } = new();

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        if (identifier == "import" && typeof(TValue) == typeof(IJSObjectReference))
            return ValueTask.FromResult((TValue)(object)Module);

        return ValueTask.FromResult(default(TValue)!);
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        => InvokeAsync<TValue>(identifier, args);
}
