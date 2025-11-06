namespace InventoryApp.Models;

public class Like
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string UserId { get; set; } = "";
}
