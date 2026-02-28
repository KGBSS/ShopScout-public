using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ShopScout.Client.Services;
using ShopScout.SharedLib.Services;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace ShopScout.Client.Components;

public partial class BarcodeReader : IAsyncDisposable
{
    [Inject]
    [NotNull]
    private IJSRuntime? JSRuntime { get; set; }

    [Inject]
    [NotNull]
    private StorageService? Storage { get; set; }

    private IJSObjectReference? Module { get; set; }
    private DotNetObjectReference<BarcodeReader>? Instance { get; set; }

    [Parameter] public EventCallback<string> ScanResult { get; set; }
    [Parameter] public EventCallback Close { get; set; }
    [Parameter] public Func<string, Task>? OnError { get; set; }
    [Parameter] public bool UseBuiltinDiv { get; set; } = true;
    [Parameter] public ZXingBlazorStyle Style { get; set; } = ZXingBlazorStyle.Modal;
    [Parameter] public bool Pdf417Only { get; set; }
    [Parameter] public bool Decodeonce { get; set; } = true;
    [Parameter] public bool DecodeAllFormats { get; set; }
    [Parameter] public ZXingOptions? Options { get; set; }
    [Parameter] public string? DeviceID { get; set; }
    [Parameter] public bool SaveDeviceID { get; set; } = true;
    [Parameter] public bool Screenshot { get; set; }
    [Parameter] public bool StreamFromZxing { get; set; }
    [Parameter] public bool TorchOn { get; set; }

    public ElementReference Element { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        try
        {
            Module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "/BarcodeReader.js");
            Instance = DotNetObjectReference.Create(this);

            if (SaveDeviceID)
            {
                try { DeviceID = await Storage.GetValue("CamsDeviceID", DeviceID); }
                catch { }
            }

            Options ??= new()
            {
                Pdf417 = Pdf417Only,
                Decodeonce = Decodeonce,
                DecodeAllFormats = DecodeAllFormats,
                Screenshot = Screenshot,
                StreamFromZxing = StreamFromZxing,
                DeviceID = DeviceID,
                TRY_HARDER = true
            };

            await Module.InvokeVoidAsync("init", Instance, Element, Element.Id, Options, DeviceID);
        }
        catch (Exception e)
        {
            if (OnError != null) await OnError.Invoke(e.Message);
        }
    }

    public async Task ToggleFlashlight()
    {
        if (Module is null) return;
        TorchOn = !TorchOn;
        await Module.InvokeVoidAsync("toggleFlashlight", Element.Id, TorchOn);
    }

    public async Task Start()
    {
        if (Module is not null) await Module.InvokeVoidAsync("start", Element.Id);
    }

    public async Task Stop()
    {
        if (Module is not null) await Module.InvokeVoidAsync("stop", Element.Id);
    }

    public async Task Reload()
    {
        if (Module is not null) await Module.InvokeVoidAsync("reload", Element.Id);
    }

    [JSInvokable]
    public async Task GetResult(string val) => await ScanResult.InvokeAsync(val);

    [JSInvokable]
    public async Task CloseScan() => await Close.InvokeAsync();

    [JSInvokable]
    public async Task GetError(string err)
    {
        if (OnError != null) await OnError.Invoke(err);
    }

    [JSInvokable]
    public async Task SelectDeviceID(string deviceID, string deviceName)
    {
        if (!SaveDeviceID) return;
        try
        {
            await Storage.SetValue("CamsDeviceID", deviceID);
            await Storage.SetValue("CamsDeviceName", deviceName);
        }
        catch { }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (Module is not null)
            {
                await Module.InvokeVoidAsync("destroy", Element.Id);
                await Module.DisposeAsync();
            }
        }
        catch { }
        finally
        {
            Instance?.Dispose();
        }
    }
}