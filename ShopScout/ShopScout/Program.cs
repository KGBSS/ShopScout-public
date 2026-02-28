using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using ShopScout.Components;
using ShopScout.Components.Account;
using ShopScout.Data;
using ShopScout.Services;
using ShopScout.SharedLib.Models;
using ShopScout.SharedLib.Services;
using System.Globalization;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var logTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(Matching.FromSource("Microsoft.EntityFrameworkCore"))
        .WriteTo.File("logs/ef-core-.txt", 
            rollingInterval: RollingInterval.Day,
            shared: true,
            fileSizeLimitBytes: 10 * 1024 * 1024, // 10 MB limit per file
            outputTemplate: logTemplate,
            rollOnFileSizeLimit: true,
            retainedFileCountLimit: 7))

    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error)
        .WriteTo.File("logs/errors-.txt",
            rollingInterval: RollingInterval.Day,
            shared: true,
            outputTemplate: logTemplate))

    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Fatal)
        .WriteTo.File("logs/fatals-.txt",
            rollingInterval: RollingInterval.Day,
            shared: true,
            outputTemplate: logTemplate))

    .WriteTo.Logger(lc => lc
        .Filter.ByExcluding(Matching.FromSource("Microsoft.EntityFrameworkCore"))
        .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information || e.Level == LogEventLevel.Warning)
        .WriteTo.File("logs/app-.txt",
            rollingInterval: RollingInterval.Day,
            shared: true,
            outputTemplate: logTemplate))

    .WriteTo.Console(
        restrictedToMinimumLevel: LogEventLevel.Error,
        outputTemplate: logTemplate)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
}); ;
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<LogService>();
builder.Services.AddHttpClient<StatsService>();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<QueuedHostedService>();

if (builder.Environment.IsProduction())
    builder.Services.AddHostedService<DailyTaskScheduler>();

if (Environment.GetEnvironmentVariable("Authentication:Google:ClientId") != null)
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    }).AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = Environment.GetEnvironmentVariable("Authentication:Google:ClientId");
        googleOptions.ClientSecret = Environment.GetEnvironmentVariable("Authentication:Google:ClientSecret");
    }).AddIdentityCookies();
}
else
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    }).AddIdentityCookies();
}

builder.Services.AddScoped(http => new HttpClient
{
    BaseAddress = new Uri("https://shopscout.hu/")
});
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<IStoreLayoutService, StoreLayoutService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IImageStorageService, GoogleCloudImageStorage>();
builder.Services.AddScoped<IUserAccessor, UserAccessor>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ServerCookieService>();
builder.Services.AddScoped<StorageService>();
builder.Services.AddScoped<LocationService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AccountNavbarService>();
builder.Services.AddScoped<ArfigyeloFetchService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

builder.Services.AddDataProtection()
    .PersistKeysToDbContext<ApplicationDbContext>()
    .SetApplicationName("ShopScout");

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "AuthCookie";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSingleton<ShopScout.Services.IEmailSender, EmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.Use(async (context, next) =>
{
    // middleware for hiding admin pages
    if (context.Request.Path.StartsWithSegments("/admin"))
    {
        if (context.User == null || !context.User.Identity.IsAuthenticated || !context.User.IsInRole("Admin"))
        {
            context.Response.StatusCode = 404;
            return;
        }
    }

    string[] disabledUrls = ["/Account/Manage/ExternalLogins", "/Account/Manage/ResetAuthenticator", "/Account/Manage/SetPassword"];
    foreach (var disabledUrl in disabledUrls)
    {
        if (context.Request.Path.StartsWithSegments(disabledUrl))
        {
            context.Response.StatusCode = 404;
            return;
        }
    }
    await next();
});

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ShopScout.Client._Imports).Assembly);

app.MapAdditionalIdentityEndpoints();
app.MapControllers();

// for running on localhost in a hungarian culture
var culture = new CultureInfo("en-GB");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

try
{
    Log.Information("Starting web host");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}