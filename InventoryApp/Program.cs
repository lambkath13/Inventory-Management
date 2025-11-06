using InventoryApp.Data;
using InventoryApp.Models;
using InventoryApp.Services;
using Inventory.Web.Domain.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using InventoryApp.Authorization;
using System.Globalization;
using Microsoft.AspNetCore.SignalR;
using InventoryApp.Infrastructure;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityCore<AppUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

builder.Services.AddHttpContextAccessor();

var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/auth/login";
    o.LogoutPath = "/auth/logout";
    o.AccessDeniedPath = "/auth/login";
});

var googleId     = builder.Configuration["Authentication:Google:ClientId"];
var googleSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleId) && !string.IsNullOrWhiteSpace(googleSecret))
{
      builder.Services.AddAuthentication()
        .AddGoogle(o =>
        {
            o.ClientId = googleId!;
            o.ClientSecret = googleSecret!;
        });
}

var fbId     = builder.Configuration["Authentication:Facebook:AppId"];
var fbSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
if (!string.IsNullOrWhiteSpace(fbId) && !string.IsNullOrWhiteSpace(fbSecret))
{
    builder.Services.AddAuthentication()
            .AddFacebook(o =>
            {
                o.AppId = fbId!;
                o.AppSecret = fbSecret!;
            });
}
builder.Services.AddSignalR();

builder.Services.AddAuthorization(o =>
{
    o.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    o.AddPolicy("CanWriteInventory", p => p.Requirements.Add(new InventoryWriteRequirement()));
});
builder.Services.AddScoped<IAuthorizationHandler, InventoryWriteHandler>();

builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<ICustomIdService, CustomIdService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAccessService, AccessService>();
builder.Services.AddScoped<ITagService, TagService>();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (type, factory)
            => factory.Create(typeof(SharedResource)); 
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var supportedCultures = new[] { "en", "ru" }
    .Select(c => new CultureInfo(c))
    .ToList();
builder.Services.Configure<RequestLocalizationOptions>(opts =>
{
    opts.DefaultRequestCulture = new RequestCulture("en");
    opts.SupportedCultures = supportedCultures;
    opts.SupportedUICultures = supportedCultures;

    opts.RequestCultureProviders = new IRequestCultureProvider[]
    {
        new CookieRequestCultureProvider()
    };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    await Seed.EnsureRolesAndAdminAsync(services);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseRequestLocalization(app.Services
    .GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>().Value);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapHub<InventoryApp.Hubs.DiscussionHub>("/hubs/discussion");

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
