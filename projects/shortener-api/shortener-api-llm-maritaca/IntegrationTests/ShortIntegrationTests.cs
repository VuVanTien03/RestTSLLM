// File: UrlShortenerIntegrationTests.cs

using System.Net.Http.Json;
using System.Net;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTests
{
    public class UrlShortenerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public UrlShortenerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<string> CreateShortenedUrlAsync(string url, int? expireMinutes = null, bool hasQrCode = false)
        {
            var request = new
            {
                url = url,
                expireMinutes = expireMinutes,
                hasQrCode = hasQrCode
            };

            var response = await _client.PostAsJsonAsync("/api/url-shorts", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ShortUrlResponse>();
            return result!.token;
        }

        [Fact]
        public async Task TC001_RetrieveShortenedURLWhenValidTokenReturnsOK()
        {
            // arrange
            var token = await CreateShortenedUrlAsync("https://www.example.com");

            // act
            var response = await _client.GetAsync($"/{token}");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<UrlResult>();
            Assert.NotNull(body);
            Assert.NotNull(body.url);
        }

        [Fact]
        public async Task TC002_RetrieveShortenedURLWhenInvalidTokenReturnsNotFound()
        {
            // act
            var response = await _client.GetAsync($"/invalidtoken123");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<BaseResult>();
            Assert.NotNull(body);
            Assert.NotNull(body.message);
        }

        [Fact]
        public async Task TC003_CreateShortenedURLWhenValidDataReturnsOK()
        {
            // arrange
            var url = "https://www.example.com";

            // act
            var response = await _client.PostAsJsonAsync("/api/url-shorts", new { url = url, expireMinutes = (int?) null, hasQrCode = true });

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<ShortUrlResponse>();
            Assert.NotNull(body);
            Assert.NotNull(body.token);
            Assert.NotNull(body.shortenedUrl);
            Assert.NotNull(body.qrCode);
        }

        [Fact]
        public async Task TC004_CreateShortenedURLWhenInvalidURLReturnsBadRequest()
        {
            // act
            var response = await _client.PostAsJsonAsync("/api/url-shorts", new { url = "invalidurl", expireMinutes = 60, hasQrCode = false });

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<BaseResult>();
            Assert.NotNull(body);
            Assert.NotNull(body.message);
        }

        [Fact]
        public async Task TC005_CreateShortenedURLWhenMissingRequiredFieldReturnsBadRequest()
        {
            // act
            var response = await _client.PostAsJsonAsync("/api/url-shorts", new { expireMinutes = 60, hasQrCode = false });

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<BaseResult>();
            Assert.NotNull(body);
            Assert.NotNull(body.message);
        }

        [Fact]
        public async Task TC006_CreateShortenedURLWhenExpireMinutesIsOneReturnsOK()
        {
            // act
            var response = await _client.PostAsJsonAsync("/api/url-shorts", new { url = "https://www.example.com", expireMinutes = 1, hasQrCode = true });

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<ShortUrlResponse>();
            Assert.NotNull(body);
            Assert.NotNull(body.token);
            Assert.NotNull(body.shortenedUrl);
            Assert.NotNull(body.qrCode);
        }

        [Fact]
        public async Task TC007_CreateShortenedURLWhenExpireMinutesNullReturnsOK()
        {
            // act
            var response = await _client.PostAsJsonAsync("/api/url-shorts", new { url = "https://www.example.com", expireMinutes = (int?) null, hasQrCode = false });

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<ShortUrlResponse>();
            Assert.NotNull(body);
            Assert.NotNull(body.token);
            Assert.NotNull(body.shortenedUrl);
            Assert.Null(body.qrCode);
        }

        [Fact]
        public async Task TC008_TestEndpointWhenValidReturnsOK()
        {
            // act
            var response = await _client.GetAsync("/test");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<OnlyToken>();
            Assert.NotNull(body);
            Assert.NotNull(body.token);
        }
    }

    public class ShortUrlResponse
    {
        public string? token { get; set; }
        public string? shortenedUrl { get; set; }
        public string? qrCode { get; set; }
    }

    public class UrlResult
    {
        public string? url { get; set; }
    }

    public class BaseResult
    {
        public string? message { get; set; }
        public int statusCode { get; set; }
    }

    public class OnlyToken
    {
        public string? token { get; set; }
    }
}