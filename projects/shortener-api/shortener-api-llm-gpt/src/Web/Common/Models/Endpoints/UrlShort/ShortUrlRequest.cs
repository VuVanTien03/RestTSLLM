using DataAnnotationsExtensions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Web.UseCases.UrlShorten.ShortUrl;

namespace Web.Common.Models.Endpoints.UrlShort;

public class ShortUrlRequest
{
    [Required]
    [Description("Must be a valid URL.")]

    public string? Url { get; set; }

    [Min(1)]
    [Description("'null' for infinty or a number greater than or equal 1.")]
    public int? ExpireMinutes { get; set; }

    [Required]
    [Description("Enable or disabled qr code generation.")]
    public bool? HasQrCode { get; set; }

    public ShortUrlCommand ToCommand()
    {
        return new ShortUrlCommand
        {
            Url = Url,
            ExpireMinutes = ExpireMinutes,
            HasQrCode = HasQrCode,
        };
    }
}