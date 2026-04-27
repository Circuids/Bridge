using Circuids.Bridge.TestSupport.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.Maui.Conformance.Tests.Conformance;

public sealed class BridgeHostHandlersMauiConformanceSuite
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public BridgeHostHandlersMauiConformanceSuite(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    [PulseCase]
    public async Task BridgeHostHandler_dispatches_Maui_branch_for_runtime_host()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var bridge = scope.ServiceProvider.GetRequiredService<IBridge>();
        var handler = new RecordingHostHandler(bridge);

        handler.Execute();

        PulseAssert.Equal(nameof(Host.Maui), handler.Branch);
    }

    [PulseCase]
    public async Task BridgeHostHandler_generic_returns_Maui_branch_for_runtime_host()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var bridge = scope.ServiceProvider.GetRequiredService<IBridge>();
        var handler = new ReturningHostHandler(bridge);

        var branch = handler.Execute();

        PulseAssert.Equal(nameof(Host.Maui), branch);
    }

    [PulseCase]
    public async Task BridgeHostHandlerAsync_dispatches_Maui_branch_for_runtime_host()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var bridge = scope.ServiceProvider.GetRequiredService<IBridge>();
        var handler = new RecordingAsyncHostHandler(bridge);

        await handler.ExecuteAsync();

        PulseAssert.Equal(nameof(Host.Maui), handler.Branch);
    }

    [PulseCase]
    public async Task BridgeHostHandlerAsync_generic_returns_Maui_branch_for_runtime_host()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var bridge = scope.ServiceProvider.GetRequiredService<IBridge>();
        var handler = new ReturningAsyncHostHandler(bridge);

        var branch = await handler.ExecuteAsync();

        PulseAssert.Equal(nameof(Host.Maui), branch);
    }

    [PulseCase]
    public Task BridgeHostHandler_uses_Blazor_fallback_for_Wpf_and_WinForms()
    {
        var wpfHandler = new FallbackHostHandler(new StaticBridge(Host.Wpf));
        var winFormsHandler = new FallbackHostHandler(new StaticBridge(Host.WinForms));

        PulseAssert.Equal("BlazorFallback", wpfHandler.Execute());
        PulseAssert.Equal("BlazorFallback", winFormsHandler.Execute());

        return Task.CompletedTask;
    }

    [PulseCase]
    public Task BridgeHostHandler_unknown_host_throws_BridgeException()
    {
        var handler = new FallbackHostHandler(new StaticBridge(Host.Unknown));
        var threw = false;

        try
        {
            _ = handler.Execute();
        }
        catch (BridgeException exception)
        {
            threw = true;
            PulseAssert.False(string.IsNullOrWhiteSpace(exception.Message));
        }

        PulseAssert.True(threw, "Unknown host should throw BridgeException.");
        return Task.CompletedTask;
    }
}
