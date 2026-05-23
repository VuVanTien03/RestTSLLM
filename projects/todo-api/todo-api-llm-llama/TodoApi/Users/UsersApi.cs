using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using TodoApi.Extensions;
using TodoApi.GenericResponse;
using static TodoApi.Extensions.DescribeSwaggerExtensions;

namespace TodoApi;

public static class UsersApi
{
    public static RouteGroupBuilder MapUsers(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/users");

        group.WithTags("Users");

        group.WithParameterValidation(typeof(CreateUserRequest));

        group.MapPost("/", async Task<Results<Ok, BadRequest<BadRequestResponse>>> (CreateUserRequest newUser, UserManager<TodoUser> userManager) =>
        {
            var result = await userManager.CreateAsync(new() { UserName = newUser.Username }, newUser.Password);

            if (result.Succeeded)
            {
                return TypedResults.Ok();
            }

            return TypedResults.BadRequest(BadRequestResponse.BuildFrom(result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description })));
        }).DescribeCreateUser();

        group.MapPost("/token", async Task<Results<BadRequest<BadRequestResponse>, Ok<TokenResponse>>> (CreateUserRequest userInfo, UserManager<TodoUser> userManager, ITokenService tokenService) =>
        {
            var user = await userManager.FindByNameAsync(userInfo.Username);

            if (user is null || !await userManager.CheckPasswordAsync(user, userInfo.Password))
            {
                var errors = new Dictionary<string, string[]>();
                errors.Add("password", new string[] { "Invalid user or password" });
                return TypedResults.BadRequest(BadRequestResponse.BuildFrom(errors));
            }

            return TypedResults.Ok(new TokenResponse(tokenService.GenerateToken(user.UserName!)));
        }).DescribeToken();

        return group;
    }
}
