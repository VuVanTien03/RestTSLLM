// File: BookIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace IntegrationTests
{
    public class BookIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public BookIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
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
        public async Task TC001_Get_All_Books_Returns_OK()
        {
            // arrange
            var response1 = await CreateBookAsync("Valid Title 1", "Valid ISBN 1", "Valid Description 1", "Valid Author 1");
            var response2 = await CreateBookAsync("Valid Title 2", "Valid ISBN 2", "Valid Description 2", "Valid Author 2");

            // act
            var response = await _client.GetAsync("/books");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonArray>();
            Assert.True(body.Count >= 2);
        }

        [Fact]
        public async Task TC002_Create_Book_When_Valid_Data_Returns_OK()
        {
            // arrange
            string title = "Valid Title";
            string isbn = "Valid ISBN";
            string description = "Valid Description";
            string author = "Valid Author";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_id = body["id"].AsValue().GetValue<int>();
            var body_title = body["title"].AsValue().GetValue<string>();
            var body_isbn = body["isbn"].AsValue().GetValue<string>();
            var body_description = body["description"].AsValue().GetValue<string>();
            var body_author = body["author"].AsValue().GetValue<string>();
            Assert.True(body_id > 0);
            Assert.Equal(title, body_title);
            Assert.Equal(isbn, body_isbn);
            Assert.Equal(description, body_description);
            Assert.Equal(author, body_author);
        }

        [Fact]
        public async Task TC003_Create_Book_When_Title_Is_Null_Returns_BadRequest()
        {
            // arrange
            string title = null;
            string isbn = "Valid ISBN";
            string description = "Valid Description";
            string author = "Valid Author";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_Book_When_Title_Is_Empty_Returns_BadRequest()
        {
            // arrange
            string title = "";
            string isbn = "Valid ISBN";
            string description = "Valid Description";
            string author = "Valid Author";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_Book_When_ISBN_Is_Null_Returns_BadRequest()
        {
            // arrange
            string title = "Valid Title";
            string isbn = null;
            string description = "Valid Description";
            string author = "Valid Author";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Create_Book_When_ISBN_Is_Empty_Returns_BadRequest()
        {
            // arrange
            string title = "Valid Title";
            string isbn = "";
            string description = "Valid Description";
            string author = "Valid Author";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Create_Book_When_Description_Is_Null_Returns_BadRequest()
        {
            // arrange
            string title = "Valid Title";
            string isbn = "Valid ISBN";
            string description = null;
            string author = "Valid Author";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_Book_When_Description_Is_Empty_Returns_BadRequest()
        {
            // arrange
            string title = "Valid Title";
            string isbn = "Valid ISBN";
            string description = "";
            string author = "Valid Author";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Create_Book_When_Author_Is_Null_Returns_BadRequest()
        {
            // arrange
            string title = "Valid Title";
            string isbn = "Valid ISBN";
            string description = "Valid Description";
            string author = null;

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Create_Book_When_Author_Is_Empty_Returns_BadRequest()
        {
            // arrange
            string title = "Valid Title";
            string isbn = "Valid ISBN";
            string description = "Valid Description";
            string author = "";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Get_Book_By_Valid_ID_Returns_OK()
        {
            // arrange
            string title = "Valid Title";
            string isbn = "Valid ISBN";
            string description = "Valid Description";
            string author = "Valid Author";
            var responseCreate = await CreateBookAsync(title, isbn, description, author);
            var body = await responseCreate.Content.ReadFromJsonAsync<JsonObject>();
            var book_id = body["id"].AsValue().GetValue<int>();

            // act
            var response = await GetBookAsync(book_id);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var bodyGet = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_id = bodyGet["id"].AsValue().GetValue<int>();
            var body_title = bodyGet["title"].AsValue().GetValue<string>();
            var body_isbn = bodyGet["isbn"].AsValue().GetValue<string>();
            var body_description = bodyGet["description"].AsValue().GetValue<string>();
            var body_author = bodyGet["author"].AsValue().GetValue<string>();
            Assert.Equal(book_id, body_id);
            Assert.Equal(title, body_title);
            Assert.Equal(isbn, body_isbn);
            Assert.Equal(description, body_description);
            Assert.Equal(author, body_author);
        }

        [Fact]
        public async Task TC012_Get_Book_By_Invalid_ID_Returns_NotFound()
        {
            // arrange
            int invalid_id = 9999999;

            // act
            var response = await GetBookAsync(invalid_id);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Update_Book_When_Valid_Data_Returns_NoContent()
        {
            // arrange
            string old_title = "Old Title";
            string old_isbn = "Old ISBN";
            string old_description = "Old Description";
            string old_author = "Old Author";
            var responseCreate = await CreateBookAsync(old_title, old_isbn, old_description, old_author);
            var body = await responseCreate.Content.ReadFromJsonAsync<JsonObject>();
            var book_id = body["id"].AsValue().GetValue<int>();

            string new_title = "New Title";
            string new_isbn = "New ISBN";
            string new_description = "New Description";
            string new_author = "New Author";

            // act
            var response = await UpdateBookAsync(book_id, new_title, new_isbn, new_description, new_author);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Update_Book_When_Title_Is_Null_Returns_BadRequest()
        {
            // arrange
            string old_title = "Old Title";
            string old_isbn = "Old ISBN";
            string old_description = "Old Description";
            string old_author = "Old Author";
            var responseCreate = await CreateBookAsync(old_title, old_isbn, old_description, old_author);
            var body = await responseCreate.Content.ReadFromJsonAsync<JsonObject>();
            var book_id = body["id"].AsValue().GetValue<int>();

            string new_title = null;
            string new_isbn = "New ISBN";
            string new_description = "New Description";
            string new_author = "New Author";

            // act
            var response = await UpdateBookAsync(book_id, new_title, new_isbn, new_description, new_author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Update_Book_When_Title_Is_Empty_Returns_BadRequest()
        {
            // arrange
            string old_title = "Old Title";
            string old_isbn = "Old ISBN";
            string old_description = "Old Description";
            string old_author = "Old Author";
            var responseCreate = await CreateBookAsync(old_title, old_isbn, old_description, old_author);
            var body = await responseCreate.Content.ReadFromJsonAsync<JsonObject>();
            var book_id = body["id"].AsValue().GetValue<int>();

            string new_title = "";
            string new_isbn = "New ISBN";
            string new_description = "New Description";
            string new_author = "New Author";

            // act
            var response = await UpdateBookAsync(book_id, new_title, new_isbn, new_description, new_author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC016_Update_Book_When_ISBN_Is_Null_Returns_BadRequest()
        {
            // arrange
            string old_title = "Old Title";
            string old_isbn = "Old ISBN";
            string old_description = "Old Description";
            string old_author = "Old Author";
            var responseCreate = await CreateBookAsync(old_title, old_isbn, old_description, old_author);
            var body = await responseCreate.Content.ReadFromJsonAsync<JsonObject>();
            var book_id = body["id"].AsValue().GetValue<int>();

            string new_title = "New Title";
            string new_isbn = null;
            string new_description = "New Description";
            string new_author = "New Author";

            // act
            var response = await UpdateBookAsync(book_id, new_title, new_isbn, new_description, new_author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Update_Book_When_ISBN_Is_Empty_Returns_BadRequest()
        {
            // arrange
            string old_title = "Old Title";
            string old_isbn = "Old ISBN";
            string old_description = "Old Description";
            string old_author = "Old Author";
            var responseCreate = await CreateBookAsync(old_title, old_isbn, old_description, old_author);
            var body = await responseCreate.Content.ReadFromJsonAsync<JsonObject>();
            var book_id = body["id"].AsValue().GetValue<int>();

            string new_title = "New Title";
            string new_isbn = "";
            string new_description = "New Description";
            string new_author = "New Author";

            // act
            var response = await UpdateBookAsync(book_id, new_title, new_isbn, new_description, new_author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    } 
}