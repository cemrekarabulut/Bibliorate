using BiblioRate.Domain.Entities;
using BiblioRate.Domain.Models;

namespace BiblioRate.API.Extensions;

/// <summary>
/// Book → BookDto dönüşümünü tek yerden yönetir.
/// BooksController ve FavoritesController'daki tekrarlanan mapping kodunu ortadan kaldırır.
/// </summary>
public static class BookMappingExtensions
{
    /// <summary>
    /// Book entity'sini BookDto'ya dönüştürür.
    /// <list type="bullet">
    ///   <item>ratingAvg değeri hem <c>ratingAvg</c> hem <c>averageRating</c> alanına yazılır — React uyumu.</item>
    ///   <item>Genre, Seeder'ın AuthorGenreMap'ten belirlediği değerden alınır.</item>
    ///   <item>ThumbnailUrl Smart Sanitize + OpenLibrary fallback zinciriyle döner.</item>
    /// </list>
    /// </summary>
    public static BookDto ToDto(this Book book)
    {
        // Eagerly loaded Ratings koleksiyonundan direkt hesapla
        var ratingAvg   = book.Ratings?.Any() == true
                            ? Math.Round(book.Ratings.Average(r => (double)r.Score), 1)
                            : 0.0;
        var ratingCount = book.Ratings?.Count ?? 0;
        var reviewCount = book.Reviews?.Count ?? 0;

        return new BookDto
        {
            BookId        = book.BookId,
            Title         = book.Title,
            Authors       = string.IsNullOrEmpty(book.Author)
                                ? []
                                : [.. book.Author.Split(',', StringSplitOptions.TrimEntries)],
            Description   = book.Description,

            // DB'deki URL için tam fallback zinciri: bad-URL tespiti → OpenLibrary → placehold.co
            ThumbnailUrl  = ResolveThumbnail(book.ThumbnailUrl, book.Isbn),

            // Puan alanları — her iki field name ile frontend'e gönderiliyor
            RatingAvg     = ratingAvg,
            AverageRating = ratingAvg,
            RatingCount   = ratingCount,
            ReviewCount   = reviewCount,

            // Tür — Seeder'ın AuthorGenreMap'ten belirlediği değeri taşır
            Genre         = book.Genre,
            Categories    = string.IsNullOrEmpty(book.Genre)
                                ? []
                                : [book.Genre]
        };
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Thumbnail URL İşleme — Fallback Zinciri
    // ──────────────────────────────────────────────────────────────────────────

    // Google'ın gerçek görsel yerine döndürdüğü 'boş/kötü' URL parçaları
    private static readonly string[] BadThumbnailSignals =
    [
        "static.googleusercontent.com",
        "books.google.com/books?id=",
        "img1.doubanio.com",
    ];

    /// <summary>
    /// DB'den gelen URL için tam fallback zinciri uygular.
    /// GoogleBooksService.ResolveThumbnail ile aynı kurallar; eski kayıtlar için ikinci güvenlik katmanı.
    /// </summary>
    private static string ResolveThumbnail(string? storedUrl, string? isbn)
    {
        var isBadOrMissing = string.IsNullOrWhiteSpace(storedUrl)
            || BadThumbnailSignals.Any(sig =>
                storedUrl!.Contains(sig, StringComparison.OrdinalIgnoreCase));

        if (!isBadOrMissing)
            return SanitizeThumbnail(storedUrl);

        // ISBN gerçekse OpenLibrary'den dene
        var cleanIsbn = isbn?.Trim();
        if (!string.IsNullOrWhiteSpace(cleanIsbn) &&
            !cleanIsbn.StartsWith("ISBN_MISSING", StringComparison.OrdinalIgnoreCase))
        {
            return $"https://covers.openlibrary.org/b/isbn/{cleanIsbn}-L.jpg";
        }

        return "https://placehold.co/128x192/1a1a2e/e0e0e0?text=No+Cover";
    }

    /// <summary>
    /// Smart Sanitize: OpenLibrary URL'lerine zoom/edge uygulamaz; yalnızca Google URL'lerini işler.
    /// </summary>
    private static string SanitizeThumbnail(string? raw)
    {
        const string Placeholder =
            "https://placehold.co/128x192/1a1a2e/e0e0e0?text=No+Cover";

        if (string.IsNullOrWhiteSpace(raw))
            return Placeholder;

        var url = raw.Trim();

        // 1. http → https (her kaynak için zorunlu)
        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            url = "https://" + url["http://".Length..];

        // 2. OpenLibrary URL'lerinde zoom/edge manipülasyonu yapma
        if (url.Contains("covers.openlibrary.org", StringComparison.OrdinalIgnoreCase))
            return url;

        // 3. Yalnızca Google URL'leri için: zoom=1 → zoom=2
        url = url.Replace("zoom=1", "zoom=2", StringComparison.OrdinalIgnoreCase);

        // 4. &edge=curl kaldır (kıvrık köşe efekti)
        url = url.Replace("&edge=curl", string.Empty, StringComparison.OrdinalIgnoreCase);

        return url;
    }
}
