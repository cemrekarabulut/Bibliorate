using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;

namespace BiblioRate.Infrastructure.Services;

public class GoogleBooksService : IGoogleBooksService
{
    private readonly HttpClient _httpClient;

    // case-insensitive okuma — Google'ın camelCase alanlarını PascalCase property'lere güvenle map eder
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Google'dan "Fiction", "General" gibi jenerik kategori gelirse authorGenre sözlüğü devreye girer
    private static readonly HashSet<string> GenericGenres = new(StringComparer.OrdinalIgnoreCase)
    {
        "Fiction", "General", "General Fiction", "Literary Fiction",
        "English Fiction", "American Fiction"
    };

    public GoogleBooksService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<Book>> SearchBooksAsync(string query, string authorName = "", string authorGenre = "General")
    {
        try
        {
            // q=Gone+Girl+Gillian+Flynn — kitap adı + yazar adı düz arama
            var q   = query.Replace(" ", "+");
            var url = $"https://www.googleapis.com/books/v1/volumes?q={q}&printType=books&maxResults=10&langRestrict=en";

            // Tam URL'i her zaman logla — 0 sonuç gelirse tarayıcıda yapıştırıp test et
            Console.WriteLine($"[API] İstek URL: {url}");

            var response = await _httpClient.GetFromJsonAsync<GoogleBooksResponse>(url, JsonOptions);

            // Ham veri logu
            Console.WriteLine($"[API] Google'dan gelen ham veri adedi: {response?.Items?.Count ?? 0} (query: \"{query}\")");

            if (response?.Items is null || response.Items.Count == 0) return [];

            // ── Yardımcı fonksiyonlar ─────────────────────────────────────────

            // ── ResolveGenre: Akıllı tür belirleme ────────────────────────────────────
            // Google spesifik bir kategori verdiyse ("Vampire", "Space Opera" vb.) koru.
            // "Fiction"/"General"/null gelirse authorGenre sözlük değerini kullan.
            string ResolveGenre(VolumeInfo v)
            {
                if (v.Categories is { Count: > 0 })
                {
                    var first = v.Categories[0].Trim();
                    if (!string.IsNullOrWhiteSpace(first) && !GenericGenres.Contains(first))
                        return first;
                }
                return authorGenre;
            }

            static int ResolveYear(string? publishedDate)
            {
                if (string.IsNullOrWhiteSpace(publishedDate) || publishedDate.Length < 4) return 0;
                return int.TryParse(publishedDate[..4], out var year) ? year : 0;
            }

            // ── Mapping ───────────────────────────────────────────────────────

            Book? TryMapToBook(GoogleBookItem item)
            {
                try
                {
                    var v = item.VolumeInfo;

                    // Zorunlu alan: title
                    if (string.IsNullOrWhiteSpace(v.Title))
                    {
                        Console.WriteLine("[Skip] - Sebep: Title boş");
                        return null;
                    }

                    // Dil kontrolü: boş veya İngilizce dışını reddet
                    var language = v.Language?.Trim() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(language))
                    {
                        Console.WriteLine($"[Skip] \"{v.Title}\" - Sebep: Language alanı boş");
                        return null;
                    }
                    if (!language.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"[Skip] \"{v.Title}\" - Sebep: Language={language} (İngilizce değil)");
                        return null;
                    }

                    // Strict author match: authorName kitabın yazarlar listesinde geçmeli
                    // authorName boşsa (fallback) kontrolü atla
                    if (!string.IsNullOrWhiteSpace(authorName))
                    {
                        var authors = v.Authors ?? [];
                        var authorMatched = authors.Any(a =>
                            a.Contains(authorName, StringComparison.OrdinalIgnoreCase));
                        if (!authorMatched)
                        {
                            Console.WriteLine($"[Skip] \"{v.Title}\" - Sebep: Yazar eşleşmedi (Arınan: {authorName}, Gelen: {string.Join(", ", authors)})");
                            return null;
                        }
                    }

                    // Genre blacklist: aşağıdaki kategoriler için kaydetme
                    var blacklistedKeywords = new[]
                    {
                        "Biography", "Autobiography", "Statistics", "Reference",
                        "Insurance", "Business", "Economics", "Self-Help",
                        "Political", "Law", "Medical", "Technology"
                    };
                    var rawCategories = v.Categories ?? [];
                    var blacklisted = rawCategories.Any(cat =>
                        blacklistedKeywords.Any(kw =>
                            cat.Contains(kw, StringComparison.OrdinalIgnoreCase)));
                    if (blacklisted)
                    {
                        Console.WriteLine($"[Skip] \"{v.Title}\" - Sebep: Kara listede kategori var ({string.Join(", ", rawCategories)})");
                        return null;
                    }

                    // Açıklama — kısa veya eksikse boş bırak, kitabı reddetme
                    var description = v.Description?.Trim() ?? string.Empty;

                    // Yayınlanma tarihi
                    var publishedAt = DateTime.UtcNow;
                    if (!string.IsNullOrWhiteSpace(v.PublishedDate) &&
                        DateTime.TryParse(v.PublishedDate, out var parsed))
                        publishedAt = parsed;

                    // ISBN — boşsa benzersiz placeholder
                    var isbn = v.IndustryIdentifiers?.FirstOrDefault()?.Identifier;
                    if (string.IsNullOrWhiteSpace(isbn))
                        isbn = $"ISBN_MISSING_{item.Id}";

                    return new Book
                    {
                        Title        = v.Title,
                        Author       = v.Authors is { Count: > 0 }
                                           ? string.Join(", ", v.Authors)
                                           : "Unknown Author",
                        Genre        = ResolveGenre(v),
                        Year         = ResolveYear(v.PublishedDate),
                        Isbn         = isbn,
                        Description  = description,
                        ThumbnailUrl = v.ImageLinks?.Thumbnail ?? string.Empty,
                        PublishedAt  = publishedAt,
                        GoogleBookId = string.IsNullOrWhiteSpace(item.Id) ? null : item.Id
                    };
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Mapping Hatası] \"{item.VolumeInfo?.Title}\": {ex.Message}");
                    return null;
                }
            }

            var books = response.Items
                .Select(TryMapToBook)
                .Where(b => b is not null)
                .Cast<Book>()
                .ToList();

            Console.WriteLine($"[API] Filtreden geçen kitap adedi: {books.Count}");
            return books;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SearchBooksAsync Hatası] {ex.Message}");
            return [];
        }
    }
}

// ── Google Books API model sınıfları ─────────────────────────────────────────
// [JsonPropertyName] ile camelCase ↔ PascalCase uyumsuzluğu kesin olarak giderildi.

file sealed class GoogleBooksResponse
{
    [JsonPropertyName("items")]
    public List<GoogleBookItem>? Items { get; set; }
}

file sealed class GoogleBookItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("volumeInfo")]
    public VolumeInfo VolumeInfo { get; set; } = new();
}

file sealed class VolumeInfo
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("authors")]
    public List<string>? Authors { get; set; }

    [JsonPropertyName("categories")]
    public List<string>? Categories { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("publishedDate")]
    public string? PublishedDate { get; set; }

    [JsonPropertyName("industryIdentifiers")]
    public List<IndustryId>? IndustryIdentifiers { get; set; }

    [JsonPropertyName("imageLinks")]
    public ImageLinks? ImageLinks { get; set; }
}

file sealed class IndustryId
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("identifier")]
    public string Identifier { get; set; } = string.Empty;
}

file sealed class ImageLinks
{
    [JsonPropertyName("thumbnail")]
    public string? Thumbnail { get; set; }
}
