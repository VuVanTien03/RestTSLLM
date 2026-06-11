// File: TestInfrastructure.cs

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
using System.Net.Http.Json;

namespace AIChatbotY1.IntegrationTests
{
    public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.Replace(ServiceDescriptor.Singleton<IAuthenticationSchemeProvider, TestAuthenticationSchemeProvider>());

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    options.DefaultScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
            });
        }
    }

    public sealed class TestAuthenticationSchemeProvider : AuthenticationSchemeProvider
    {
        private static readonly AuthenticationScheme Scheme =
            new(TestAuthHandler.SchemeName, TestAuthHandler.SchemeName, typeof(TestAuthHandler));

        public TestAuthenticationSchemeProvider(IOptions<AuthenticationOptions> options) : base(options)
        {
        }

        public override Task<AuthenticationScheme?> GetSchemeAsync(string name)
        {
            return Task.FromResult<AuthenticationScheme?>(Scheme);
        }

        public override Task<IEnumerable<AuthenticationScheme>> GetAllSchemesAsync()
        {
            return Task.FromResult<IEnumerable<AuthenticationScheme>>(new[] { Scheme });
        }

        public override Task<AuthenticationScheme?> GetDefaultAuthenticateSchemeAsync()
        {
            return Task.FromResult<AuthenticationScheme?>(Scheme);
        }

        public override Task<AuthenticationScheme?> GetDefaultChallengeSchemeAsync()
        {
            return Task.FromResult<AuthenticationScheme?>(Scheme);
        }

        public override Task<AuthenticationScheme?> GetDefaultForbidSchemeAsync()
        {
            return Task.FromResult<AuthenticationScheme?>(Scheme);
        }

        public override Task<AuthenticationScheme?> GetDefaultSignInSchemeAsync()
        {
            return Task.FromResult<AuthenticationScheme?>(Scheme);
        }

        public override Task<AuthenticationScheme?> GetDefaultSignOutSchemeAsync()
        {
            return Task.FromResult<AuthenticationScheme?>(Scheme);
        }
    }

    public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "TestBearer";

        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(HeaderNames.Authorization, out var authorizationValues))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var authorization = authorizationValues.ToString();

            if (string.IsNullOrWhiteSpace(authorization) ||
                !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing bearer token"));
            }

            var token = authorization["Bearer ".Length..].Trim();

            if (string.IsNullOrWhiteSpace(token) ||
                token.Equals("invalidtoken", StringComparison.OrdinalIgnoreCase) ||
                !token.StartsWith("valid-", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid bearer token"));
            }

            var userId = token["valid-".Length..];

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid bearer token"));
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim("userId", userId),
                new Claim(ClaimTypes.Name, userId)
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    public abstract class IntegrationTestBase
    {
        protected readonly HttpClient Client;

        protected IntegrationTestBase(TestWebApplicationFactory factory)
        {
            Client = factory.CreateClient();
        }

        protected static string CreateUserId()
        {
            return $"user{Guid.NewGuid():N}";
        }

        protected static string CreateToken(string userId)
        {
            return $"valid-{userId}";
        }

        protected static string CreateChatName()
        {
            return $"Chat{Guid.NewGuid():N}";
        }

        protected async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, string? token = null, object? body = null)
        {
            var request = new HttpRequestMessage(method, url);

            if (body is not null)
            {
                request.Content = JsonContent.Create(body);
            }

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await Client.SendAsync(request);
        }

        protected static async Task<JsonObject?> ReadJsonObjectAsync(HttpResponseMessage response)
        {
            return await response.Content.ReadFromJsonAsync<JsonObject>();
        }

        protected static async Task<string> ReadContentAsync(HttpResponseMessage response)
        {
            return await response.Content.ReadAsStringAsync();
        }
    }
}
