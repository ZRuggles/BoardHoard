using BoardHoard.Components;
using BoardHoard.Components.Account;
using BoardHoard.Data;
using BoardHoard.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Runtime.Intrinsics.Arm;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Read API base from configuration with a fallback default
var apiBaseStr = builder.Configuration["Api:BaseUrl"] ?? "https://localhost:7021/";
var apiBase = new Uri(apiBaseStr);

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

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
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

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await AdminSeeder.SeedAdminAsync(scope.ServiceProvider);
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
