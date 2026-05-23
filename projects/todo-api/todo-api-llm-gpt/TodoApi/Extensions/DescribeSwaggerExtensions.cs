using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;

namespace TodoApi.Extensions
{
    public class TagDescriptionsDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Tags = new List<OpenApiTag>
            {
                new OpenApiTag { Name = "Users", Description = "Users management operations" },
                new OpenApiTag { Name = "Todos", Description = "Todo list management operations" },
            };
        }
    }

    public static class DescribeSwaggerExtensions
    {


        public static SwaggerGenOptions DescribeApi(this SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1",
                new OpenApiInfo
                {
                    Title = "Simple Todo API",
                    Description = "API to manage a simple Todo List with user auth control.",
                    Version = "v1",
                    Contact = new OpenApiContact()
                    {
                        Name = "David Fowler",
                        Url = new Uri("https://github.com/davidfowl"),
                    },
                    License = new OpenApiLicense()
                    {
                        Name = "MIT",
                        Url = new Uri("http://opensource.org/licenses/MIT"),
                    }
                });
            options.DocumentFilter<TagDescriptionsDocumentFilter>();
            options.InferSecuritySchemes();
            options.SupportNonNullableReferenceTypes();
            options.EnableAnnotations();

            return options;
        }

        public static IEndpointConventionBuilder DescribeApiSecurityRequirement(this IEndpointConventionBuilder builder)
        {
            var scheme = new OpenApiSecurityScheme()
            {
                Type = SecuritySchemeType.Http,
                Name = JwtBearerDefaults.AuthenticationScheme,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Reference = new()
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            };

            return builder.WithOpenApi(operation => new(operation)
            {
                Security =
            {
                new()
                {
                    [scheme] = new List<string>()
                }
            }
            });
        }

        public static RouteHandlerBuilder DescribeToken(this RouteHandlerBuilder builder)
        {
            builder.WithOpenApi(o =>
             {
                 o.Summary = "Authenticate user";
                 o.Description = "Generate a valid token to authenticate Todos endpoints by username and password.";
                 o.Responses["200"].Description = "Token generated successfully";
                 o.Responses["400"].Description = "Invalid credentials";
                 return o;
             });

            return builder;
        }

        public static RouteHandlerBuilder DescribeCreateUser(this RouteHandlerBuilder builder)
        {
            builder.WithOpenApi(o =>
            {
                o.Summary = "Create an user";
                o.Description = "This endpoint creates new user with an username and password.";
                o.Responses["200"].Description = "User created successfully";
                o.Responses["400"].Description = "Invalid fields or user already exists";
                return o;
            });

            return builder;
        }

        public static RouteHandlerBuilder DescribeCreateTodo(this RouteHandlerBuilder builder)
        {
            builder
                .Produces(401)
                .WithOpenApi(o =>
            {
                o.Summary = "Create a todo";
                o.Description = "This endpoint creates new todo with a simple title (description) always incomplete.";
                o.Responses["201"].Description = "Todo created successfully";
                o.Responses["400"].Description = "Invalid title";
                o.Responses["401"].Description = "Invalid token or missing";
                return o;
            });

            return builder;
        }

        public static RouteHandlerBuilder DescribeUpdateTodo(this RouteHandlerBuilder builder)
        {
            builder
                .Produces(401)
                .WithOpenApi(o =>
            {
                o.Summary = "Update a todo";
                o.Description = "This endpoint updates an existing todo with a simple title (description) and its status (isComplete). It can only be done by the user who created the todo and can also be used to mark or unmark a todo as complete.";
                o.Parameters[0].Description = "Id of an existing todo created by the user who owns the authentication token.";
                o.Parameters[0].AllowEmptyValue = false;
                o.Responses["200"].Description = "Todo updated successfully";
                o.Responses["400"].Description = "Invalid title or missing isComplete";
                o.Responses["401"].Description = "Invalid token or missing";
                o.Responses["404"].Description = "Todo does not exists or user is not the owner";
                return o;
            });

            return builder;
        }

        public static RouteHandlerBuilder DescribeDeleteTodo(this RouteHandlerBuilder builder)
        {
            builder
                .Produces(401)
                .WithOpenApi(o =>
            {
                o.Summary = "Delete a todo";
                o.Description = "This endpoint deletes an existing todo. It can only be done by the user who created the todo.";
                o.Parameters[0].Description = "Id of an existing todo created by the user who owns the authentication token.";
                o.Parameters[0].AllowEmptyValue = false;
                o.Responses["200"].Description = "Todo deleted successfully";
                o.Responses["401"].Description = "Invalid token or missing";
                o.Responses["404"].Description = "Todo does not exists or user is not the owner";
                return o;
            });

            return builder;
        }

        public static RouteHandlerBuilder DescribeListTodos(this RouteHandlerBuilder builder)
        {
            builder
                .Produces(401, typeof(void))
                .WithOpenApi(o =>
            {
                o.Summary = "List all todos";
                o.Description = "This endpoint list all existing todos that the user has created. If current user has no todos, an empty array will be returned.";
                o.Responses["200"].Description = "Todos listed successfully";
                o.Responses["401"].Description = "Invalid token or missing";
                o.Responses["401"].Content = null;
                return o;
            });

            return builder;
        }

        public static RouteHandlerBuilder DescribeGetTodo(this RouteHandlerBuilder builder)
        {
            builder
                .Produces(401)
                .WithOpenApi(o =>
            {
                o.Summary = "Get a todo";
                o.Description = "This endpoint consult an existing todo. It can only be done by the user who created the todo.";
                o.Parameters[0].Description = "Id of an existing todo created by the user who owns the authentication token.";
                o.Parameters[0].AllowEmptyValue = false;
                o.Responses["200"].Description = "Todo consulted successfully";
                o.Responses["401"].Description = "Invalid token or missing";
                o.Responses["404"].Description = "Todo does not exists or user is not the owner";
                return o;
            });

            return builder;
        }

        public class DictionaryDefaultAttribute : DefaultValueAttribute
        {
            public DictionaryDefaultAttribute(string key, string[] value)
                : base(new Dictionary<string, string[]>() { { key, value } })
            {
            }
        }
    }
}