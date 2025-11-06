using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Data;
using InventoryApp.Models;
using InventoryApp.Services;
using InventoryApp.Enums;


namespace Inventory.Web.Domain.Services;

public class CustomIdService : ICustomIdService
{
    private readonly ApplicationDbContext _db;
    private readonly Random _rnd = new();

    public CustomIdService(ApplicationDbContext db) => _db = db;

    public async Task<string> GenerateAsync(int inventoryId, CancellationToken ct = default)
{
    await using var tx = await _db.Database.BeginTransactionAsync(ct);

    var format = await _db.CustomIdFormats
        .AsNoTracking()
        .FirstOrDefaultAsync(f => f.InventoryId == inventoryId, ct);


    var seq = await _db.InventorySequences
        .FromSqlRaw(@"SELECT * FROM ""InventorySequences""
                      WHERE ""InventoryId"" = {0}
                      FOR UPDATE",
                    inventoryId)
        .FirstOrDefaultAsync(ct);

    long nextSeq = (seq?.LastValue ?? 0) + 1;

    if (seq is null)
    {
        seq = new InventorySequence { InventoryId = inventoryId, LastValue = nextSeq };
        _db.InventorySequences.Add(seq);
    }
    else
    {
        seq.LastValue = nextSeq;
        _db.InventorySequences.Update(seq);
    }

    if (format == null)
        return Guid.NewGuid().ToString("N")[..8];

    var elements = JsonSerializer.Deserialize<List<CustomIdElement>>(format.JsonDefinition) ?? new();
    var sb = new StringBuilder();
    foreach (var e in elements)
    {
        sb.Append(e.Kind switch
        {
            CustomIdElementKind.Fixed     => e.Value ?? "",
            CustomIdElementKind.Random20  => _rnd.Next(0, 1 << 20).ToString("X5"),
            CustomIdElementKind.Random32  => _rnd.NextInt64(0, 1L << 32).ToString("X8"),
            CustomIdElementKind.D6        => _rnd.Next(0, 999_999).ToString("D6"),
            CustomIdElementKind.D9        => _rnd.Next(0, 999_999_999).ToString("D9"),
            CustomIdElementKind.Guid      => Guid.NewGuid().ToString("N")[..8],
            CustomIdElementKind.Date      => DateTime.UtcNow.ToString(e.Format ?? "yyyyMMdd"),
            CustomIdElementKind.Sequence  => nextSeq.ToString(e.Format ?? "D3"),
            _ => ""
        });
    }

    await _db.SaveChangesAsync(ct);
    await tx.CommitAsync(ct);

    return sb.ToString();
}

    public bool Validate(CustomIdFormat format, string value)
        => string.IsNullOrEmpty(format.CompiledRegex) ||
           System.Text.RegularExpressions.Regex.IsMatch(value, format.CompiledRegex);
}

public class CustomIdElement
{
    public CustomIdElementKind Kind { get; set; }
    public string? Value { get; set; }
    public string? Format { get; set; }
}
