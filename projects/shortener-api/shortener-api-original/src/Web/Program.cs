using Carter;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
using Web;
using Web.Middlewares;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //builder.Services.AddOpenApi();
        builder.Services.AddSwaggerGen(c =>
        {
            c.DescribeAllParametersInCamelCase();
            c.EnableAnnotations();
        });
        builder.Services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add(new ProducesAttribute("application/json"));
            options.Filters.Add(new ConsumesAttribute("application/json"));
        });
        
        builder.Services.AddControllers()
            .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); }); ;

        builder.Services.AddCarter();
        builder.Services.AddWeb(builder.Configuration);
        builder.Services.AddExceptionHandler<GlobalExceptionMiddleware>();

        var app = builder.Build();

        //app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapScalarApiReference(opt =>
        {
            opt.WithTitle("URL Shortener")
                .WithDarkMode(true)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });

        app.UseHttpsRedirection();

        app.MapCarter();
        await app.RunAsync();
    }
}