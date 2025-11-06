namespace InventoryApp.Models;

public class InventoryAccess
{
    public int Id { get; set; }
    public int InventoryId { get; set; }
    public string UserId { get; set; } = "";
    public bool CanWrite { get; set; }
}
