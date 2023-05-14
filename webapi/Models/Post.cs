namespace webapi.Models;

public class Post : EntityBase
{
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string BlogId { get; set; } = null!;
    public Blog Blog { get; set; } = null!;
    public List<Comment> Comments { get; set; } = null!;
}
