using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Dto;

public class Register
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, MinLength(3)]
    public string DisplayName { get; set; } = "";

    [Required, DataType(DataType.Password), MinLength(6)]
    public string Password { get; set; } = "";

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = "";
}