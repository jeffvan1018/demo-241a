namespace DCC.Util;

/// <summary>
/// Summary report of current stock levels and inventory valuations.
/// </summary>
public record InventoryReport
{
    public required int TotalPartItems { get; init; }
    public required int TotalPartQuantity { get; init; }
    public required decimal TotalPartValuation { get; init; }
    public required int LowStockItemsCount { get; init; }
    public required int TotalVehicles { get; init; }
    public required int VehiclesInStockCount { get; init; }
    public required int VehiclesInProductionCount { get; init; }
    public required decimal InStockVehicleValuation { get; init; }
    public decimal TotalInventoryValuation => TotalPartValuation + InStockVehicleValuation;
}
