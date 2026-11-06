using AutoSales.Application.Requests;
using AutoSales.Application.Validators;

namespace AutoSales.Application.Tests;

public class VehiclePlateValidationTests
{
    [Theory]
    [InlineData("ABC1234", true)]
    [InlineData("ABC-1234", true)]
    [InlineData("abc1d23", true)]
    [InlineData("ABC1D23", true)]
    [InlineData("1234567", false)]
    [InlineData("AB12345", false)]
    [InlineData("ABC123", false)]
    [InlineData("ABC12345", false)]
    [InlineData("ABC1DD3", false)]
    [InlineData("ABC 1234", false)]
    [InlineData("", false)]
    public void Plate_ValidatesFormatOnCreateAndUpdate(string plate, bool expected)
    {
        var create = new CreateVehicleValidator().Validate(new CreateVehicleRequest(plate, 2020, "Honda", "Civic", 1));
        var update = new UpdateVehicleValidator().Validate(new UpdateVehicleRequest(plate, 2020, "Honda", "Civic"));

        Assert.Equal(expected, !create.Errors.Any(error => error.PropertyName == "Plate"));
        Assert.Equal(expected, !update.Errors.Any(error => error.PropertyName == "Plate"));
    }
}
