
using FluentValidation;
using System.Reflection;
using System.Text.Json.Serialization;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.DescribeAllParametersInCamelCase();
            c.EnableAnnotations();
        });

        builder.Services.AddControllers()
            .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); }); ;

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Services
            .AddPostgreSqlConfig(builder.Configuration)
            .AddRedisConfig(builder.Configuration)
            .AddHealthChecks()
            .AddPostgreSqlHealth(builder.Configuration)
            .AddRedisHealth(builder.Configuration);

        builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();
        builder.Services.AddScoped<IBookService, BookService>();

        var app = builder.Build();

        app.UseSwagger().UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Books  API");
            options.DocumentTitle = "Books API";
        });
        app.ApplyMigrations();

        app.MapHealthChecks("health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.UseHttpsRedirection();

        app.MapBookEndpoints();
        app.MapControllers();

        app.Run();
    }
}