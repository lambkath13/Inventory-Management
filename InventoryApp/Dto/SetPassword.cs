using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Dto;

public class SetPassword
{
    [Required, DataType(DataType.Password), MinLength(6)]
    public string NewPassword { get; set; } = "";

    [Required, DataType(DataType.Password), Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = "";
}