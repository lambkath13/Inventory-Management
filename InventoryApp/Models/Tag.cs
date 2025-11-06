using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Models;

public class Tag
{
    public int Id { get; set; }
    [Required, MaxLength(64)] 
    public string Name { get; set; } = "";
}
