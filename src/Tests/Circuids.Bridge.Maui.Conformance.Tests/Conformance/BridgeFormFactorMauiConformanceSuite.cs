using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.Maui.Conformance.Tests.Conformance;

public sealed class BridgeFormFactorMauiConformanceSuite
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public BridgeFormFactorMauiConformanceSuite(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    [PulseCase]
    public async Task FormFactor_defaults_to_unknown_before_initialization()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var formFactor = scope.ServiceProvider.GetRequiredService<IBridgeFormFactor>();

        PulseAssert.Equal(FormFactorInfo.Unknown(), formFactor.FormFactor);
    }

    [PulseCase]
    public async Task InitializeAsync_reads_real_Maui_window_form_factor()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var formFactor = scope.ServiceProvider.GetRequiredService<IBridgeFormFactor>();
        FormFactorInfo? raisedValue = null;

        formFactor.FormFactorChanged += OnFormFactorChanged;

        try
        {
            await formFactor.InitializeAsync(ResizeMode.None);

            PulseAssert.True(Enum.IsDefined(formFactor.FormFactor.FormFactor));
            PulseAssert.True(formFactor.FormFactor.Width >= 0);
            PulseAssert.True(formFactor.FormFactor.Height >= 0);
            PulseAssert.Equal(formFactor.FormFactor, raisedValue);
        }
        finally
        {
            formFactor.FormFactorChanged -= OnFormFactorChanged;
        }

        void OnFormFactorChanged(object? sender, FormFactorInfo info)
        {
            raisedValue = info;
        }
    }

    [PulseCase]
    public async Task InitializeAsync_is_idempotent_and_raises_FormFactorChanged_once()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var formFactor = scope.ServiceProvider.GetRequiredService<IBridgeFormFactor>();
        var eventCount = 0;

        formFactor.FormFactorChanged += OnFormFactorChanged;

        try
        {
            await formFactor.InitializeAsync(ResizeMode.None);
            var firstValue = formFactor.FormFactor;

            await formFactor.InitializeAsync(ResizeMode.None);

            PulseAssert.Equal(1, eventCount);
            PulseAssert.Equal(firstValue, formFactor.FormFactor);
        }
        finally
        {
            formFactor.FormFactorChanged -= OnFormFactorChanged;
        }

        void OnFormFactorChanged(object? sender, FormFactorInfo info)
        {
            eventCount++;
        }
    }

    [PulseCase]
    public async Task InitializeAsync_raises_FormFactorChanged_with_service_sender()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var formFactor = scope.ServiceProvider.GetRequiredService<IBridgeFormFactor>();
        object? raisedSender = null;

        formFactor.FormFactorChanged += OnFormFactorChanged;

        try
        {
            await formFactor.InitializeAsync(ResizeMode.None);
            PulseAssert.True(ReferenceEquals(formFactor, raisedSender));
        }
        finally
        {
            formFactor.FormFactorChanged -= OnFormFactorChanged;
        }

        void OnFormFactorChanged(object? sender, FormFactorInfo info)
        {
            raisedSender = sender;
        }
    }

    [PulseCase]
    public async Task InitializeAsync_with_Global_raises_FormFactorChanged_once()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var formFactor = scope.ServiceProvider.GetRequiredService<IBridgeFormFactor>();
        var eventCount = 0;

        formFactor.FormFactorChanged += OnFormFactorChanged;

        try
        {
            await formFactor.InitializeAsync(ResizeMode.Global);
            PulseAssert.Equal(1, eventCount);
        }
        finally
        {
            formFactor.FormFactorChanged -= OnFormFactorChanged;
        }

        void OnFormFactorChanged(object? sender, FormFactorInfo info)
        {
            eventCount++;
        }
    }

    [PulseCase]
    public async Task CreateListenerAsync_before_initialization_throws_BridgeException()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var formFactor = scope.ServiceProvider.GetRequiredService<IBridgeFormFactor>();
        var threw = false;

        try
        {
            await formFactor.CreateListenerAsync();
        }
        catch (BridgeException exception)
        {
            threw = true;
            PulseAssert.False(string.IsNullOrWhiteSpace(exception.Message));
        }

        PulseAssert.True(threw, "CreateListenerAsync should require initialization.");
    }

    [PulseCase]
    public async Task CreateListenerAsync_attaches_without_throwing_after_initialization()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var formFactor = scope.ServiceProvider.GetRequiredService<IBridgeFormFactor>();

        await formFactor.InitializeAsync(ResizeMode.None);
        await formFactor.CreateListenerAsync();
    }

    [PulseCase]
    public async Task CreateListenerAsync_does_not_raise_FormFactorChanged_when_value_did_not_change()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var formFactor = scope.ServiceProvider.GetRequiredService<IBridgeFormFactor>();

        await formFactor.InitializeAsync(ResizeMode.None);

        var eventCount = 0;
        formFactor.FormFactorChanged += OnFormFactorChanged;

        try
        {
            await formFactor.CreateListenerAsync();
            PulseAssert.Equal(0, eventCount);
        }
        finally
        {
            formFactor.FormFactorChanged -= OnFormFactorChanged;
        }

        void OnFormFactorChanged(object? sender, FormFactorInfo info)
        {
            eventCount++;
        }
    }

    [PulseCase]
    public async Task CreateListenerAsync_can_be_called_multiple_times_after_initialization()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var formFactor = scope.ServiceProvider.GetRequiredService<IBridgeFormFactor>();

        await formFactor.InitializeAsync(ResizeMode.None);
        await formFactor.CreateListenerAsync();
        await formFactor.CreateListenerAsync();

        PulseAssert.True(Enum.IsDefined(formFactor.FormFactor.FormFactor));
    }

    [PulseCase]
    public async Task DisposeListenerAsync_before_listener_exists_does_not_throw()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var formFactor = scope.ServiceProvider.GetRequiredService<IBridgeFormFactor>();

        await formFactor.InitializeAsync(ResizeMode.None);
        await formFactor.DisposeListenerAsync();

        PulseAssert.True(Enum.IsDefined(formFactor.FormFactor.FormFactor));
    }

    [PulseCase]
    public async Task ResizeModeOnce_keeps_CreateListenerAsync_as_noop()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var formFactor = scope.ServiceProvider.GetRequiredService<IBridgeFormFactor>();

        await formFactor.InitializeAsync(ResizeMode.Once);
        await formFactor.CreateListenerAsync();

        PulseAssert.True(Enum.IsDefined(formFactor.FormFactor.FormFactor));
    }

    [PulseCase]
    public async Task ResizeModeOnce_keeps_DisposeListenerAsync_as_noop()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var formFactor = scope.ServiceProvider.GetRequiredService<IBridgeFormFactor>();

        await formFactor.InitializeAsync(ResizeMode.Once);
        await formFactor.DisposeListenerAsync();

        PulseAssert.True(Enum.IsDefined(formFactor.FormFactor.FormFactor));
    }
}