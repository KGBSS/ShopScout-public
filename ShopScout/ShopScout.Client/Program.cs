using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ShopScout.SharedLib.Services;
using System.Globalization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddScoped(http => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});
builder.Services.AddScoped<IStoreLayoutService, ClientStoreLayoutService>();
builder.Services.AddScoped<IProductService, ClientProductService>();
builder.Services.AddScoped<IStoreService, ClientStoreService>();
builder.Services.AddScoped<StorageService>();

// for running on localhost in a hungarian culture
var culture = new CultureInfo("en-GB");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

await builder.Build().RunAsync();