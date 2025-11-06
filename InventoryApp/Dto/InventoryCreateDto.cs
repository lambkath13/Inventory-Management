using InventoryApp.Enums;

namespace InventoryApp.Dto;

public class InventoryCreateDto
{
    public string Title { get; set; } = default!;
    public string? DescriptionMarkdown { get; set; }
    public string? Category { get; set; }
    public bool IsPublic { get; set; }
    public string[]? Tags { get; set; }  
    
    public string? CustomString1Name { get; set; }
    public string? CustomString2Name { get; set; }
    public string? CustomString3Name { get; set; }
    public CustomFieldState CustomString1State { get; set; }
    public CustomFieldState CustomString2State { get; set; }
    public CustomFieldState CustomString3State { get; set; }

    public string? CustomInt1Name { get; set; }
    public string? CustomInt2Name { get; set; }
    public string? CustomInt3Name { get; set; }
    public CustomFieldState CustomInt1State { get; set; }
    public CustomFieldState CustomInt2State { get; set; }
    public CustomFieldState CustomInt3State { get; set; }

    public string? CustomBool1Name { get; set; }
    public string? CustomBool2Name { get; set; }
    public string? CustomBool3Name { get; set; }
    public CustomFieldState CustomBool1State { get; set; }
    public CustomFieldState CustomBool2State { get; set; }
    public CustomFieldState CustomBool3State { get; set; }

    public string? CustomText1Name { get; set; }
    public string? CustomText2Name { get; set; }
    public string? CustomText3Name { get; set; }
    public CustomFieldState CustomText1State { get; set; }
    public CustomFieldState CustomText2State { get; set; }
    public CustomFieldState CustomText3State { get; set; }

    public string? CustomLink1Name { get; set; }
    public string? CustomLink2Name { get; set; }
    public string? CustomLink3Name { get; set; }
    public CustomFieldState CustomLink1State { get; set; }
    public CustomFieldState CustomLink2State { get; set; }
    public CustomFieldState CustomLink3State { get; set; }
}
