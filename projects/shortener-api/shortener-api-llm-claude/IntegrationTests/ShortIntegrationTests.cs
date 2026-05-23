using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests
{
    public class WebIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public WebIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task TC001_Get_Test_Token_Returns_OK()
        {
            // act
            var response = await _client.GetAsync("/test");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var token = body["token"].AsValue().GetValue<string>();
            Assert.True(token == null || !string.IsNullOrEmpty(token));
        }
    }

    public class UrlShortenIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public UrlShortenIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<JsonObject> CreateShortUrlAndGetResponseAsync(string url, int? expireMinutes, bool hasQrCode)
        {
            var requestBody = new
            {
                url = url,
                expireMinutes = expireMinutes,
                hasQrCode = hasQrCode
            };

            var response = await _client.PostAsJsonAsync("/api/url-shorts", requestBody);
            return await response.Content.ReadFromJsonAsync<JsonObject>();
        }

        [Fact]
        public async Task TC002_Get_URL_When_Token_Exists_Returns_OK()
        {
            // arrange
            var originalUrl = "https://valid.com";
            var shortUrlResponse = await CreateShortUrlAndGetResponseAsync(originalUrl, 60, false);
            var token = shortUrlResponse["token"].AsValue().GetValue<string>();

            // act
            var response = await _client.GetAsync($"/{token}");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(originalUrl, body["url"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC003_Get_URL_When_Token_Not_Found_Returns_NotFound()
        {
            // act
            var response = await _client.GetAsync("/invalid_token");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(404, body["statusCode"].AsValue().GetValue<int>());
        }

        [Fact]
        public async Task TC004_Create_Short_URL_When_Valid_Data_With_QR_Code_Returns_OK()
        {
            // arrange
            var requestBody = new
            {
                url = "https://valid.com",
                expireMinutes = 60,
                hasQrCode = true
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/url-shorts", requestBody);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotEmpty(body["token"].AsValue().GetValue<string>());
            Assert.NotEmpty(body["shortenedUrl"].AsValue().GetValue<string>());
            Assert.NotEmpty(body["qrCode"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC005_Create_Short_URL_When_Valid_Data_Without_QR_Code_Returns_OK()
        {
            // arrange
            var requestBody = new
            {
                url = "https://valid.com",
                expireMinutes = 60,
                hasQrCode = false
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/url-shorts", requestBody);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotEmpty(body["token"].AsValue().GetValue<string>());
            Assert.NotEmpty(body["shortenedUrl"].AsValue().GetValue<string>());
            Assert.Null(body["qrCode"]);
        }

        [Fact]
        public async Task TC006_Create_Short_URL_When_Valid_Data_Without_Expiry_Returns_OK()
        {
            // arrange
            var requestBody = new
            {
                url = "https://valid.com",
                expireMinutes = (int?)null,
                hasQrCode = false
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/url-shorts", requestBody);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotEmpty(body["token"].AsValue().GetValue<string>());
            Assert.NotEmpty(body["shortenedUrl"].AsValue().GetValue<string>());
            Assert.Null(body["qrCode"]);
        }

        [Fact]
        public async Task TC007_Create_Short_URL_When_URL_Is_Null_Returns_BadRequest()
        {
            // arrange
            var requestBody = new
            {
                url = (string)null,
                expireMinutes = 60,
                hasQrCode = false
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/url-shorts", requestBody);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(400, body["statusCode"].AsValue().GetValue<int>());
        }

        [Fact]
        public async Task TC008_Create_Short_URL_When_URL_Is_Empty_Returns_BadRequest()
        {
            // arrange
            var requestBody = new
            {
                url = "",
                expireMinutes = 60,
                hasQrCode = false
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/url-shorts", requestBody);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(400, body["statusCode"].AsValue().GetValue<int>());
        }

        [Fact]
        public async Task TC009_Create_Short_URL_When_URL_Is_Invalid_Returns_BadRequest()
        {
            // arrange
            var requestBody = new
            {
                url = "invalid-url",
                expireMinutes = 60,
                hasQrCode = false
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/url-shorts", requestBody);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(400, body["statusCode"].AsValue().GetValue<int>());
        }

        [Fact]
        public async Task TC010_Create_Short_URL_When_Expire_Minutes_Is_Zero_Returns_BadRequest()
        {
            // arrange
            var requestBody = new
            {
                url = "https://valid.com",
                expireMinutes = 0,
                hasQrCode = false
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/url-shorts", requestBody);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(400, body["statusCode"].AsValue().GetValue<int>());
        }

        [Fact]
        public async Task TC011_Create_Short_URL_When_Expire_Minutes_Is_Negative_Returns_BadRequest()
        {
            // arrange
            var requestBody = new
            {
                url = "https://valid.com",
                expireMinutes = -1,
                hasQrCode = false
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/url-shorts", requestBody);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(400, body["statusCode"].AsValue().GetValue<int>());
        }

        [Fact]
        public async Task TC012_Create_Short_URL_When_HasQrCode_Is_Null_Returns_BadRequest()
        {
            // arrange
            var requestBody = new
            {
                url = "https://valid.com",
                expireMinutes = 60,
                hasQrCode = (bool?)null
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/url-shorts", requestBody);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(400, body["statusCode"].AsValue().GetValue<int>());
        }

        [Fact]
        public async Task TC013_Get_URL_When_Token_Is_Expired_Returns_NotFound()
        {
            // arrange
            var requestBody = new
            {
                url = "https://valid.com",
                expireMinutes = 1, // 1 minute expiration
                hasQrCode = false
            };
            var createResponse = await _client.PostAsJsonAsync("/api/url-shorts", requestBody);
            var shortUrlResponse = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var token = shortUrlResponse["token"].AsValue().GetValue<string>();
            await Task.Delay(TimeSpan.FromSeconds(61)); // wait for expiration

            // act
            var response = await _client.GetAsync($"/{token}");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(404, body["statusCode"].AsValue().GetValue<int>());
        }
    }
}
