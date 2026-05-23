using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using TodoApi.Extensions;
using TodoApi.Migrations;
using static TodoApi.AuthorizationHandlerExtensions;
using static TodoApi.CurrentUserExtensions;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure auth
        builder.Services.AddAuthentication().AddJwtBearer();
        builder.Services.AddAuthorizationBuilder();
        builder.Services.AddScoped<IAuthorizationHandler, CheckCurrentUserAuthHandler>();

        // Add the service to generate JWT tokens
        builder.Services.AddSingleton<ITokenService, TokenService>();

        // Configure the database
        var connectionString = builder.Configuration.GetConnectionString("Todos") ?? "Data Source=.db/Todos.db";
        builder.Services.AddSqlite<TodoDbContext>(connectionString);

        // Configure identity
        builder.Services.AddIdentityCore<TodoUser>()
                        .AddEntityFrameworkStores<TodoDbContext>();

        // State that represents the current user from the database *and* the request
        builder.Services.AddScoped<CurrentUser>();
        builder.Services.AddScoped<IClaimsTransformation, ClaimsTransformation>();

        // Configure Open API
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(o => {
            o.DescribeApi();
        });

        // Configure rate limiting
        builder.Services.AddRateLimiting();

        // Configure OpenTelemetry
        builder.AddOpenTelemetry();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var databasePath = Path.GetDirectoryName(connectionString.Split("=", 2)[1]);
                if (!Directory.Exists(databasePath))
                {
                    Directory.CreateDirectory(databasePath);
                }

                var context = services.GetRequiredService<TodoDbContext>();

                var databaseConfig = new DatabaseConfig();
                builder.Configuration.GetSection("Database").Bind(databaseConfig);
                if (databaseConfig.Clear)
                {
                    context.Database.EnsureDeleted();
                }

                context.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Error to apply migrations");
            }
        }
        
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseRateLimiter();

        app.Map("/", () => Results.Redirect("/swagger"));

        // Configure the APIs
        app.MapTodos();
        app.MapUsers();

        // Configure the prometheus endpoint for scraping metrics
        app.MapPrometheusScrapingEndpoint();
        // NOTE: This should only be exposed on an internal port!
        // .RequireHost("*:9100");

        app.Run();
    }
}