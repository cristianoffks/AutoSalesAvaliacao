using System.Text.Json;
using AutoSales.Application.Requests;

namespace AutoSales.Application.Tests;

public class UpdateRequestTests
{
    [Fact]
    public void UpdateVehicleRequest_IdIsNotReadFromBody()
    {
        var request = JsonSerializer.Deserialize<UpdateVehicleRequest>(
            """{"Id":999,"Plate":"ABC1234","ManufactureYear":2020,"Brand":"Honda","Model":"Civic"}""");

        Assert.NotNull(request);
        Assert.Equal(0, request.Id);
        request.Id = 10;
        Assert.DoesNotContain("\"Id\"", JsonSerializer.Serialize(request));
    }

    [Fact]
    public void UpdatePersonRequest_IdIsNotReadFromBody()
    {
        var request = JsonSerializer.Deserialize<UpdatePersonRequest>(
            """{"Id":999,"FullName":"Ana","Cpf":"12345678909","BirthDate":"1990-01-15","Email":"ana@example.com","Phone":"11999999999"}""");

        Assert.NotNull(request);
        Assert.Equal(0, request.Id);
        request.Id = 10;
        Assert.DoesNotContain("\"Id\"", JsonSerializer.Serialize(request));
    }
}
