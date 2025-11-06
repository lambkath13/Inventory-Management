using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Models;
public class Item
{
    public int Id { get; set; }
    public int InventoryId { get; set; }

    [Required, MaxLength(128)] 
    public string CustomId { get; set; } = "";

    [Required] 
    public string CreatedByUserId { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Timestamp]
    public uint xmin { get; set; }

    public string? String1 { get; set; }
    public string? String2 { get; set; }
    public string? String3 { get; set; }

    public int?    Int1 { get; set; }
    public int?    Int2 { get; set; }
    public int?    Int3 { get; set; }

    public bool?   Bool1 { get; set; }
    public bool?   Bool2 { get; set; }
    public bool?   Bool3 { get; set; }

    public string? Text1 { get; set; }
    public string? Text2 { get; set; }
    public string? Text3 { get; set; }

    public string? Link1 { get; set; }
    public string? Link2 { get; set; }
    public string? Link3 { get; set; }
}