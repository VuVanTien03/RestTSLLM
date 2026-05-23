using Microsoft.AspNetCore.Http;

namespace HotelListing.API.Core.Models
{
    public class BadRequestResponse
    {
        public BadRequestResponse()
        {
                
        }

        public BadRequestResponse(Dictionary<string, string[]> errors, string traceId)
        {
            this.Errors = errors;
            this.TraceId = traceId;
        }

        public string Type { get; set; } = "https://tools.ietf.org/html/rfc7231#section-6.5.1";

        public string Title { get; set; } = "One or more validation errors occurred.";

        public int Status { get; set; } = 400;

        public string TraceId { get; set; }

        public Dictionary<string, string[]> Errors { get; set; }
    }
}
