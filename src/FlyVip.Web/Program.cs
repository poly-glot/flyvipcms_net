using System.Text.Encodings.Web;
using System.Text.Unicode;
using FlyVip.Web.Data;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Features.Auth;
using FlyVip.Web.Infrastructure;
using FlyVip.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var testing = builder.Environment.IsEnvironment("Testing");

builder.Services.AddDbContext<AppDbContext>(options => options.UseMySQL(DatabaseConnection.From(builder.Configuration)));
builder.Services.AddDataProtection().SetApplicationName("FlyVip").PersistKeysToDbContext<AppDbContext>();
builder.Services.AddSingleton<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddScoped<PointsService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<MemberService>();
builder.Services.AddScoped<ReservationService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddQueries();

if (testing)
{
    builder.Services.Configure<PasswordHasherOptions>(options => options.IterationCount = 1000);
}

builder.Services.AddWebEncoders(options => options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new IsoDateModelBinderProvider());

    if (!testing)
    {
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    }
}).AddRazorOptions(options =>
{
    options.ViewLocationFormats.Insert(0, "/Features/{1}/{0}.cshtml");
    options.AreaViewLocationFormats.Insert(0, "/Areas/{2}/Features/{1}/{0}.cshtml");
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".FlyVip.Auth";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
        options.LoginPath = "/login";
        options.Events.OnRedirectToLogin = context =>
        {
            var beforeLoginUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;

            if (beforeLoginUrl.Length <= AuthController.BeforeLoginUrlMaxLength)
            {
                Flash(context.HttpContext, AuthController.BeforeLoginUrlKey, beforeLoginUrl);
            }

            context.Response.Redirect("/login");
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            Flash(context.HttpContext, FlashKey.Error, "You do not have the necessary permission to perform the desired operation.");
            context.Response.Redirect("/");
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("admin", policy => policy.RequireRole("admin"))
    .AddPolicy("portal", policy => policy.RequireRole("member", "submember"));

var app = builder.Build();

switch (args)
{
    case ["migrate"]:
        await Migrate(app.Services);
        return 0;
    case ["seed"]:
        await using (var scope = app.Services.CreateAsyncScope())
        {
            await ActivatorUtilities.CreateInstance<DemoSeeder>(scope.ServiceProvider).Run();
        }

        return 0;
    case ["create-admin", var username, var email]:
        return await CreateAdmin(app.Services, username, email);
}

if (Environment.GetEnvironmentVariable("SKIP_MIGRATIONS") != "1")
{
    await Migrate(app.Services);
}

app.UseStaticFiles();
app.UseMiddleware<SessionCookieJar>();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok());
app.MapControllers();

await app.RunAsync();

return 0;

static void Flash(HttpContext context, string key, string value)
{
    var tempData = context.RequestServices.GetRequiredService<ITempDataDictionaryFactory>().GetTempData(context);
    tempData[key] = value;
    tempData.Save();
}

static async Task Migrate(IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
}

static async Task<int> CreateAdmin(IServiceProvider services, string username, string email)
{
    var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD")?.TrimEnd('\r', '\n') is { Length: > 0 } fromEnvironment ? fromEnvironment : Prompt("Password");

    if (password.Length < 12)
    {
        await Console.Error.WriteLineAsync("The Password field must be at least 12 characters in length.");
        return 1;
    }

    await using var scope = services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var now = DateTime.Now;
    var admin = new AppUser { Username = username, Email = email, Role = UserRole.Admin, CreatedAt = now, UpdatedAt = now };

    admin.PasswordHash = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>().HashPassword(admin, password);
    db.Users.Add(admin);
    await db.SaveChangesAsync();

    Console.WriteLine($"Admin {email} created.");

    return 0;
}

static string Prompt(string label)
{
    Console.Write($"{label}: ");

    return Console.ReadLine() ?? "";
}
