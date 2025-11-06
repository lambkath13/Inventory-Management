namespace InventoryApp.Dto;

public record ItemCreateDto(
    int InventoryId,
    string? String1, string? String2, string? String3,
    int? Int1, int? Int2, int? Int3,
    bool? Bool1, bool? Bool2, bool? Bool3,
    string? Text1, string? Text2, string? Text3,
    string? Link1, string? Link2, string? Link3
    );

