using AutoSales.Application.Entities;

namespace AutoSales.Application.Responses;

public sealed record VehicleResponse(
    int Id,
    string Plate,
    int ManufactureYear,
    string Brand,
    string Model,
    int PersonId)
{
    public static VehicleResponse From(Vehicle vehicle)
    {
        return new VehicleResponse(
            vehicle.Id,
            vehicle.Plate,
            vehicle.ManufactureYear,
            vehicle.Brand,
            vehicle.Model,
            vehicle.PersonId);
    }
}
