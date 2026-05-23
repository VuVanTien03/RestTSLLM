using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit;

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

        private async Task<JsonDocument> CreateShortUrlAsync(object requestBody)
        {
            var response = await _client.PostAsJsonAsync("/api/url-shorts", requestBody);
            return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        }

        [Fact]
        public async Task TC001_Redirect_Valid_Token_Returns_OK()
        {
            var request = new { url = $"https://example.com/{Guid.NewGuid()}", hasQrCode = true };
            var createResponse = await CreateShortUrlAsync(request);
            var token = createResponse.RootElement.GetProperty("token").GetString();

            var redirectResponse = await _client.GetAsync($"/{token}");
            var redirectContent = await JsonDocument.ParseAsync(await redirectResponse.Content.ReadAsStreamAsync());

            Assert.Equal(HttpStatusCode.OK, redirectResponse.StatusCode);
            Assert.Equal(request.url, redirectContent.RootElement.GetProperty("url").GetString());
        }

        [Fact]
        public async Task TC002_Redirect_NonExistent_Token_Returns_NotFound()
        {
            var response = await _client.GetAsync($"/nonexistenttoken-{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Redirect_Empty_Token_Returns_BadRequest()
        {
            var response = await _client.GetAsync("/");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Redirect_Invalid_Token_Format_Returns_BadRequest()
        {
            var response = await _client.GetAsync("/invalid!@token");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_ShortUrl_Valid_Data_Returns_OK()
        {
            var request = new
            {
                url = $"https://valid-{Guid.NewGuid()}.com",
                hasQrCode = true
            };

            var doc = await CreateShortUrlAsync(request);

            Assert.NotNull(doc.RootElement.GetProperty("token").GetString());
            Assert.Contains("http", doc.RootElement.GetProperty("shortenedUrl").GetString());
            Assert.False(string.IsNullOrEmpty(doc.RootElement.GetProperty("qrCode").GetString()));
        }

        [Fact]
        public async Task TC006_Create_ShortUrl_Invalid_Url_Returns_BadRequest()
        {
            var request = new { url = "invalid-url", hasQrCode = true };
            var response = await _client.PostAsJsonAsync("/api/url-shorts", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(400, (await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync())).RootElement.GetProperty("statusCode").GetInt32());
        }

        [Fact]
        public async Task TC007_Create_ShortUrl_Empty_Url_Returns_BadRequest()
        {
            var request = new { url = "", hasQrCode = true };
            var response = await _client.PostAsJsonAsync("/api/url-shorts", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_ShortUrl_Min_ExpireMinutes_Returns_OK()
        {
            var request = new
            {
                url = $"https://min-expire-{Guid.NewGuid()}.com",
                expireMinutes = 1,
                hasQrCode = false
            };

            var doc = await CreateShortUrlAsync(request);

            Assert.Null(doc.RootElement.GetProperty("qrCode").GetString());
        }

        [Fact]
        public async Task TC009_Create_ShortUrl_Null_ExpireMinutes_Returns_OK()
        {
            var request = new
            {
                url = $"https://null-expire-{Guid.NewGuid()}.com",
                expireMinutes = (int?)null,
                hasQrCode = true
            };

            var doc = await CreateShortUrlAsync(request);

            Assert.False(string.IsNullOrEmpty(doc.RootElement.GetProperty("qrCode").GetString()));
        }

        [Fact]
        public async Task TC010_Create_ShortUrl_Invalid_ExpireMinutes_Returns_BadRequest()
        {
            var request = new
            {
                url = "https://invalid-expire.com",
                expireMinutes = 0,
                hasQrCode = false
            };

            var response = await _client.PostAsJsonAsync("/api/url-shorts", request);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Create_ShortUrl_Missing_Required_Fields_Returns_BadRequest()
        {
            var response = await _client.PostAsJsonAsync("/api/url-shorts", new { });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Create_ShortUrl_Extra_Properties_Returns_BadRequest()
        {
            var request = new
            {
                url = "https://extra-fields.com",
                hasQrCode = true,
                invalidField = "value"
            };

            var response = await _client.PostAsJsonAsync("/api/url-shorts", request);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Create_ShortUrl_QrCode_Generated_When_Enabled()
        {
            var request = new
            {
                url = $"https://qrcode-test-{Guid.NewGuid()}.com",
                hasQrCode = true
            };

            var doc = await CreateShortUrlAsync(request);
            Assert.NotNull(doc.RootElement.GetProperty("qrCode").GetString());
        }

        [Fact]
        public async Task TC014_Create_ShortUrl_QrCode_Null_When_Disabled()
        {
            var request = new
            {
                url = $"https://noqrcode-{Guid.NewGuid()}.com",
                hasQrCode = false
            };

            var doc = await CreateShortUrlAsync(request);
            Assert.Null(doc.RootElement.GetProperty("qrCode").GetString());
        }

        [Fact]
        public async Task TC015_Test_Endpoint_Returns_Valid_Response()
        {
            var response = await _client.GetAsync("/test");
            var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

            Assert.NotNull(doc.RootElement.GetProperty("token").GetString());
        }

        [Fact]
        public async Task TC016_Create_ShortUrl_Max_Url_Length_Returns_OK()
        {
            var longPath = new string('a', 2040);
            var request = new
            {
                url = $"https://example.com/{longPath}",
                hasQrCode = true
            };

            var response = await _client.PostAsJsonAsync("/api/url-shorts", request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Redirect_Expired_Token_Returns_NotFound()
        {
            var request = new
            {
                url = $"https://expired-test-{Guid.NewGuid()}.com",
                expireMinutes = 1,
                hasQrCode = false
            };

            await Task.Delay(61000); // Wait 61 seconds
            var token = (await CreateShortUrlAsync(request)).RootElement.GetProperty("token").GetString();

            var response = await _client.GetAsync($"/{token}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC018_Create_ShortUrl_Special_Characters_Returns_OK()
        {
            var request = new
            {
                url = "https://exemplo.com/path?query=param&value=áéíóú",
                hasQrCode = false
            };

            var response = await _client.PostAsJsonAsync("/api/url-shorts", request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC019_Create_ShortUrl_Invalid_Protocol_Returns_BadRequest()
        {
            var request = new
            {
                url = "ftp://invalid-protocol.com",
                hasQrCode = true
            };

            var response = await _client.PostAsJsonAsync("/api/url-shorts", request);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC020_Verify_Shortened_Url_Format()
        {
            var request = new
            {
                url = $"https://url-format-test-{Guid.NewGuid()}.com",
                hasQrCode = false
            };

            var doc = await CreateShortUrlAsync(request);
            var shortenedUrl = doc.RootElement.GetProperty("shortenedUrl").GetString();

            Assert.Matches(new Regex(@"http(s)?://.*/\w+"), shortenedUrl);
        }
    }
}