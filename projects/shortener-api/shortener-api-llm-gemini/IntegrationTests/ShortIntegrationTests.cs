using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace IntegrationTests
{
    public class UrlShortenEndpointIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public UrlShortenEndpointIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<HttpResponseMessage> ShortenUrlAsync(string url, int? expireMinutes, bool hasQrCode)
        {
            var requestBody = new
            {
                url = url,
                expireMinutes = expireMinutes,
                hasQrCode = hasQrCode
            };
            return await _client.PostAsJsonAsync("/api/url-shorts", requestBody);
        }

        private async Task<HttpResponseMessage> GetUrlAsync(string token)
        {
            return await _client.GetAsync($"/{token}");
        }


        [Fact]
        public async Task TC001_Shorten_URL_With_Valid_Data_And_QR_Code_Enabled()
        {
            // arrange
            string url = "https://www.example.com";
            int? expireMinutes = null;
            bool hasQrCode = true;

            // act
            var response = await ShortenUrlAsync(url, expireMinutes, hasQrCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(body!["token"]?.AsValue()?.GetValue<string>());
            Assert.NotEmpty(body!["shortenedUrl"]?.AsValue()?.GetValue<string>());
            Assert.NotEmpty(body!["qrCode"]?.AsValue()?.GetValue<string>());
        }

        [Fact]
        public async Task TC002_Shorten_URL_With_Valid_Data_And_QR_Code_Disabled()
        {
            // arrange
            string url = "https://www.example.com";
            int? expireMinutes = null;
            bool hasQrCode = false;

            // act
            var response = await ShortenUrlAsync(url, expireMinutes, hasQrCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(body!["token"]?.AsValue()?.GetValue<string>());
            Assert.NotEmpty(body!["shortenedUrl"]?.AsValue()?.GetValue<string>());
            Assert.Null(body!["qrCode"]?.AsValue()?.GetValue<string>());
        }

        [Fact]
        public async Task TC003_Shorten_URL_With_Valid_Data_And_Expiration_Time()
        {
            // arrange
            string url = "https://www.example.com";
            int? expireMinutes = 60;
            bool hasQrCode = true;

            // act
            var response = await ShortenUrlAsync(url, expireMinutes, hasQrCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(body!["token"]?.AsValue()?.GetValue<string>());
            Assert.NotEmpty(body!["shortenedUrl"]?.AsValue()?.GetValue<string>());
            Assert.NotEmpty(body!["qrCode"]?.AsValue()?.GetValue<string>());
        }

        [Fact]
        public async Task TC004_Shorten_URL_With_Invalid_URL()
        {
            // arrange
            string url = "invalid-url";
            int? expireMinutes = null;
            bool hasQrCode = true;

            // act
            var response = await ShortenUrlAsync(url, expireMinutes, hasQrCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Shorten_URL_With_Empty_URL()
        {
            // arrange
            string url = "";
            int? expireMinutes = null;
            bool hasQrCode = true;

            // act
            var response = await ShortenUrlAsync(url, expireMinutes, hasQrCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Shorten_URL_With_Null_URL()
        {
            // arrange
            string url = null;
            int? expireMinutes = null;
            bool hasQrCode = true;

            // act
            var response = await ShortenUrlAsync(url, expireMinutes, hasQrCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Shorten_URL_With_Negative_expireMinutes()
        {
            // arrange
            string url = "https://www.example.com";
            int? expireMinutes = -60;
            bool hasQrCode = true;

            // act
            var response = await ShortenUrlAsync(url, expireMinutes, hasQrCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Shorten_URL_With_Zero_expireMinutes()
        {
            // arrange
            string url = "https://www.example.com";
            int? expireMinutes = 0;
            bool hasQrCode = true;

            // act
            var response = await ShortenUrlAsync(url, expireMinutes, hasQrCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Shorten_URL_With_Missing_url_Parameter()
        {
            // arrange
            int? expireMinutes = 60;
            bool hasQrCode = true;

            // act
            var response = await ShortenUrlAsync(null, expireMinutes, hasQrCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Shorten_URL_With_Missing_hasQrCode_Parameter()
        {
            // arrange
            string url = "https://www.example.com";
            int? expireMinutes = 60;

            // act
            var response = await ShortenUrlAsync(url, expireMinutes, false);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Get_URL_By_Valid_Token()
        {
            // arrange
            string url = "https://www.example.com";
            int? expireMinutes = null;
            bool hasQrCode = true;
            var shortenResponse = await ShortenUrlAsync(url, expireMinutes, hasQrCode);
            var shortenBody = await shortenResponse.Content.ReadFromJsonAsync<JsonObject>();
            string token = shortenBody!["token"]?.AsValue()?.GetValue<string>();

            // act
            var response = await GetUrlAsync(token);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(url, body!["url"]?.AsValue()?.GetValue<string>());

        }

        [Fact]
        public async Task TC012_Get_URL_By_Invalid_Token()
        {
            // arrange
            string token = "invalid-token";

            // act
            var response = await GetUrlAsync(token);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Get_URL_By_Empty_Token()
        {
            // arrange
            string token = "";

            // act
            var response = await GetUrlAsync(token);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Get_URL_By_Null_Token()
        {
            // arrange
            string token = null;

            // act
            var response = await GetUrlAsync(token);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

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
        public async Task TC015_Get_Test_Endpoint()
        {
            // arrange

            // act
            var response = await _client.GetAsync("/test");
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(body!["token"]?.AsValue()?.GetValue<string>());
        }
    }
}
