namespace DCC.Util;

/// <summary>
/// Represents a part, component, or raw material used in Detroit Cycle Car manufacturing and maintenance.
/// </summary>
public class InventoryItem
{
    private int _quantityOnHand;
    private decimal _unitPrice;
    private int _reorderLevel;
    private int _reorderQuantity;

    public InventoryItem(
        string sku,
        string name,
        PartCategory category,
        decimal unitPrice,
        int quantityOnHand = 0,
        int reorderLevel = 0,
        int reorderQuantity = 10,
        string? description = null,
        string? location = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (unitPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price cannot be negative.");
        if (quantityOnHand < 0)
            throw new ArgumentOutOfRangeException(nameof(quantityOnHand), "Initial quantity cannot be negative.");
        if (reorderLevel < 0)
            throw new ArgumentOutOfRangeException(nameof(reorderLevel), "Reorder level cannot be negative.");
        if (reorderQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(reorderQuantity), "Reorder quantity cannot be negative.");

        Sku = sku.Trim().ToUpperInvariant();
        Name = name.Trim();
        Category = category;
        UnitPrice = unitPrice;
        QuantityOnHand = quantityOnHand;
        ReorderLevel = reorderLevel;
        ReorderQuantity = reorderQuantity;
        Description = description;
        Location = location;
    }

    /// <summary>
    /// Gets the unique Stock Keeping Unit (SKU) identifier for this part.
    /// </summary>
    public string Sku { get; }

    /// <summary>
    /// Gets or sets the descriptive name of the part.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the optional detailed description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the functional category of the part.
    /// </summary>
    public PartCategory Category { get; set; }

    /// <summary>
    /// Gets or sets the unit price/cost for this item.
    /// </summary>
    public decimal UnitPrice
    {
        get => _unitPrice;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Unit price cannot be negative.");
            _unitPrice = value;
        }
    }

    /// <summary>
    /// Gets or sets the current quantity in stock.
    /// </summary>
    public int QuantityOnHand
    {
        get => _quantityOnHand;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Quantity on hand cannot be negative.");
            _quantityOnHand = value;
        }
    }

    /// <summary>
    /// Gets or sets the threshold quantity that triggers a reorder alert.
    /// </summary>
    public int ReorderLevel
    {
        get => _reorderLevel;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Reorder level cannot be negative.");
            _reorderLevel = value;
        }
    }

    /// <summary>
    /// Gets or sets the standard batch size to order when restocking.
    /// </summary>
    public int ReorderQuantity
    {
        get => _reorderQuantity;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Reorder quantity cannot be negative.");
            _reorderQuantity = value;
        }
    }

    /// <summary>
    /// Gets or sets the physical storage location (e.g. warehouse aisle, shelf).
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets a value indicating whether current stock is at or below the reorder threshold.
    /// </summary>
    public bool IsLowStock => QuantityOnHand <= ReorderLevel;

    /// <summary>
    /// Gets the total calculated inventory valuation for this item (QuantityOnHand * UnitPrice).
    /// </summary>
    public decimal TotalValue => QuantityOnHand * UnitPrice;

    /// <summary>
    /// Adjusts the stock quantity by the specified amount (positive to increase, negative to decrease).
    /// </summary>
    /// <param name="delta">Amount to adjust by.</param>
    /// <returns>The new quantity on hand.</returns>
    /// <exception cref="InsufficientStockException">Thrown if reduction exceeds available stock.</exception>
    public int AdjustQuantity(int delta)
    {
        int newQuantity = _quantityOnHand + delta;
        if (newQuantity < 0)
        {
            throw new InsufficientStockException(Sku, Math.Abs(delta), _quantityOnHand);
        }
        _quantityOnHand = newQuantity;
        return _quantityOnHand;
    }
}
