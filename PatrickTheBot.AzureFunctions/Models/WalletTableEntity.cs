using System;
using Azure;
using Azure.Data.Tables;

namespace PatrickTheBot.AzureFunctions.Models;

public record WalletTableEntity : ITableEntity
{
    public string? WalletHolder { get; init; }
    public int BackendBucks { get; set; }
    public int BackendPoints { get; set; }
    public int FrontendPoints { get; set; }
    
    public string PartitionKey { get; set; } = default!;
    public string RowKey { get; set; } = default!;
    public DateTimeOffset? Timestamp { get; set; } = default!;
    public ETag ETag { get; set; } = default!;
}