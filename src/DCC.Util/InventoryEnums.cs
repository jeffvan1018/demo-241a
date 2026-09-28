namespace DCC.Util;

/// <summary>
/// Specifies the category of a cycle car component, part, or raw material.
/// </summary>
public enum PartCategory
{
    Engine,
    Chassis,
    Transmission,
    WheelsAndTires,
    BodyAndFrame,
    Electrical,
    Brakes,
    InteriorAndTrim,
    Accessories,
    Hardware
}

/// <summary>
/// Specifies the availability or production status of a Detroit Cycle Car vehicle.
/// </summary>
public enum VehicleStatus
{
    InProduction,
    InStock,
    Reserved,
    Sold,
    InMaintenance
}

/// <summary>
/// Specifies the type of stock movement or adjustment.
/// </summary>
public enum TransactionType
{
    PurchaseReceipt,
    ProductionConsumption,
    StockAdjustment,
    CustomerReturn,
    Scrap
}
