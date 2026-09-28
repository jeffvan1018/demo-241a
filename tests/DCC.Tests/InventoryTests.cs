using DCC.Util;
using Xunit;

namespace DCC.Tests;

public class InventoryTests
{
    [Fact]
    public void InventoryItem_CalculatesLowStockAndTotalValueCorrectly()
    {
        var item = new InventoryItem(
            sku: "ENG-1914-15L",
            name: "1.5L 4-Cylinder Water-Cooled Engine",
            category: PartCategory.Engine,
            unitPrice: 120.00m,
            quantityOnHand: 4,
            reorderLevel: 5,
            reorderQuantity: 10
        );

        Assert.Equal("ENG-1914-15L", item.Sku);
        Assert.True(item.IsLowStock);
        Assert.Equal(480.00m, item.TotalValue);

        item.AdjustQuantity(3);
        Assert.Equal(7, item.QuantityOnHand);
        Assert.False(item.IsLowStock);
        Assert.Equal(840.00m, item.TotalValue);
    }

    [Fact]
    public void InventoryItem_ThrowsOnNegativeStockAdjustment()
    {
        var item = new InventoryItem(
            sku: "WHL-28-WIRE",
            name: "28-inch Wire Spoke Wheel",
            category: PartCategory.WheelsAndTires,
            unitPrice: 15.50m,
            quantityOnHand: 2
        );

        Assert.Throws<InsufficientStockException>(() => item.AdjustQuantity(-3));
    }

    [Fact]
    public void CycleCar_CanInstallAndRemoveParts()
    {
        var car = new CycleCar(
            serialNumber: "DCC-1914-001",
            modelName: "Little Detroit Speedster",
            modelYear: 1914,
            basePrice: 375.00m,
            color: "Speedster Red"
        );

        Assert.Equal("DCC-1914-001", car.SerialNumber);
        Assert.Equal(VehicleStatus.InProduction, car.Status);
        Assert.Equal(375.00m, car.BasePrice);

        bool added1 = car.InstallPart("ENG-1914-15L");
        bool addedDuplicate = car.InstallPart("eng-1914-15l");
        bool added2 = car.InstallPart("CHS-1914-FRAME");

        Assert.True(added1);
        Assert.False(addedDuplicate);
        Assert.True(added2);
        Assert.Equal(2, car.InstalledPartSkus.Count);
        Assert.Contains("ENG-1914-15L", car.InstalledPartSkus);

        bool removed = car.RemovePart("ENG-1914-15L");
        Assert.True(removed);
        Assert.Single(car.InstalledPartSkus);
    }

    [Fact]
    public void InventoryManager_AdjustStock_UpdatesQuantityAndRecordsTransaction()
    {
        var manager = new InventoryManager();
        var item = new InventoryItem(
            sku: "BRK-1914-DRUM",
            name: "Mechanical Drum Brake Assembly",
            category: PartCategory.Brakes,
            unitPrice: 22.00m,
            quantityOnHand: 10,
            reorderLevel: 5
        );

        manager.AddItem(item);

        int newQty = manager.AdjustStock(
            sku: "BRK-1914-DRUM",
            quantityChange: -4,
            type: TransactionType.ProductionConsumption,
            referenceNumber: "BUILD-WO-101",
            notes: "Fitted to DCC-1914-001"
        );

        Assert.Equal(6, newQty);

        var transactions = manager.GetTransactions("BRK-1914-DRUM");
        // Expecting 2 transactions: initial setup (10) and consumption (-4)
        Assert.Equal(2, transactions.Count);

        var latestTx = transactions[0];
        Assert.Equal(TransactionType.ProductionConsumption, latestTx.Type);
        Assert.Equal(-4, latestTx.QuantityChange);
        Assert.Equal("BUILD-WO-101", latestTx.ReferenceNumber);
    }

    [Fact]
    public void InventoryManager_ThrowsOnDuplicateSkuOrSerial()
    {
        var manager = new InventoryManager();
        var item1 = new InventoryItem("TRN-1914-2SPD", "2-Speed Transmission", PartCategory.Transmission, 45.00m);
        var item2 = new InventoryItem("trn-1914-2spd", "Duplicate Transmission", PartCategory.Transmission, 50.00m);

        manager.AddItem(item1);
        Assert.Throws<DuplicateInventoryItemException>(() => manager.AddItem(item2));

        var car1 = new CycleCar("DCC-1914-002", "Saginaw Speedster", 1914, 375m);
        var car2 = new CycleCar("dcc-1914-002", "Saginaw Speedster", 1914, 375m);

        manager.RegisterVehicle(car1);
        Assert.Throws<DuplicateInventoryItemException>(() => manager.RegisterVehicle(car2));
    }

    [Fact]
    public void InventoryManager_GeneratesAccurateValuationReport()
    {
        var manager = new InventoryManager();

        manager.AddItem(new InventoryItem("ENG-1", "Engine", PartCategory.Engine, 100m, quantityOnHand: 5, reorderLevel: 2));
        manager.AddItem(new InventoryItem("WHL-1", "Wheel", PartCategory.WheelsAndTires, 20m, quantityOnHand: 2, reorderLevel: 4)); // Low stock

        manager.RegisterVehicle(new CycleCar("DCC-01", "Speedster", 1914, 375m, status: VehicleStatus.InStock));
        manager.RegisterVehicle(new CycleCar("DCC-02", "Speedster", 1914, 375m, status: VehicleStatus.InProduction));

        var report = manager.GenerateReport();

        Assert.Equal(2, report.TotalPartItems);
        Assert.Equal(7, report.TotalPartQuantity);
        Assert.Equal(540m, report.TotalPartValuation); // (5 * 100) + (2 * 20)
        Assert.Equal(1, report.LowStockItemsCount); // WHL-1 has 2 <= 4
        Assert.Equal(2, report.TotalVehicles);
        Assert.Equal(1, report.VehiclesInStockCount);
        Assert.Equal(1, report.VehiclesInProductionCount);
        Assert.Equal(375m, report.InStockVehicleValuation);
        Assert.Equal(915m, report.TotalInventoryValuation); // 540 + 375
    }

    [Fact]
    public void InventoryManager_FiltersByCategoryAndStatus()
    {
        var manager = new InventoryManager();

        manager.AddItem(new InventoryItem("ENG-1", "Engine A", PartCategory.Engine, 100m));
        manager.AddItem(new InventoryItem("ENG-2", "Engine B", PartCategory.Engine, 110m));
        manager.AddItem(new InventoryItem("ELE-1", "Headlamp", PartCategory.Electrical, 15m));

        var engines = manager.GetItemsByCategory(PartCategory.Engine);
        Assert.Equal(2, engines.Count);

        manager.RegisterVehicle(new CycleCar("DCC-01", "Speedster", 1914, 375m, status: VehicleStatus.InStock));
        manager.RegisterVehicle(new CycleCar("DCC-02", "Speedster", 1914, 375m, status: VehicleStatus.Sold));

        var inStock = manager.GetVehiclesByStatus(VehicleStatus.InStock);
        Assert.Single(inStock);
        Assert.Equal("DCC-01", inStock[0].SerialNumber);
    }
}
