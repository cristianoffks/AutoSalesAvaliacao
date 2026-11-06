namespace AutoSales.Application.Extensions;

public static class CpfExtensions
{
    public static string NormalizeCpf(this string cpf)
    {
        return new string(cpf.Where(character => character >= '0' && character <= '9').ToArray());
    }
}
