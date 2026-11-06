namespace AutoSales.Application.Extensions;

public static class PlateExtensions
{
    public static string NormalizePlate(this string plate)
    {
        return plate.Replace("-", "").ToUpperInvariant();
    }
}
