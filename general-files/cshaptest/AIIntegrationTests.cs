// File: AIIntegrationTests.cs

using System.Net;
using System.Text.Json.Nodes;

namespace AIChatbotY1.IntegrationTests
{
    public sealed class AIIntegrationTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
    {
        public AIIntegrationTests(TestWebApplicationFactory factory) : base(factory)
        {
            
        }

        private async Task<HttpResponseMessage> CreateMcpQueryAsync(string? token, object body)
        {
            return await SendAsync(HttpMethod.Post, "/mcp/queries", token, body);
        }

        private async Task<HttpResponseMessage> StreamMcpQueryAsync(string? token, object body)
        {
            return await SendAsync(HttpMethod.Post, "/mcp/queries/stream", token, body);
        }

        private async Task<HttpResponseMessage> GetMcpStatusAsync(string? token)
        {
            return await SendAsync(HttpMethod.Get, "/mcp/status", token);
        }

        private static void AssertResponseHasNonEmptyString(JsonObject? body, string propertyName)
        {
            var value = body?[propertyName]?.GetValue<string>();
            Assert.False(string.IsNullOrWhiteSpace(value));
        }

        [Fact]
        public async Task TC001_Create_MCP_Query_When_Valid_Conversation_Returns_OK()
        {
            var userId = CreateUserId();
            var token = CreateToken(userId);

            var response = await CreateMcpQueryAsync(token, new
            {
                conversation = new[]
                {
                    new { role = "user", content = "Hello, chatbot." }
                }
            });

            var body = await ReadJsonObjectAsync(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertResponseHasNonEmptyString(body, "response");
        }

        [Fact]
        public async Task TC002_Create_MCP_Query_When_Multiple_Conversation_Items_Returns_OK()
        {
            var userId = CreateUserId();
            var token = CreateToken(userId);

            var response = await CreateMcpQueryAsync(token, new
            {
                conversation = new[]
                {
                    new { role = "user", content = "What is the weather?" },
                    new { role = "assistant", content = "I can help with that." }
                },
                IdDonVi = "DV001",
                max_iterations = 1
            });

            var body = await ReadJsonObjectAsync(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertResponseHasNonEmptyString(body, "response");
        }

        [Fact]
        public async Task TC003_Create_MCP_Query_When_IdDonVi_Is_Null_Returns_OK()
        {
            var userId = CreateUserId();
            var token = CreateToken(userId);

            var response = await CreateMcpQueryAsync(token, new
            {
                conversation = new[]
                {
                    new { role = "user", content = "Please summarize this." }
                },
                IdDonVi = (string?)null
            });

            var body = await ReadJsonObjectAsync(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertResponseHasNonEmptyString(body, "response");
        }

        [Fact]
        public async Task TC004_Create_MCP_Query_When_Conversation_Is_Missing_Returns_BadRequest()
        {
            var userId = CreateUserId();
            var token = CreateToken(userId);

            var response = await CreateMcpQueryAsync(token, new
            {
                max_iterations = 3
            });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_MCP_Query_When_Conversation_Item_Missing_Role_Returns_BadRequest()
        {
            var userId = CreateUserId();
            var token = CreateToken(userId);

            var response = await CreateMcpQueryAsync(token, new
            {
                conversation = new[]
                {
                    new { content = "Hello" }
                }
            });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Create_MCP_Query_When_Conversation_Item_Missing_Content_Returns_BadRequest()
        {
            var userId = CreateUserId();
            var token = CreateToken(userId);

            var response = await CreateMcpQueryAsync(token, new
            {
                conversation = new[]
                {
                    new { role = "user" }
                }
            });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Create_MCP_Query_When_Conversation_Role_Is_Null_Returns_BadRequest()
        {
            var userId = CreateUserId();
            var token = CreateToken(userId);

            var response = await CreateMcpQueryAsync(token, new
            {
                conversation = new[]
                {
                    new { role = (string?)null, content = "Hello" }
                }
            });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_MCP_Query_When_Conversation_Content_Is_Null_Returns_BadRequest()
        {
            var userId = CreateUserId();
            var token = CreateToken(userId);

            var response = await CreateMcpQueryAsync(token, new
            {
                conversation = new[]
                {
                    new { role = "user", content = (string?)null }
                }
            });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Create_MCP_Query_When_Token_Is_Invalid_Returns_Unauthorized()
        {
            var response = await CreateMcpQueryAsync("invalidtoken", new
            {
                conversation = new[]
                {
                    new { role = "user", content = "Hello" }
                }
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Create_MCP_Query_When_Without_Token_Returns_Unauthorized()
        {
            var response = await CreateMcpQueryAsync(null, new
            {
                conversation = new[]
                {
                    new { role = "user", content = "Hello" }
                }
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Stream_MCP_Query_When_Valid_Conversation_Returns_OK()
        {
            var userId = CreateUserId();
            var token = CreateToken(userId);

            var response = await StreamMcpQueryAsync(token, new
            {
                conversation = new[]
                {
                    new { role = "user", content = "Tell me a joke." }
                }
            });

            var body = await ReadContentAsync(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(string.IsNullOrWhiteSpace(body));
        }

        [Fact]
        public async Task TC012_Stream_MCP_Query_When_Multiple_Conversation_Items_Returns_OK()
        {
            var userId = CreateUserId();
            var token = CreateToken(userId);

            var response = await StreamMcpQueryAsync(token, new
            {
                conversation = new[]
                {
                    new { role = "user", content = "Summarize the topic." },
                    new { role = "assistant", content = "Sure, here is a summary." }
                },
                IdDonVi = "DV001",
                max_iterations = 5
            });

            var body = await ReadContentAsync(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(string.IsNullOrWhiteSpace(body));
        }

        [Fact]
        public async Task TC013_Stream_MCP_Query_When_Conversation_Is_Missing_Returns_BadRequest()
        {
            var userId = CreateUserId();
            var token = CreateToken(userId);

            var response = await StreamMcpQueryAsync(token, new
            {
                IdDonVi = "DV001"
            });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Stream_MCP_Query_When_Token_Is_Invalid_Returns_Unauthorized()
        {
            var response = await StreamMcpQueryAsync("invalidtoken", new
            {
                conversation = new[]
                {
                    new { role = "user", content = "Hello" }
                }
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Stream_MCP_Query_When_Without_Token_Returns_Unauthorized()
        {
            var response = await StreamMcpQueryAsync(null, new
            {
                conversation = new[]
                {
                    new { role = "user", content = "Hello" }
                }
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC016_MCP_Status_When_Token_Is_Valid_Returns_OK()
        {
            var userId = CreateUserId();
            var token = CreateToken(userId);

            var response = await GetMcpStatusAsync(token);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC017_MCP_Status_When_Token_Is_Invalid_Returns_Unauthorized()
        {
            var response = await GetMcpStatusAsync("invalidtoken");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC018_MCP_Status_When_Without_Token_Returns_Unauthorized()
        {
            var response = await GetMcpStatusAsync(null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
