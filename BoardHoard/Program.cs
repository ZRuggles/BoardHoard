using BoardHoard.Components;
using BoardHoard.Components.Account;
using BoardHoard.Data;
using BoardHoard.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Security.Claims;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// ---------------------- API base (unchanged) ----------------------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Read API base from configuration with a fallback default
var apiBaseStr = builder.Configuration["Api:BaseUrl"] ?? "https://localhost:7021/";
var apiBase = new Uri(apiBaseStr);

// ---------------------- HTTP clients (unchanged) ----------------------
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<UserHeaderHandler>();

builder.Services.AddHttpClient<IBoardGameService, BoardGameService>(c => c.BaseAddress = apiBase)
    .AddHttpMessageHandler<UserHeaderHandler>();
builder.Services.AddHttpClient<ICardGameService, CardGameService>(c => c.BaseAddress = apiBase)
    .AddHttpMessageHandler<UserHeaderHandler>();
builder.Services.AddHttpClient<IImageService, ImageService>(c => { c.BaseAddress = apiBase; })
    .AddHttpMessageHandler<UserHeaderHandler>();
builder.Services.AddHttpClient<IReviewsService, ReviewService>(c => c.BaseAddress = apiBase)
    .AddHttpMessageHandler<UserHeaderHandler>();
builder.Services.AddScoped<IProfileService, ProfileService>();

// ---------------------- Identity (unchanged) ----------------------
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

builder.Services.AddAuthorizationBuilder();

// ---------------------- DB Provider toggle (NEW) ----------------------
// If USE_SQLITE=1, use SQLite on a persistent disk path; else use SQL Server from DefaultConnection
var useSqlite = Environment.GetEnvironmentVariable("USE_SQLITE") == "1";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (useSqlite)
    {
        var dbPath = "/data/boardhoard.identity.db"; // mount a Render Persistent Disk to /data
        options.UseSqlite($"Data Source={dbPath}");
    }
    else
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        options.UseSqlServer(connectionString);
    }
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Password.RequiredLength = 10;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddControllers();

// ---------------------- DataProtection key persistence (NEW) ----------------------
// Prevents auth cookies from breaking on each deploy/restart.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/data/keys"))
    .SetApplicationName("BoardHoard");

var app = builder.Build();

// ---------------------- Forwarded headers (NEW) ----------------------
// Must run BEFORE auth/cookies so scheme/origin are correct behind Render's proxy.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// ---------------------- DB init + Admin seed (MOVED/UPDATED) ----------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (useSqlite)
        db.Database.EnsureCreated();  // simple schema ensure for container-local SQLite
    else
        db.Database.Migrate();        // apply migrations for SQL Server

    try
    {
        await AdminSeeder.SeedAdminAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Admin seeding failed");
    }
}

// ---------------------- Pipeline (unchanged except order) ----------------------
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.MapGet("/api/profile/me", async (UserManager<ApplicationUser> users, IProfileService svc, HttpContext ctx, CancellationToken ct) =>
{
    var userId = users.GetUserId(ctx.User)!;
    var dto = await svc.GetMineAsync(userId, ct) ?? new ProfileViewDto { UserId = userId };
    return Results.Ok(dto);
}).RequireAuthorization();

app.MapPost("/api/profile/update", async (UserManager<ApplicationUser> users, IProfileService svc, HttpContext ctx, ProfileUpdateDto dto, CancellationToken ct) =>
{
    var userId = users.GetUserId(ctx.User)!;
    var ok = await svc.UpsertAsync(userId, dto, ct);
    return ok ? Results.Ok() : Results.BadRequest();
}).RequireAuthorization();

app.Run();

// Delegating handler to push authenticated user's id to the API
internal class UserHeaderHandler(IHttpContextAccessor accessor) : DelegatingHandler
{
    private readonly IHttpContextAccessor _accessor = accessor;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var user = _accessor.HttpContext?.User;
        var userId = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            // API expects X-UserId for create endpoints
            if (!request.Headers.Contains("X-UserId"))
                request.Headers.Add("X-UserId", userId);
        }
        return base.SendAsync(request, cancellationToken);
    }
}
