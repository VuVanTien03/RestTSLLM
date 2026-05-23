using static TodoApi.Extensions.DescribeSwaggerExtensions;
using System.ComponentModel;

namespace TodoApi.GenericResponse
{
    public class BadRequestResponse
    {
        [DefaultValue("https://tools.ietf.org/html/rfc7231#section-6.5.1")]
        public string Type { get; set; } = "https://tools.ietf.org/html/rfc7231#section-6.5.1";

        [DefaultValue("One or more validation errors occurred.")]
        public string Title { get; set; } = "One or more validation errors occurred.";

        [DefaultValue(400)]
        public int Status { get; set; } = 400;

        [DictionaryDefault("property", new string[] { "The field <property> is an invalid field (...)" })]
        public Dictionary<string, string[]> Errors { get; set; }

        public static BadRequestResponse BuildFrom(Dictionary<string, string[]> errors)
        {
            return new BadRequestResponse { Errors = errors };
        }
    }
}
