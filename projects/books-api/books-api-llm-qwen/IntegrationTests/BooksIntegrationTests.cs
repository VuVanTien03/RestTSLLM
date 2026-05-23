// File: IntegrationTests/BookIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Net;
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

        private async Task<HttpResponseMessage> GetBookByIdAsync(int id)
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

        private async Task<HttpResponseMessage> DeleteBookByIdAsync(int id)
        {
            return await _client.DeleteAsync($"/books/{id}");
        }

        private async Task<int> CreateBookAndGetIdAsync(string title, string isbn, string description, string author)
        {
            var response = await CreateBookAsync(title, isbn, description, author);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].AsValue().GetValue<int>();
        }

        [Fact]
        public async Task TC001_Get_All_Books_Returns_OK()
        {
            // act
            var response = await _client.GetAsync("/books");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC002_Create_Book_When_Valid_Data_Returns_OK()
        {
            // arrange
            string title = "Valid Title";
            string isbn = "1234567890";
            string description = "Valid Description";
            string author = "Valid Author";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Create_Book_When_Title_Is_Null_Returns_BadRequest()
        {
            // arrange
            string title = null;
            string isbn = "1234567890";
            string description = "Valid Description";
            string author = "Valid Author";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_Book_When_Title_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            string title = "";
            string isbn = "1234567890";
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
        public async Task TC006_Create_Book_When_ISBN_Is_Empty_String_Returns_BadRequest()
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
            string isbn = "1234567890";
            string description = null;
            string author = "Valid Author";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_Book_When_Description_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            string title = "Valid Title";
            string isbn = "1234567890";
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
            string isbn = "1234567890";
            string description = "Valid Description";
            string author = null;

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Create_Book_When_Author_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            string title = "Valid Title";
            string isbn = "1234567890";
            string description = "Valid Description";
            string author = "";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Get_Book_By_ID_When_Valid_ID_Returns_OK()
        {
            // arrange
            string title = "Valid Title";
            string isbn = "1234567890";
            string description = "Valid Description";
            string author = "Valid Author";
            int bookId = await CreateBookAndGetIdAsync(title, isbn, description, author);

            // act
            var response = await GetBookByIdAsync(bookId);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Get_Book_By_ID_When_Invalid_ID_Returns_NotFound()
        {
            // arrange
            int invalidBookId = 9999999;

            // act
            var response = await GetBookByIdAsync(invalidBookId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Update_Book_When_Valid_ID_And_Valid_Data_Returns_NoContent()
        {
            // arrange
            string title = "Valid Title";
            string isbn = "1234567890";
            string description = "Valid Description";
            string author = "Valid Author";
            int bookId = await CreateBookAndGetIdAsync(title, isbn, description, author);

            string updatedTitle = "Updated Title";
            string updatedIsbn = "0987654321";
            string updatedDescription = "Updated Description";
            string updatedAuthor = "Updated Author";

            // act
            var response = await UpdateBookAsync(bookId, updatedTitle, updatedIsbn, updatedDescription, updatedAuthor);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Update_Book_When_Invalid_ID_Returns_NotFound()
        {
            // arrange
            int invalidBookId = 9999999;
            string updatedTitle = "Updated Title";
            string updatedIsbn = "0987654321";
            string updatedDescription = "Updated Description";
            string updatedAuthor = "Updated Author";

            // act
            var response = await UpdateBookAsync(invalidBookId, updatedTitle, updatedIsbn, updatedDescription, updatedAuthor);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Update_Book_When_Title_Is_Null_Returns_BadRequest()
        {
            // arrange
            string title = "Valid Title";
            string isbn = "1234567890";
            string description = "Valid Description";
            string author = "Valid Author";
            int bookId = await CreateBookAndGetIdAsync(title, isbn, description, author);

            string updatedTitle = null;
            string updatedIsbn = "0987654321";
            string updatedDescription = "Updated Description";
            string updatedAuthor = "Updated Author";

            // act
            var response = await UpdateBookAsync(bookId, updatedTitle, updatedIsbn, updatedDescription, updatedAuthor);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC016_Update_Book_When_Title_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            string title = "Valid Title";
            string isbn = "1234567890";
            string description = "Valid Description";
            string author = "Valid Author";
            int bookId = await CreateBookAndGetIdAsync(title, isbn, description, author);

            string updatedTitle = "";
            string updatedIsbn = "0987654321";
            string updatedDescription = "Updated Description";
            string updatedAuthor = "Updated Author";

            // act
            var response = await UpdateBookAsync(bookId, updatedTitle, updatedIsbn, updatedDescription, updatedAuthor);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Update_Book_When_ISBN_Is_Null_Returns_BadRequest()
        {
            // arrange
            string title = "Valid Title";
            string isbn = "1234567890";
            string description = "Valid Description";
            string author = "Valid Author";
            int bookId = await CreateBookAndGetIdAsync(title, isbn, description, author);

            string updatedTitle = "Updated Title";
            string updatedIsbn = null;
            string updatedDescription = "Updated Description";
            string updatedAuthor = "Updated Author";

            // act
            var response = await UpdateBookAsync(bookId, updatedTitle, updatedIsbn, updatedDescription, updatedAuthor);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC018_Update_Book_When_ISBN_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            string title = "Valid Title";
            string isbn = "1234567890";
            string description = "Valid Description";
            string author = "Valid Author";
            int bookId = await CreateBookAndGetIdAsync(title, isbn, description, author);

            string updatedTitle = "Updated Title";
            string updatedIsbn = "";
            string updatedDescription = "Updated Description";
            string updatedAuthor = "Updated Author";

            // act
            var response = await UpdateBookAsync(bookId, updatedTitle, updatedIsbn, updatedDescription, updatedAuthor);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC019_Update_Book_When_Description_Is_Null_Returns_BadRequest()
        {
            // arrange
            string title = "Valid Title";
            string isbn = "1234567890";
            string description = "Valid Description";
            string author = "Valid Author";
            int bookId = await CreateBookAndGetIdAsync(title, isbn, description, author);

            string updatedTitle = "Updated Title";
            string updatedIsbn = "0987654321";
            string updatedDescription = null;
            string updatedAuthor = "Updated Author";

            // act
            var response = await UpdateBookAsync(bookId, updatedTitle, updatedIsbn, updatedDescription, updatedAuthor);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC020_Update_Book_When_Description_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            string title = "Valid Title";
            string isbn = "1234567890";
            string description = "Valid Description";
            string author = "Valid Author";
            int bookId = await CreateBookAndGetIdAsync(title, isbn, description, author);

            string updatedTitle = "Updated Title";
            string updatedIsbn = "0987654321";
            string updatedDescription = "";
            string updatedAuthor = "Updated Author";

            // act
            var response = await UpdateBookAsync(bookId, updatedTitle, updatedIsbn, updatedDescription, updatedAuthor);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC021_Update_Book_When_Author_Is_Null_Returns_BadRequest()
        {
            // arrange
            string title = "Valid Title";
            string isbn = "1234567890";
            string description = "Valid Description";
            string author = "Valid Author";
            int bookId = await CreateBookAndGetIdAsync(title, isbn, description, author);

            string updatedTitle = "Updated Title";
            string updatedIsbn = "0987654321";
            string updatedDescription = "Updated Description";
            string updatedAuthor = null;

            // act
            var response = await UpdateBookAsync(bookId, updatedTitle, updatedIsbn, updatedDescription, updatedAuthor);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC022_Update_Book_When_Author_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            string title = "Valid Title";
            string isbn = "1234567890";
            string description = "Valid Description";
            string author = "Valid Author";
            int bookId = await CreateBookAndGetIdAsync(title, isbn, description, author);

            string updatedTitle = "Updated Title";
            string updatedIsbn = "0987654321";
            string updatedDescription = "Updated Description";
            string updatedAuthor = "";

            // act
            var response = await UpdateBookAsync(bookId, updatedTitle, updatedIsbn, updatedDescription, updatedAuthor);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC023_Delete_Book_By_ID_When_Valid_ID_Returns_NoContent()
        {
            // arrange
            string title = "Valid Title";
            string isbn = "1234567890";
            string description = "Valid Description";
            string author = "Valid Author";
            int bookId = await CreateBookAndGetIdAsync(title, isbn, description, author);

            // act
            var response = await DeleteBookByIdAsync(bookId);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC024_Delete_Book_By_ID_When_Invalid_ID_Returns_NotFound()
        {
            // arrange
            int invalidBookId = 9999999;

            // act
            var response = await DeleteBookByIdAsync(invalidBookId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}