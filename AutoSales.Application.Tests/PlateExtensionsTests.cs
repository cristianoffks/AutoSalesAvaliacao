using AutoSales.Application.Extensions;

namespace AutoSales.Application.Tests;

public class PlateExtensionsTests
{
    [Theory]
    [InlineData("abc-1234", "ABC1234")]
    [InlineData("abc1d23", "ABC1D23")]
    [InlineData("ABC1234", "ABC1234")]
    public void NormalizePlate_RemovesHyphenAndConvertsToUppercase(string plate, string expected)
    {
        Assert.Equal(expected, plate.NormalizePlate());
    }
}
