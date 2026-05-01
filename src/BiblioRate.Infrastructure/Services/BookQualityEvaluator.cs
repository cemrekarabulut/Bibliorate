using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiblioRate.Application.Interfaces; // 1. HATA: Interface'i bulabilmesi için bu şart
using BiblioRate.Domain.Entities;

namespace BiblioRate.Infrastructure.Services
{
   // Infrastructure/Services/BookQualityEvaluator.cs
public class BookQualityEvaluator : IBookQualityEvaluator
{
    private static readonly string[] NoisyGenres = 
        ["Adopted children", "Juvenile Fiction", "Undefined", "General"];

    public int Evaluate(Book book)
    {
        int score = 0;
        score += HasGoodThumbnail(book.ThumbnailUrl) ? 30 : 0;
        
        bool hasGoodDesc = (book.Description?.Length ?? 0) > 200;
        score += hasGoodDesc ? 25 : 0;
        
        bool isValidIsbn = IsValidIsbn13(book.Isbn);
        score += isValidIsbn ? 20 : 0;
        
        score += IsCleanGenre(book.Genre) ? 25 : 0;

        // ISBN formatı bozuk olsa bile kaliteli açıklamalara şans tanı
        if (!isValidIsbn && hasGoodDesc)
        {
            score += 15; // Kaybedilen 20 puanın 15'ini açıklama kalitesiyle telafi et
        }

        return Math.Clamp(score, 0, 100);
    }

    private static bool HasGoodThumbnail(string? url) =>
        !string.IsNullOrEmpty(url) &&
        !url.Contains("no_image") &&
        !url.Contains("placeholder") &&
        Uri.TryCreate(url, UriKind.Absolute, out _);

    private static bool IsValidIsbn13(string? isbn)
    {
        if (string.IsNullOrEmpty(isbn)) return false;
        var digits = isbn.Replace("-", "").Replace(" ", "");
        if (digits.Length != 13 || !digits.All(char.IsDigit)) return false;

        // ISBN-13 checksum: alternating weights 1 and 3, total mod 10 must be 0
        var sum = 0;
        for (int i = 0; i < 12; i++)
            sum += (digits[i] - '0') * (i % 2 == 0 ? 1 : 3);
        var checkDigit = (10 - (sum % 10)) % 10;
        return checkDigit == (digits[12] - '0');
    }

    private static bool IsCleanGenre(string? genre) =>
        !string.IsNullOrEmpty(genre) &&
        !NoisyGenres.Any(n => genre.Contains(n, StringComparison.OrdinalIgnoreCase));
}
}