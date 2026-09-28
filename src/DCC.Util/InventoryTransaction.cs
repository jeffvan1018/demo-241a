namespace DCC.Util;

/// <summary>
/// Represents an immutable log entry recording stock movements, receipts, or adjustments.
/// </summary>
public record InventoryTransaction
{
    public InventoryTransaction(
        string itemSku,
        TransactionType type,
        int quantityChange,
        string? referenceNumber = null,
        decimal? unitCost = null,
        string? notes = null,
        DateTimeOffset? timestamp = null,
        Guid? transactionId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemSku);

        TransactionId = transactionId ?? Guid.NewGuid();
        ItemSku = itemSku.Trim().ToUpperInvariant();
        Type = type;
        QuantityChange = quantityChange;
        ReferenceNumber = referenceNumber;
        UnitCost = unitCost;
        Notes = notes;
        Timestamp = timestamp ?? DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the unique identifier for this transaction.
    /// </summary>
    public Guid TransactionId { get; init; }

    /// <summary>
    /// Gets the SKU of the affected inventory item.
    /// </summary>
    public string ItemSku { get; init; }

    /// <summary>
    /// Gets the category of the stock movement.
    /// </summary>
    public TransactionType Type { get; init; }

    /// <summary>
    /// Gets the net quantity change (positive for additions, negative for deductions).
    /// </summary>
    public int QuantityChange { get; init; }

    /// <summary>
    /// Gets an optional reference number (e.g., purchase order, work order, invoice).
    /// </summary>
    public string? ReferenceNumber { get; init; }

    /// <summary>
    /// Gets the optional unit cost associated with this transaction.
    /// </summary>
    public decimal? UnitCost { get; init; }

    /// <summary>
    /// Gets optional notes detailing the transaction reason.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Gets the UTC timestamp when the transaction occurred.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; }
}
