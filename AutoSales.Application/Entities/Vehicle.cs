namespace AutoSales.Application.Entities;

public sealed class Vehicle
{
    public int Id { get; set; }
    public required string Plate { get; set; }
    public int ManufactureYear { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int PersonId { get; set; }
}
