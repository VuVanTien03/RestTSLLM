using HotelListing.API.Core.Configurations;
using HotelListing.API.Core.Contracts;
using HotelListing.API.Core.Middleware;
using HotelListing.API.Core.Repository;
using HotelListing.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using System.Text.Json.Serialization;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connectionString = builder.Configuration.GetConnectionString("HotelListingDbConnectionString");
        builder.Services.AddDbContext<HotelListingDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        builder.Services.AddIdentityCore<ApiUser>()
            .AddRoles<IdentityRole>()
            .AddTokenProvider<DataProtectorTokenProvider<ApiUser>>("HotelListingApi")
            .AddEntityFrameworkStores<HotelListingDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Hotel Listing API", Version = "v1" });
            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Description = @"JWT Authorization header using the Bearer scheme. 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = JwtBearerDefaults.AuthenticationScheme
            });

            options.DescribeAllParametersInCamelCase();
            options.EnableAnnotations();
            options.OperationFilter<RemoveOtherContentTypesFilter>();
            options.OperationFilter<AuthResponsesOperationFilter>();
            options.OperationFilter<AuthorizeCheckOperationFilter>();

        });

        builder.Services.AddControllers()
            .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); }); ;

        builder.Services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add(new ProducesAttribute("application/json"));
            options.Filters.Add(new ConsumesAttribute("application/json"));
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                b => b.AllowAnyHeader()
                    .AllowAnyOrigin()
                    .AllowAnyMethod());
        });

        builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console().ReadFrom.Configuration(ctx.Configuration));

        builder.Services.AddAutoMapper(typeof(MapperConfig));

        builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
        builder.Services.AddScoped<IHotelsRepository, HotelsRepository>();
        builder.Services.AddScoped<IAuthManager, AuthManager>();

        builder.Services.AddAuthentication()
            .AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                    ValidAudience = builder.Configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
                };
            });

        builder.Services.AddResponseCaching(options =>
        {
            options.MaximumBodySize = 1024;
            options.UseCaseSensitivePaths = true;
        });

        builder.Services.AddControllers().AddOData(options =>
        {
            options.Select().Filter().OrderBy();
        });

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseSerilogRequestLogging();

        app.UseMiddleware<ExceptionMiddleware>();

        app.UseHttpsRedirection();

        app.UseCors("AllowAll");

        app.UseResponseCaching();

        app.Use(async (context, next) =>
        {
            context.Response.GetTypedHeaders().CacheControl =
                new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                {
                    Public = true,
                    MaxAge = TimeSpan.FromSeconds(10)
                };
            context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
                new string[] { "Accept-Encoding" };

            await next();
        });

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HotelListingDbContext>();
            while (dbContext.Database.GetPendingMigrations().Any())
            {
                try
                {
                    dbContext.Database.Migrate();
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                }
            }
        }

        app.Run();
    }

    public class RemoveOtherContentTypesFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            foreach (var response in operation.Responses)
            {
                response.Value.Content = response.Value.Content
                    .Where(c => c.Key == "application/json")
                    .ToDictionary(c => c.Key, c => c.Value);
            }

            if (operation.RequestBody != null &&
                operation.RequestBody.Content.First().Key == "multipart/form-data")
            {
                operation.RequestBody.Content = operation.RequestBody.Content
                    .Where(c => c.Key == "multipart/form-data")
                    .ToDictionary(c => c.Key, c => c.Value);
            }
            else if (operation.RequestBody != null)
            {
                operation.RequestBody.Content = operation.RequestBody.Content
                    .Where(c => c.Key == "application/json")
                    .ToDictionary(c => c.Key, c => c.Value);
            }
        }
    }
}

public class AuthResponsesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context != default && context.MethodInfo != default && context.MethodInfo.DeclaringType != default)
        {
            if (operation.Responses.Any(r => r.Key == "404"))
            {
                operation.Responses.Remove("404");
                operation.Responses.Add("404", new OpenApiResponse { Description = "Not Found" });
            }

            if (operation.Responses.Any(r => r.Key == "401"))
            {
                operation.Responses.Remove("401");
                operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
            }

            if (operation.Responses.Any(r => r.Key == "403"))
            {
                operation.Responses.Remove("403");
                operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden (User must be a admin)" });
            }
        }
    }
}

public class AuthorizeCheckOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthorize = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>().Any() == true
            || context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

        var hasAllowAnonymous = context.MethodInfo.GetCustomAttributes(true)
            .OfType<AllowAnonymousAttribute>().Any();

        var authorizeAttributes = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>();

        var requiredRoles = authorizeAttributes
            .Where(a => !string.IsNullOrEmpty(a.Roles))
            .SelectMany(a => a.Roles.Split(','))
            .Distinct()
            .ToList();

        if (hasAuthorize && !hasAllowAnonymous)
        {
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference 
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            },
                            Scheme = "0auth2",
                            Name = JwtBearerDefaults.AuthenticationScheme,
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                }
            };

            var description = "This endpoint requires authentication. ";
            if (requiredRoles.Any())
            {
                description += $" Requires Admin user.";
            }

            operation.Description += "\n\n" + description;
        }
    }
}