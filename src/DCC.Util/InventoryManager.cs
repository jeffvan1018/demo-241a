using System.Collections.Concurrent;

namespace DCC.Util;

/// <summary>
/// Central manager for Detroit Cycle Car parts inventory, vehicle stock, and transaction audit trails.
/// </summary>
public class InventoryManager
{
    private readonly ConcurrentDictionary<string, InventoryItem> _items = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, CycleCar> _vehicles = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<InventoryTransaction> _transactions = new();
    private readonly object _transactionLock = new();

    #region Part Inventory Management

    /// <summary>
    /// Adds a new part or material to the inventory catalog.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when item is null.</exception>
    /// <exception cref="DuplicateInventoryItemException">Thrown when an item with the same SKU already exists.</exception>
    public void AddItem(InventoryItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (!_items.TryAdd(item.Sku, item))
        {
            throw new DuplicateInventoryItemException(item.Sku);
        }

        if (item.QuantityOnHand > 0)
        {
            RecordTransaction(new InventoryTransaction(
                itemSku: item.Sku,
                type: TransactionType.PurchaseReceipt,
                quantityChange: item.QuantityOnHand,
                notes: "Initial inventory setup"
            ));
        }
    }

    /// <summary>
    /// Retrieves an inventory item by its SKU.
    /// </summary>
    /// <exception cref="InventoryItemNotFoundException">Thrown when item does not exist.</exception>
    public InventoryItem GetItem(string sku)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);

        if (!_items.TryGetValue(sku.Trim().ToUpperInvariant(), out var item))
        {
            throw new InventoryItemNotFoundException(sku);
        }

        return item;
    }

    /// <summary>
    /// Attempts to retrieve an inventory item by its SKU.
    /// </summary>
    public bool TryGetItem(string sku, out InventoryItem? item)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            item = null;
            return false;
        }

        return _items.TryGetValue(sku.Trim().ToUpperInvariant(), out item);
    }

    /// <summary>
    /// Gets all inventory items currently cataloged.
    /// </summary>
    public IReadOnlyCollection<InventoryItem> GetAllItems() => _items.Values.ToList();

    /// <summary>
    /// Removes an inventory item from the catalog.
    /// </summary>
    /// <returns>True if the item was removed; otherwise false.</returns>
    public bool RemoveItem(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return false;

        return _items.TryRemove(sku.Trim().ToUpperInvariant(), out _);
    }

    /// <summary>
    /// Adjusts the stock quantity of a specific item and logs an audit transaction.
    /// </summary>
    /// <param name="sku">Item SKU.</param>
    /// <param name="quantityChange">Positive or negative adjustment.</param>
    /// <param name="type">Reason category for the movement.</param>
    /// <param name="referenceNumber">Optional document reference.</param>
    /// <param name="unitCost">Optional cost per unit for this transaction.</param>
    /// <param name="notes">Optional remarks.</param>
    /// <returns>The resulting quantity on hand.</returns>
    /// <exception cref="InventoryItemNotFoundException">Thrown when item does not exist.</exception>
    /// <exception cref="InsufficientStockException">Thrown when reducing stock below zero.</exception>
    public int AdjustStock(
        string sku,
        int quantityChange,
        TransactionType type,
        string? referenceNumber = null,
        decimal? unitCost = null,
        string? notes = null)
    {
        var item = GetItem(sku);
        int newQuantity = item.AdjustQuantity(quantityChange);

        RecordTransaction(new InventoryTransaction(
            itemSku: item.Sku,
            type: type,
            quantityChange: quantityChange,
            referenceNumber: referenceNumber,
            unitCost: unitCost ?? item.UnitPrice,
            notes: notes
        ));

        return newQuantity;
    }

    /// <summary>
    /// Retrieves all items whose current quantity on hand is at or below their reorder level.
    /// </summary>
    public IReadOnlyList<InventoryItem> GetLowStockItems()
    {
        return _items.Values
            .Where(i => i.IsLowStock)
            .OrderBy(i => i.QuantityOnHand)
            .ToList();
    }

    /// <summary>
    /// Retrieves all items belonging to a given part category.
    /// </summary>
    public IReadOnlyList<InventoryItem> GetItemsByCategory(PartCategory category)
    {
        return _items.Values
            .Where(i => i.Category == category)
            .OrderBy(i => i.Name)
            .ToList();
    }

    #endregion

    #region Vehicle Management

    /// <summary>
    /// Registers a new Detroit Cycle Car vehicle build.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when vehicle is null.</exception>
    /// <exception cref="DuplicateInventoryItemException">Thrown when vehicle with the same serial number exists.</exception>
    public void RegisterVehicle(CycleCar vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);

        if (!_vehicles.TryAdd(vehicle.SerialNumber, vehicle))
        {
            throw new DuplicateInventoryItemException(vehicle.SerialNumber);
        }
    }

    /// <summary>
    /// Retrieves a vehicle by serial number.
    /// </summary>
    /// <exception cref="InventoryItemNotFoundException">Thrown when vehicle does not exist.</exception>
    public CycleCar GetVehicle(string serialNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serialNumber);

        if (!_vehicles.TryGetValue(serialNumber.Trim().ToUpperInvariant(), out var vehicle))
        {
            throw new InventoryItemNotFoundException(serialNumber);
        }

        return vehicle;
    }

    /// <summary>
    /// Attempts to retrieve a vehicle by serial number.
    /// </summary>
    public bool TryGetVehicle(string serialNumber, out CycleCar? vehicle)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
        {
            vehicle = null;
            return false;
        }

        return _vehicles.TryGetValue(serialNumber.Trim().ToUpperInvariant(), out vehicle);
    }

    /// <summary>
    /// Updates the lifecycle status of a registered vehicle.
    /// </summary>
    public void UpdateVehicleStatus(string serialNumber, VehicleStatus newStatus)
    {
        var vehicle = GetVehicle(serialNumber);
        vehicle.Status = newStatus;
    }

    /// <summary>
    /// Gets all vehicles matching a given status.
    /// </summary>
    public IReadOnlyList<CycleCar> GetVehiclesByStatus(VehicleStatus status)
    {
        return _vehicles.Values
            .Where(v => v.Status == status)
            .OrderBy(v => v.ModelYear)
            .ThenBy(v => v.SerialNumber)
            .ToList();
    }

    /// <summary>
    /// Gets all registered vehicles.
    /// </summary>
    public IReadOnlyCollection<CycleCar> GetAllVehicles() => _vehicles.Values.ToList();

    #endregion

    #region Transactions and Audit

    /// <summary>
    /// Records an inventory movement transaction to the audit trail.
    /// </summary>
    public void RecordTransaction(InventoryTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        lock (_transactionLock)
        {
            _transactions.Add(transaction);
        }
    }

    /// <summary>
    /// Retrieves recorded transactions, optionally filtered by part SKU.
    /// </summary>
    public IReadOnlyList<InventoryTransaction> GetTransactions(string? itemSku = null)
    {
        lock (_transactionLock)
        {
            if (string.IsNullOrWhiteSpace(itemSku))
            {
                return _transactions.OrderByDescending(t => t.Timestamp).ToList();
            }

            var normalizedSku = itemSku.Trim().ToUpperInvariant();
            return _transactions
                .Where(t => t.ItemSku.Equals(normalizedSku, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(t => t.Timestamp)
                .ToList();
        }
    }

    #endregion

    #region Reports and Valuation

    /// <summary>
    /// Generates a comprehensive inventory and valuation summary report.
    /// </summary>
    public InventoryReport GenerateReport()
    {
        var items = _items.Values.ToList();
        var vehicles = _vehicles.Values.ToList();

        return new InventoryReport
        {
            TotalPartItems = items.Count,
            TotalPartQuantity = items.Sum(i => i.QuantityOnHand),
            TotalPartValuation = items.Sum(i => i.TotalValue),
            LowStockItemsCount = items.Count(i => i.IsLowStock),
            TotalVehicles = vehicles.Count,
            VehiclesInStockCount = vehicles.Count(v => v.Status == VehicleStatus.InStock),
            VehiclesInProductionCount = vehicles.Count(v => v.Status == VehicleStatus.InProduction),
            InStockVehicleValuation = vehicles
                .Where(v => v.Status == VehicleStatus.InStock)
                .Sum(v => v.BasePrice)
        };
    }

    #endregion
}
