using System.Text.Json;
using System.Text.Json.Serialization;
using Circuids.Bridge.Blazor.Internal;
using Circuids.Bridge.Tests.Fakes;

namespace Circuids.Bridge.Tests.Blazor;

public sealed class BridgeFormFactorBlazorTests
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private static string SerializeFormFactor(FormFactorInfo info)
        => JsonSerializer.Serialize(info, _jsonOptions);

    // ── Initial state ─────────────────────────────────────────────────────────

    [Fact]
    public void FormFactor_IsUnknown_BeforeInitialize()
    {
        var jsRuntime = new FakeJSRuntime();
        var service = new BridgeFormFactorBlazor(jsRuntime);

        Assert.Equal(FormFactorInfo.Unknown(), service.FormFactor);
    }

    // ── InitializeAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task InitializeAsync_SetsFormFactorFromJSResult()
    {
        var jsRuntime = new FakeJSRuntime();
        var expected = new FormFactorInfo(FormFactor.Desktop, 1920, 1080);
        jsRuntime.Module.SetReturnValue("getFormFactor", SerializeFormFactor(expected));

        var service = new BridgeFormFactorBlazor(jsRuntime);
        await service.InitializeAsync();

        Assert.Equal(expected, service.FormFactor);
    }

    [Fact]
    public async Task InitializeAsync_RaisesFormFactorChangedEvent()
    {
        var jsRuntime = new FakeJSRuntime();
        var expected = new FormFactorInfo(FormFactor.Phone, 390, 844);
        jsRuntime.Module.SetReturnValue("getFormFactor", SerializeFormFactor(expected));

        var service = new BridgeFormFactorBlazor(jsRuntime);
        FormFactorInfo? raised = null;
        service.FormFactorChanged += (_, f) => raised = f;

        await service.InitializeAsync();

        Assert.Equal(expected, raised);
    }

    [Fact]
    public async Task InitializeAsync_IsIdempotent()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getFormFactor", SerializeFormFactor(FormFactorInfo.Unknown()));

        var service = new BridgeFormFactorBlazor(jsRuntime);
        await service.InitializeAsync();
        await service.InitializeAsync();

        var getCalls = jsRuntime.Module.Invocations.Count(i => i.Identifier == "getFormFactor");
        Assert.Equal(1, getCalls);
    }

    [Fact]
    public async Task InitializeAsync_WithResizeModeGlobal_CallsCreateListener()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getFormFactor", SerializeFormFactor(FormFactorInfo.Unknown()));

        var service = new BridgeFormFactorBlazor(jsRuntime);
        await service.InitializeAsync(ResizeMode.Global);

        Assert.Contains(jsRuntime.Module.Invocations, i => i.Identifier == "initialize");
    }

    [Fact]
    public async Task InitializeAsync_WithResizeModeNone_DoesNotCallCreateListener()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getFormFactor", SerializeFormFactor(FormFactorInfo.Unknown()));

        var service = new BridgeFormFactorBlazor(jsRuntime);
        await service.InitializeAsync(ResizeMode.None);

        Assert.DoesNotContain(jsRuntime.Module.Invocations, i => i.Identifier == "initialize");
    }

    // ── NotifyFormFactorChanged callback ──────────────────────────────────────

    [Fact]
    public async Task NotifyFormFactorChanged_UpdatesFormFactor_WhenValueChanges()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getFormFactor", SerializeFormFactor(FormFactorInfo.Unknown()));

        var service = new BridgeFormFactorBlazor(jsRuntime);
        await service.InitializeAsync();

        var newInfo = new FormFactorInfo(FormFactor.Desktop, 1920, 1080);
        await service.NotifyFormFactorChanged(SerializeFormFactor(newInfo));

        Assert.Equal(newInfo, service.FormFactor);
    }

    [Fact]
    public async Task NotifyFormFactorChanged_RaisesEvent_WhenValueChanges()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getFormFactor", SerializeFormFactor(FormFactorInfo.Unknown()));

        var service = new BridgeFormFactorBlazor(jsRuntime);
        await service.InitializeAsync();

        FormFactorInfo? raised = null;
        service.FormFactorChanged += (_, f) => raised = f;

        var newInfo = new FormFactorInfo(FormFactor.Desktop, 1920, 1080);
        await service.NotifyFormFactorChanged(SerializeFormFactor(newInfo));

        Assert.Equal(newInfo, raised);
    }

    [Fact]
    public async Task NotifyFormFactorChanged_DoesNotRaiseEvent_WhenSameValue()
    {
        var jsRuntime = new FakeJSRuntime();
        var initial = new FormFactorInfo(FormFactor.Desktop, 1920, 1080);
        jsRuntime.Module.SetReturnValue("getFormFactor", SerializeFormFactor(initial));

        var service = new BridgeFormFactorBlazor(jsRuntime);
        await service.InitializeAsync();

        var eventCount = 0;
        service.FormFactorChanged += (_, _) => eventCount++;

        await service.NotifyFormFactorChanged(SerializeFormFactor(initial)); // same value

        Assert.Equal(0, eventCount);
    }

    [Fact]
    public async Task NotifyFormFactorChanged_ThrowsBridgeException_WhenNotInitialized()
    {
        var jsRuntime = new FakeJSRuntime();
        var service = new BridgeFormFactorBlazor(jsRuntime);

        var act = async () => await service.NotifyFormFactorChanged(
            SerializeFormFactor(new FormFactorInfo(FormFactor.Phone, 390, 844)));

        await Assert.ThrowsAsync<BridgeException>(act);
    }

    // ── DisposeAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DisposeAsync_DoesNotThrow_WhenNotInitialized()
    {
        var jsRuntime = new FakeJSRuntime();
        var service = new BridgeFormFactorBlazor(jsRuntime);

        var act = async () => await service.DisposeAsync();

        await act();
    }

    [Fact]
    public async Task DisposeAsync_DisposesJSModule_WhenInitialized()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getFormFactor", SerializeFormFactor(FormFactorInfo.Unknown()));

        var service = new BridgeFormFactorBlazor(jsRuntime);
        await service.InitializeAsync();
        await service.DisposeAsync();

        Assert.True(jsRuntime.Module.IsDisposed);
    }
}
