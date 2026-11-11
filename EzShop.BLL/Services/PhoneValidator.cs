using System.Text.RegularExpressions;

namespace EzShop.BLL.Services;

/// <summary>
/// Валидатор и нормализатор телефонных номеров Кыргызстана (+996).
/// Допустимые форматы:
/// 1) +996 (XXX) XX-XX-XX или +996XXXXXXXXX (12 цифр с кодом страны)
/// 2) 0XXX XX-XX-XX или 0XXXXXXXXX (10 цифр с национальным префиксом 0)
/// 3) 996XXXXXXXXX (без знака плюс)
/// </summary>
public static class PhoneValidator
{
    public static bool IsValid(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return false;

        var clean = Regex.Replace(phone.Trim(), @"[\s\-\(\)\.]", "");

        // С кодом +996 или 996 (9 цифр абонента)
        if (Regex.IsMatch(clean, @"^\+?996[23579]\d{8}$") || Regex.IsMatch(clean, @"^\+?996\d{9}$"))
        {
            return true;
        }

        // В локальном формате: 0XXXXXXXXX (10 цифр)
        if (Regex.IsMatch(clean, @"^0[23579]\d{8}$") || Regex.IsMatch(clean, @"^0\d{9}$"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Приводит номер к международному стандартному формату +996XXXXXXXXX
    /// </summary>
    public static string Normalize(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return string.Empty;

        var clean = Regex.Replace(phone.Trim(), @"[\s\-\(\)\.]", "");

        if (clean.StartsWith("0") && clean.Length == 10)
        {
            return "+996" + clean[1..];
        }

        if (clean.StartsWith("996") && clean.Length == 12)
        {
            return "+" + clean;
        }

        if (clean.StartsWith("+996") && clean.Length == 13)
        {
            return clean;
        }

        return phone.Trim();
    }
}
