namespace InventoryApp.Models;

public class CustomIdFormat
{
    public int InventoryId { get; set; } 
    public string JsonDefinition { get; set; } = "[]";
    public string? CompiledRegex { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
