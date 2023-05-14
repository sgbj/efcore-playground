namespace webapi.Models;

public class Blog : EntityBase
{
    public string Name { get; set; } = null!;
    public List<Post> Posts { get; set; } = null!;
}
