using EcoSystem.API.Settings;
using EcoSystem.API.Swagger;
using EcoSystem.Data;
using EcoSystem.Data.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

Environment.SetEnvironmentVariable("DOTNET_hostBuilder__reloadConfigOnChange", "false");
var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var isRender = string.Equals(Environment.GetEnvironmentVariable("RENDER"), "true", StringComparison.OrdinalIgnoreCase);
var useInMemoryDatabase = isRender || string.IsNullOrWhiteSpace(connectionString);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (useInMemoryDatabase)
    {
        options.UseInMemoryDatabase("EcoSystemRenderDb");
    }
    else
    {
        options.UseSqlServer(connectionString, sqlOptions => sqlOptions.MigrationsAssembly("EcoSystem.API"));
    }
});

var jwtSection = builder.Configuration.GetSection("JwtSettings");
var jwtSettings = jwtSection.Get<JwtSettings>() ?? new JwtSettings();

if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) || jwtSettings.SecretKey.Length < 32 || jwtSettings.SecretKey.StartsWith("REPLACE_WITH", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("JwtSettings__SecretKey must be configured with at least 32 characters.");
}

builder.Services.Configure<JwtSettings>(jwtSection);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el encabezado completo: Bearer {token}"
    });

    options.OperationFilter<AuthorizeOperationFilter>();
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

await SeedUsersAsync(app);

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();

app.Run();

static async Task SeedUsersAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var passwordHasher = new PasswordHasher<User>();

    await AddUserIfConfiguredAsync(
        context,
        passwordHasher,
        username: "admin",
        email: "admin@ecosystem.local",
        role: "Admin",
        password: app.Configuration["SeedUsers:AdminPassword"]);

    await AddUserIfConfiguredAsync(
        context,
        passwordHasher,
        username: "user",
        email: "user@ecosystem.local",
        role: "User",
        password: app.Configuration["SeedUsers:UserPassword"]);
}

static async Task AddUserIfConfiguredAsync(
    ApplicationDbContext context,
    PasswordHasher<User> passwordHasher,
    string username,
    string email,
    string role,
    string? password)
{
    if (string.IsNullOrWhiteSpace(password))
    {
        return;
    }

    var existingUser = await context.Users.FirstOrDefaultAsync(user => user.Username == username);
    if (existingUser != null)
    {
        return;
    }

    var user = new User
    {
        Username = username,
        Email = email,
        Role = role,
        CreatedAt = DateTime.UtcNow
    };

    user.PasswordHash = passwordHasher.HashPassword(user, password);
    context.Users.Add(user);
    await context.SaveChangesAsync();
}

