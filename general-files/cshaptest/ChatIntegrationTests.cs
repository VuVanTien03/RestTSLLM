// File: ChatIntegrationTests.cs

using System.Net;
using System.Text.Json.Nodes;

namespace AIChatbotY1.IntegrationTests
{
    public sealed class ChatIntegrationTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
    {
        public ChatIntegrationTests(TestWebApplicationFactory factory) : base(factory)
        {
        }

        private async Task<HttpResponseMessage> CreateChatAsync(string? token, object body)
        {
            return await SendAsync(HttpMethod.Post, "/chats", token, body);
        }

        private async Task<HttpResponseMessage> GetChatsAsync(string? token, string userId)
        {
            return await SendAsync(HttpMethod.Get, $"/chats?userId={Uri.EscapeDataString(userId)}", token);
        }

        private async Task<HttpResponseMessage> GetChatByIdAsync(string? token, string chatId, string userId)
        {
            return await SendAsync(HttpMethod.Get, $"/chats/{Uri.EscapeDataString(chatId)}?userId={Uri.EscapeDataString(userId)}", token);
        }

        private async Task<HttpResponseMessage> DeleteChatAsync(string? token, string chatId, string userId)
        {
            return await SendAsync(HttpMethod.Delete, $"/chats/{Uri.EscapeDataString(chatId)}?userId={Uri.EscapeDataString(userId)}", token);
        }

        private async Task<HttpResponseMessage> RenameChatAsync(string? token, string chatId, string userId, object body)
        {
            return await SendAsync(new HttpMethod("PATCH"), $"/chats/{Uri.EscapeDataString(chatId)}?userId={Uri.EscapeDataString(userId)}", token, body);
        }

        private async Task<HttpResponseMessage> AddMessageAsync(string? token, string chatId, string userId, object body)
        {
            return await SendAsync(HttpMethod.Post, $"/chats/{Uri.EscapeDataString(chatId)}/messages?userId={Uri.EscapeDataString(userId)}", token, body);
        }

        private async Task<string> CreateChatIdAsync(string token, string userId, string? name = null)
        {
            var response = await CreateChatAsync(token, name is null
                ? new { userId }
                : new { userId, name });

            var body = await ReadJsonObjectAsync(response);
            var chatId = body?["chatId"]?.GetValue<string>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(string.IsNullOrWhiteSpace(chatId));

            return chatId!;
        }

        private async Task<(string UserId, string Token, string ChatId, string ChatName)> CreateChatFixtureAsync()
        {
            var userId = CreateUserId();
            var token = CreateToken(userId);
            var chatName = CreateChatName();
            var chatId = await CreateChatIdAsync(token, userId, chatName);

            return (userId, token, chatId, chatName);
        }

        private static void AssertChatResponse(JsonObject? body)
        {
            Assert.NotNull(body);
            Assert.True(body!["success"]?.GetValue<bool>() ?? false);
            Assert.False(string.IsNullOrWhiteSpace(body["message"]?.GetValue<string>()));
        }

        private static void AssertChatDetails(JsonObject? chat, string expectedChatId, string expectedName, string expectedUserId)
        {
            Assert.NotNull(chat);
            Assert.Equal(expectedChatId, chat!["_id"]?.GetValue<string>());
            Assert.Equal(expectedName, chat["name"]?.GetValue<string>());
            Assert.Equal(expectedUserId, chat["userId"]?.GetValue<string>());
            Assert.True(DateTimeOffset.TryParse(chat["createdAt"]?.GetValue<string>(), out _));
            Assert.True(DateTimeOffset.TryParse(chat["updatedAt"]?.GetValue<string>(), out _));
            Assert.NotNull(chat["messages"] as JsonArray);
        }

        [Fact]
        public async Task TC019_Create_Chat_When_UserId_Is_Valid_And_Name_Is_Omitted_Returns_OK()
        {
            var userId = CreateUserId();
            var token = CreateToken(userId);

            var response = await CreateChatAsync(token, new
            {
                userId
            });

            var body = await ReadJsonObjectAsync(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(body);
            Assert.True(body!["success"]?.GetValue<bool>() ?? false);
            Assert.False(string.IsNullOrWhiteSpace(body["message"]?.GetValue<string>()));
            Assert.False(string.IsNullOrWhiteSpace(body["chatId"]?.GetValue<string>()));
        }

        [Fact]
        public async Task TC020_Create_Chat_When_UserId_And_Custom_Name_Are_Valid_Returns_OK()
        {
            var userId = CreateUserId();
            var token = CreateToken(userId);
            var name = CreateChatName();

            var response = await CreateChatAsync(token, new
            {
                userId,
                name
            });

            var body = await ReadJsonObjectAsync(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(body);
            Assert.True(body!["success"]?.GetValue<bool>() ?? false);
            Assert.False(string.IsNullOrWhiteSpace(body["message"]?.GetValue<string>()));
            Assert.False(string.IsNullOrWhiteSpace(body["chatId"]?.GetValue<string>()));
        }

        [Fact]
        public async Task TC021_Create_Chat_When_UserId_Is_Missing_Returns_BadRequest()
        {
            var userId = CreateUserId();
            var token = CreateToken(userId);

            var response = await CreateChatAsync(token, new
            {
                name = "Project Alpha"
            });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task TC022_Create_Chat_When_UserId_Is_Null_Returns_BadRequest()
        {
            var userId = CreateUserId();
            var token = CreateToken(userId);

            var response = await CreateChatAsync(token, new
            {
                userId = (string?)null,
                name = "Project Alpha"
            });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task TC023_Create_Chat_When_Name_Is_Null_Returns_BadRequest()
        {
            var userId = CreateUserId();
            var token = CreateToken(userId);

            var response = await CreateChatAsync(token, new
            {
                userId,
                name = (string?)null
            });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task TC024_Create_Chat_When_Token_Is_Invalid_Returns_Unauthorized()
        {
            var response = await CreateChatAsync("invalidtoken", new
            {
                userId = "user123",
                name = "Project Alpha"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC025_Create_Chat_When_Without_Token_Returns_Unauthorized()
        {
            var response = await CreateChatAsync(null, new
            {
                userId = "user123",
                name = "Project Alpha"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC026_Get_Chats_When_User_Has_Chats_Returns_OK()
        {
            var fixture = await CreateChatFixtureAsync();

            var response = await GetChatsAsync(fixture.Token, fixture.UserId);
            var body = await ReadJsonObjectAsync(response);
            var data = body?["data"] as JsonArray;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(body);
            Assert.True(body!["success"]?.GetValue<bool>() ?? false);
            Assert.NotNull(data);
            Assert.True(data!.Any(item => item is JsonObject obj && obj["_id"]?.GetValue<string>() == fixture.ChatId));
        }

        [Fact]
        public async Task TC027_Get_Chats_When_UserId_Is_Missing_Returns_BadRequest()
        {
            var userId = CreateUserId();
            var token = CreateToken(userId);

            var response = await SendAsync(HttpMethod.Get, "/chats", token);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task TC028_Get_Chats_When_Token_Is_Invalid_Returns_Unauthorized()
        {
            var response = await GetChatsAsync("invalidtoken", "user123");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC029_Get_Chats_When_Without_Token_Returns_Unauthorized()
        {
            var response = await GetChatsAsync(null, "user123");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC030_Get_Chat_By_Id_When_Valid_Data_Returns_OK()
        {
            var fixture = await CreateChatFixtureAsync();

            var response = await GetChatByIdAsync(fixture.Token, fixture.ChatId, fixture.UserId);
            var body = await ReadJsonObjectAsync(response);
            var data = body?["data"] as JsonObject;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(body);
            Assert.True(body!["success"]?.GetValue<bool>() ?? false);
            AssertChatDetails(data, fixture.ChatId, fixture.ChatName, fixture.UserId);
        }

        [Fact]
        public async Task TC031_Get_Chat_By_Id_When_UserId_Is_Missing_Returns_BadRequest()
        {
            var fixture = await CreateChatFixtureAsync();

            var response = await SendAsync(HttpMethod.Get, $"/chats/{Uri.EscapeDataString(fixture.ChatId)}", fixture.Token);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task TC032_Get_Chat_By_Id_When_Token_Is_Invalid_Returns_Unauthorized()
        {
            var fixture = await CreateChatFixtureAsync();

            var response = await GetChatByIdAsync("invalidtoken", fixture.ChatId, fixture.UserId);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC033_Get_Chat_By_Id_When_Without_Token_Returns_Unauthorized()
        {
            var fixture = await CreateChatFixtureAsync();

            var response = await GetChatByIdAsync(null, fixture.ChatId, fixture.UserId);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC034_Delete_Chat_When_Valid_Data_Returns_OK()
        {
            var fixture = await CreateChatFixtureAsync();

            var response = await DeleteChatAsync(fixture.Token, fixture.ChatId, fixture.UserId);
            var body = await ReadJsonObjectAsync(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertChatResponse(body);
        }

        [Fact]
        public async Task TC035_Delete_Chat_When_UserId_Is_Missing_Returns_BadRequest()
        {
            var fixture = await CreateChatFixtureAsync();

            var response = await SendAsync(HttpMethod.Delete, $"/chats/{Uri.EscapeDataString(fixture.ChatId)}", fixture.Token);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task TC036_Delete_Chat_When_Token_Is_Invalid_Returns_Unauthorized()
        {
            var fixture = await CreateChatFixtureAsync();

            var response = await DeleteChatAsync("invalidtoken", fixture.ChatId, fixture.UserId);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC037_Delete_Chat_When_Without_Token_Returns_Unauthorized()
        {
            var fixture = await CreateChatFixtureAsync();

            var response = await DeleteChatAsync(null, fixture.ChatId, fixture.UserId);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC038_Rename_Chat_When_Valid_Data_Returns_OK()
        {
            var fixture = await CreateChatFixtureAsync();
            var newName = CreateChatName();

            var response = await RenameChatAsync(fixture.Token, fixture.ChatId, fixture.UserId, new
            {
                name = newName
            });

            var body = await ReadJsonObjectAsync(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertChatResponse(body);
        }

        [Fact]
        public async Task TC039_Rename_Chat_When_UserId_Is_Missing_Returns_BadRequest()
        {
            var fixture = await CreateChatFixtureAsync();
            var newName = CreateChatName();

            var response = await SendAsync(new HttpMethod("PATCH"), $"/chats/{Uri.EscapeDataString(fixture.ChatId)}", fixture.Token, new
            {
                name = newName
            });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task TC040_Rename_Chat_When_Name_Is_Missing_Returns_BadRequest()
        {
            var fixture = await CreateChatFixtureAsync();

            var response = await RenameChatAsync(fixture.Token, fixture.ChatId, fixture.UserId, new
            {
                userId = fixture.UserId
            });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task TC041_Rename_Chat_When_Name_Is_Null_Returns_BadRequest()
        {
            var fixture = await CreateChatFixtureAsync();

            var response = await RenameChatAsync(fixture.Token, fixture.ChatId, fixture.UserId, new
            {
                name = (string?)null
            });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task TC042_Rename_Chat_When_Token_Is_Invalid_Returns_Unauthorized()
        {
            var fixture = await CreateChatFixtureAsync();

            var response = await RenameChatAsync("invalidtoken", fixture.ChatId, fixture.UserId, new
            {
                name = CreateChatName()
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC043_Rename_Chat_When_Without_Token_Returns_Unauthorized()
        {
            var fixture = await CreateChatFixtureAsync();

            var response = await RenameChatAsync(null, fixture.ChatId, fixture.UserId, new
            {
                name = CreateChatName()
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC044_Add_Message_When_Valid_Data_Returns_OK()
        {
            var fixture = await CreateChatFixtureAsync();

            var response = await AddMessageAsync(fixture.Token, fixture.ChatId, fixture.UserId, new
            {
                message = new
                {
                    role = "user",
                    content = "Hello there",
                    timestamp = 1609459200000d
                }
            });

            var body = await ReadJsonObjectAsync(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertChatResponse(body);
        }

        [Fact]
        public async Task TC045_Add_Message_When_UserId_Is_Missing_Returns_BadRequest()
        {
            var fixture = await CreateChatFixtureAsync();

            var response = await SendAsync(HttpMethod.Post, $"/chats/{Uri.EscapeDataString(fixture.ChatId)}/messages", fixture.Token, new
            {
                message = new
                {
                    role = "user",
                    content = "Hello there",
                    timestamp = 1609459200000d
                }
            });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task TC046_Add_Message_When_Message_Is_Missing_Returns_BadRequest()
        {
            var fixture = await CreateChatFixtureAsync();

            var response = await AddMessageAsync(fixture.Token, fixture.ChatId, fixture.UserId, new
            {
                userId = fixture.UserId
            });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task TC047_Add_Message_When_Message_Is_Null_Returns_BadRequest()
        {
            var fixture = await CreateChatFixtureAsync();

            var response = await AddMessageAsync(fixture.Token, fixture.ChatId, fixture.UserId, new
            {
                message = (object?)null
            });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task TC048_Add_Message_When_Nested_Role_Is_Missing_Returns_BadRequest()
        {
            var fixture = await CreateChatFixtureAsync();

            var response = await AddMessageAsync(fixture.Token, fixture.ChatId, fixture.UserId, new
            {
                message = new
                {
                    content = "Hello there",
                    timestamp = 1609459200000d
                }
            });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task TC049_Add_Message_When_Nested_Content_Is_Missing_Returns_BadRequest()
        {
            var fixture = await CreateChatFixtureAsync();

            var response = await AddMessageAsync(fixture.Token, fixture.ChatId, fixture.UserId, new
            {
                message = new
                {
                    role = "user",
                    timestamp = 1609459200000d
                }
            });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task TC050_Add_Message_When_Nested_Timestamp_Is_Missing_Returns_BadRequest()
        {
            var fixture = await CreateChatFixtureAsync();

            var response = await AddMessageAsync(fixture.Token, fixture.ChatId, fixture.UserId, new
            {
                message = new
                {
                    role = "user",
                    content = "Hello there"
                }
            });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task TC051_Add_Message_When_Token_Is_Invalid_Returns_Unauthorized()
        {
            var fixture = await CreateChatFixtureAsync();

            var response = await AddMessageAsync("invalidtoken", fixture.ChatId, fixture.UserId, new
            {
                message = new
                {
                    role = "user",
                    content = "Hello there",
                    timestamp = 1609459200000d
                }
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC052_Add_Message_When_Without_Token_Returns_Unauthorized()
        {
            var fixture = await CreateChatFixtureAsync();

            var response = await AddMessageAsync(null, fixture.ChatId, fixture.UserId, new
            {
                message = new
                {
                    role = "user",
                    content = "Hello there",
                    timestamp = 1609459200000d
                }
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
