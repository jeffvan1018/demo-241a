namespace DCC.Util;

/// <summary>
/// Represents a completed or in-production Detroit Cycle Car vehicle.
/// </summary>
public class CycleCar
{
    private decimal _basePrice;
    private readonly HashSet<string> _installedPartSkus = new(StringComparer.OrdinalIgnoreCase);

    public CycleCar(
        string serialNumber,
        string modelName,
        int modelYear,
        decimal basePrice,
        VehicleStatus status = VehicleStatus.InProduction,
        string? color = null,
        string? engineNumber = null,
        DateTimeOffset? dateManufactured = null,
        string? notes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serialNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(modelName);

        if (modelYear < 1900 || modelYear > 2100)
            throw new ArgumentOutOfRangeException(nameof(modelYear), "Model year must be a realistic year.");
        if (basePrice < 0)
            throw new ArgumentOutOfRangeException(nameof(basePrice), "Base price cannot be negative.");

        SerialNumber = serialNumber.Trim().ToUpperInvariant();
        ModelName = modelName.Trim();
        ModelYear = modelYear;
        BasePrice = basePrice;
        Status = status;
        Color = color;
        EngineNumber = engineNumber;
        DateManufactured = dateManufactured;
        Notes = notes;
    }

    /// <summary>
    /// Gets the unique vehicle serial number / chassis number.
    /// </summary>
    public string SerialNumber { get; }

    /// <summary>
    /// Gets or sets the model name (e.g., "Little Detroit Speedster", "Saginaw Speedster").
    /// </summary>
    public string ModelName { get; set; }

    /// <summary>
    /// Gets or sets the model year (e.g., 1913, 1914).
    /// </summary>
    public int ModelYear { get; set; }

    /// <summary>
    /// Gets or sets the base price of the cycle car.
    /// </summary>
    public decimal BasePrice
    {
        get => _basePrice;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Base price cannot be negative.");
            _basePrice = value;
        }
    }

    /// <summary>
    /// Gets or sets the current production or sales status.
    /// </summary>
    public VehicleStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the vehicle exterior finish / color.
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Gets or sets the serial number of the fitted engine.
    /// </summary>
    public string? EngineNumber { get; set; }

    /// <summary>
    /// Gets or sets the date the vehicle completed assembly.
    /// </summary>
    public DateTimeOffset? DateManufactured { get; set; }

    /// <summary>
    /// Gets or sets general notes or customer build specifications.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets the collection of installed part SKUs associated with this cycle car.
    /// </summary>
    public IReadOnlyCollection<string> InstalledPartSkus => _installedPartSkus;

    /// <summary>
    /// Associates a part SKU with this vehicle build.
    /// </summary>
    public bool InstallPart(string partSku)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partSku);
        return _installedPartSkus.Add(partSku.Trim().ToUpperInvariant());
    }

    /// <summary>
    /// Removes an associated part SKU from this vehicle build.
    /// </summary>
    public bool RemovePart(string partSku)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partSku);
        return _installedPartSkus.Remove(partSku.Trim().ToUpperInvariant());
    }
}
