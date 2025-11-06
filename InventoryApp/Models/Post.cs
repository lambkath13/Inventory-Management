using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Models;

public class Post
{
    public int Id { get; set; }
    public int InventoryId { get; set; }
    [Required] public string UserId { get; set; } = "";
    [Required] public string BodyMarkdown { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
