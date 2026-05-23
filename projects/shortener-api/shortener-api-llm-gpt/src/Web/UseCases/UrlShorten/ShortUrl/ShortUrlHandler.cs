using MediatR;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using QRCoder;
using Web.Common.Constants;
using Web.Common.Models.Endpoints;
using Web.Common.Models.Options;
using Web.Data;
using d = Web.Data.Entities;
using Web.Services.Interfaces;
using Web.UseCases.UrlToken.GetUnusedToken;

namespace Web.UseCases.UrlShorten.ShortUrl;

public class ShortUrlHandler(ISender sender, MongoDbContext dbContext, ICacheService cacheService, AppSettingModel appSettingModel)
    : IRequestHandler<ShortUrlCommand, Result<ShortUrlResponse>>
{
    public async Task<Result<ShortUrlResponse>> Handle(ShortUrlCommand request, CancellationToken cancellationToken)
    {
        if (request.HasQrCode == null)
        {
            return Result<ShortUrlResponse>.Invalid("hasQrCode is required",
                new Dictionary<string, string> { { "hasQrCode", "hasQrCode is required" } });
        }

        if (request.ExpireMinutes != null && request.ExpireMinutes < 1)
        {
            return Result<ShortUrlResponse>.Invalid("invalid expire minutes",
                new Dictionary<string, string> { { "expireMinutes", "invalid expire minutes" } });
        }

        Uri uriResult;
        var validUrl = Uri.TryCreate(request.Url, UriKind.Absolute, out uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        if (!validUrl)
        {
            return Result<ShortUrlResponse>.Invalid("invalid url",
                new Dictionary<string, string> { { "url", "invalid url" } });
        }

        var response = new ShortUrlResponse();
        string token;
        var urlExist = await dbContext.UrlShortens.Find(x => x.Url == request.Url && x.ExpiredAt > DateTime.UtcNow).FirstOrDefaultAsync(cancellationToken);
        if (urlExist != null)
        {
            token = urlExist.Token;
        }
        else
        {
            var getTokenResult = await sender.Send(new GetUnusedTokenQuery(), cancellationToken);
            if (getTokenResult.StatusCode != 200)
            {
                return Result<ShortUrlResponse>.Error(getTokenResult);
            }

            token = getTokenResult.Data!.Token!;
        }

        response.Token = token;
        response.ShortenedUrl = $"{appSettingModel.Server.Url}/{token}";
        response.QrCode = GetQrBase64(request, response.ShortenedUrl);
        var expireMinutes = request.ExpireMinutes ?? appSettingModel.UrlToken.ExpirationMinutes;
        var urlShorten = new Data.Entities.UrlShorten
        {
            Url = request.Url!,
            Token = token,
            CreatedAt = DateTime.UtcNow,
            ExpiredAt = DateTime.UtcNow.AddMinutes(expireMinutes)
        };

        if (urlExist != null)
        {
            urlShorten.Id = urlExist.Id;
            var filter = Builders<d.UrlShorten>.Filter.Eq(u => u.Token, urlShorten.Token);
            await dbContext.UrlShortens.ReplaceOneAsync(filter, urlShorten, new ReplaceOptions { IsUpsert = true }, cancellationToken);
        }
        else
        {
            await dbContext.UrlShortens.InsertOneAsync(urlShorten, null, cancellationToken);
        }

        await cacheService.SetAsync(string.Format(RedisConstant.Key.ShortUrl, token), request.Url!, TimeSpan.FromMinutes(expireMinutes), cancellationToken);

        return Result<ShortUrlResponse>.Success(response);
    }

    private static string? GetQrBase64(ShortUrlCommand request, string url)
    {
        if (!request.HasQrCode.Value)
        {
            return null;
        }

        var qrGenerator = new QRCodeGenerator();
        var data = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        var code = new Base64QRCode(data);
        return code.GetGraphic(20);
    }
}