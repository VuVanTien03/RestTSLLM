using Carter;
using MediatR;
using Web.Common.Models.Endpoints;
using Web.Common.Models.Endpoints.UrlShort;
using Web.Data.Entities;
using Web.Extensions;
using Web.Filter;
using Web.UseCases.UrlShorten.GetShortenedUrl;
using Web.UseCases.UrlShorten.ShortUrl;

namespace Web.Endpoints;

public class UrlShortEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/url-shorts")
            .WithTags("Url Shorten Endpoint")
            .WithOpenApi();

        group.MapPost("", ShortUrlAsync)
            .Produces<ShortUrlResponse>()
            .Produces<BaseResult>(400)
            .AddEndpointFilter<ValidationFilter<ShortUrlRequest>>()
            .WithOpenApi();

        app.MapGet("/{token}", GetShortenedUrlAsync)
            .Produces<BaseResult>(404)
            .Produces<UrlResult>()
            .WithTags("Url Shorten Endpoint")
            .WithOpenApi();
    }

    private static async Task<IResult> ShortUrlAsync(ShortUrlRequest request, ISender sender)
    {
        var command = request.ToCommand();
        var result = await sender.Send(command);
        return result.StatusCode == 200 ?
            Results.Json(result.Data, statusCode: 200) :
            result.ToResult();
    }
    
    private static async Task<IResult> GetShortenedUrlAsync(string? token, ISender sender)
    {
        var result = await sender.Send(new GetShortenedUrlQuery { Token = token });
        return result.StatusCode == 200 ? 
            Results.Json(new UrlResult { Url = result.Data!.LongUrl }, statusCode: 200) : 
            result.ToResult();
    }
}