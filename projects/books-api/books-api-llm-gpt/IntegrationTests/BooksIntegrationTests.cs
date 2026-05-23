// File: BookIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace IntegrationTests
{
    public class BookIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public BookIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private async Task<HttpResponseMessage> CreateBookAsync(string title, string isbn, string description, string author)
        {
            var request = new
            {
                title = title,
                isbn = isbn,
                description = description,
                author = author
            };

            return await _client.PostAsJsonAsync("/books", request);
        }

        private async Task<int> CreateBookAndGetIdAsync(string title, string isbn, string description, string author)
        {
            var response = await CreateBookAsync(title, isbn, description, author);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].GetValue<int>();
        }

        private async Task<HttpResponseMessage> GetBookAsync(int id)
        {
            return await _client.GetAsync($"/books/{id}");
        }

        private async Task<HttpResponseMessage> UpdateBookAsync(int id, string title, string isbn, string description, string author)
        {
            var request = new
            {
                title = title,
                isbn = isbn,
                description = description,
                author = author
            };

            return await _client.PutAsJsonAsync($"/books/{id}", request);
        }

        private async Task<HttpResponseMessage> DeleteBookAsync(int id)
        {
            return await _client.DeleteAsync($"/books/{id}");
        }

        [Fact]
        public async Task TC001_Get_All_Books_When_Books_Exist_Returns_OK()
        {
            // arrange
            await CreateBookAsync("Sample Title", "1234567890", "Sample Description", "John Doe");

            // act
            var response = await _client.GetAsync("/books");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonNode>();
            Assert.NotNull(body);
            Assert.True(body.AsArray().Count > 0);
        }

        [Fact]
        public async Task TC002_Create_Book_With_Valid_Data_Returns_OK()
        {
            // arrange
            var title = "Valid Title";
            var isbn = "1234567890";
            var description = "Valid Description";
            var author = "John Doe";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body);
            Assert.True(body.ContainsKey("id"));
        }

        [Fact]
        public async Task TC003_Create_Book_With_Missing_Title_Returns_BadRequest()
        {
            // arrange
            var isbn = "1234567890";
            var description = "Valid Description";
            var author = "John Doe";

            // act
            var response = await CreateBookAsync(null, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_Book_With_Empty_Title_Returns_BadRequest()
        {
            // arrange
            var title = "";
            var isbn = "1234567890";
            var description = "Valid Description";
            var author = "John Doe";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_Book_With_Missing_ISBN_Returns_BadRequest()
        {
            // arrange
            var title = "Valid Title";
            var description = "Valid Description";
            var author = "John Doe";

            // act
            var response = await CreateBookAsync(title, null, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Create_Book_With_Empty_ISBN_Returns_BadRequest()
        {
            // arrange
            var title = "Valid Title";
            var isbn = "";
            var description = "Valid Description";
            var author = "John Doe";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Create_Book_With_Missing_Description_Returns_BadRequest()
        {
            // arrange
            var title = "Valid Title";
            var isbn = "1234567890";
            var author = "John Doe";

            // act
            var response = await CreateBookAsync(title, isbn, null, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_Book_With_Empty_Description_Returns_BadRequest()
        {
            // arrange
            var title = "Valid Title";
            var isbn = "1234567890";
            var description = "";
            var author = "John Doe";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Create_Book_With_Missing_Author_Returns_BadRequest()
        {
            // arrange
            var title = "Valid Title";
            var isbn = "1234567890";
            var description = "Valid Description";

            // act
            var response = await CreateBookAsync(title, isbn, description, null);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Create_Book_With_Empty_Author_Returns_BadRequest()
        {
            // arrange
            var title = "Valid Title";
            var isbn = "1234567890";
            var description = "Valid Description";
            var author = "";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Get_Book_By_ID_When_Book_Exists_Returns_OK()
        {
            // arrange
            var bookId = await CreateBookAndGetIdAsync("Title 1", "ISBN1", "Description 1", "Author 1");

            // act
            var response = await GetBookAsync(bookId);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body);
            Assert.NotNull(body["id"].GetValue<int>());
        }

        [Fact]
        public async Task TC012_Get_Book_By_ID_When_Book_Does_Not_Exist_Returns_NotFound()
        {
            // act
            var response = await GetBookAsync(999);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Update_Book_With_Valid_Data_Returns_NoContent()
        {
            // arrange
            var bookId = await CreateBookAndGetIdAsync("Title 2", "ISBN2", "Description 2", "Author 2");

            // act
            var response = await UpdateBookAsync(bookId, "Updated Title", "Updated ISBN", "Updated Description", "Updated Author");

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Update_Book_When_Book_Does_Not_Exist_Returns_NotFound()
        {
            // act
            var response = await UpdateBookAsync(999, "Updated Title", "Updated ISBN", "Updated Description", "Updated Author");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Update_Book_With_Invalid_Data_Returns_BadRequest()
        {
            // arrange
            var bookId = await CreateBookAndGetIdAsync("Title 3", "ISBN3", "Description 3", "Author 3");

            // act
            var response = await UpdateBookAsync(bookId, "", "Updated ISBN", "Updated Description", "Updated Author");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC016_Delete_Book_By_ID_When_Book_Exists_Returns_NoContent()
        {
            // arrange
            var bookId = await CreateBookAndGetIdAsync("Title 4", "ISBN4", "Description 4", "Author 4");

            // act
            var response = await DeleteBookAsync(bookId);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Delete_Book_By_ID_When_Book_Does_Not_Exist_Returns_NotFound()
        {
            // act
            var response = await DeleteBookAsync(999);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
