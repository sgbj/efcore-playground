namespace webapi.Models;

public class Comment : EntityBase
{
    public string Content { get; set; } = null!;
    public string PostId { get; set; } = null!;
    public Post Post { get; set; } = null!;
}
