namespace DCC.Util;

/// <summary>
/// Exception thrown when an inventory item or vehicle is not found.
/// </summary>
public class InventoryItemNotFoundException : Exception
{
    public InventoryItemNotFoundException(string identifier)
        : base($"Inventory item or vehicle with identifier '{identifier}' was not found.")
    {
        Identifier = identifier;
    }

    public string Identifier { get; }
}

/// <summary>
/// Exception thrown when an operation would reduce stock below zero.
/// </summary>
public class InsufficientStockException : Exception
{
    public InsufficientStockException(string sku, int requested, int available)
        : base($"Insufficient stock for '{sku}'. Requested {requested}, but only {available} available.")
    {
        Sku = sku;
        Requested = requested;
        Available = available;
    }

    public string Sku { get; }
    public int Requested { get; }
    public int Available { get; }
}

/// <summary>
/// Exception thrown when adding an item or vehicle that already exists.
/// </summary>
public class DuplicateInventoryItemException : Exception
{
    public DuplicateInventoryItemException(string identifier)
        : base($"An item or vehicle with identifier '{identifier}' already exists in inventory.")
    {
        Identifier = identifier;
    }

    public string Identifier { get; }
}
