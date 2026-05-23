namespace Restaurants.API.Swagger
{
    public class BadRequestSample
    {
        public string Type{ get; set; }

        public string Title { get; set; }

        public int Status { get; set; }

        public BadRequestSampleErrors Errors { get; set; }

        public string TraceId { get; set; }
    }

    public class BadRequestSampleErrors
    {
        public List<string> SomePropertyX { get; set; }

        public List<string> SomePropertyY { get; set; }
    }
}
