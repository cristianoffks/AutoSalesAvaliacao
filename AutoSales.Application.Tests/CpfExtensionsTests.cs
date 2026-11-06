using AutoSales.Application.Extensions;

namespace AutoSales.Application.Tests;

public class CpfExtensionsTests
{
    [Theory]
    [InlineData("123.456.789-09", "12345678909")]
    [InlineData("12345678909", "12345678909")]
    [InlineData(" 012.345.678-90 ", "01234567890")]
    public void NormalizeCpf_ReturnsOnlyNumbers(string cpf, string expected)
    {
        Assert.Equal(expected, cpf.NormalizeCpf());
    }
}
