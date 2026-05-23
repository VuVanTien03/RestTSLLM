using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TodoApi.Todos
{
    public class TodoResponse
    {
        [DefaultValue("1")]
        public int Id { get; set; }
        [DefaultValue("Something todo in the future")]
        public string Title { get; set; } = default!;
        [DefaultValue(false)]
        public bool IsComplete { get; set; }
    }
}
