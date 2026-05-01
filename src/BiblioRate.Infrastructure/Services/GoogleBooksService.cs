using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace BiblioRate.Infrastructure.Services;

public class GoogleBooksService : IGoogleBooksService
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;

    // case-insensitive okuma — Google'ın camelCase alanlarını PascalCase property'lere güvenle map eder
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public GoogleBooksService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GoogleBooksApiKey"];
    }

    public async Task<IEnumerable<Book>> SearchBooksAsync(string query, string authorName = "", string authorGenre = "General")
    {
        try
        {
            // q=Gone+Girl+Gillian+Flynn — kitap adı + yazar adı düz arama
            var q   = query.Replace(" ", "+");
            var url = $"https://www.googleapis.com/books/v1/volumes?q={q}&printType=books&maxResults=40&langRestrict=en";
            if (!string.IsNullOrWhiteSpace(_apiKey))
                url += $"&key={_apiKey}";

            // Tam URL'i her zaman logla — 0 sonuç gelirse tarayıcıda yapıştırıp test et
            Console.WriteLine($"[API] İstek URL: {url}");

            // GetAsync kullan — 429 durumunda ProcessSearchAsync'teki backoff'un çalışması için throw et
            using var httpResponse = await _httpClient.GetAsync(url);
            if (!httpResponse.IsSuccessStatusCode)
                throw new HttpRequestException(
                    $"HTTP {(int)httpResponse.StatusCode} hatası",
                    null, httpResponse.StatusCode);

            var response = await httpResponse.Content.ReadFromJsonAsync<GoogleBooksResponse>(JsonOptions);

            // Ham veri logu
            Console.WriteLine($"[API] Google'dan gelen ham veri adedi: {response?.Items?.Count ?? 0} (query: \"{query}\")");

            if (response?.Items is null || response.Items.Count == 0) return [];

            // ── Yardımcı fonksiyonlar ─────────────────────────────────────────

            // ── ResolveGenre: GenreNormalizer üzerinden akıllı tür belirleme ────────────
            // Yazar koruma tablosu → Romance override engeli → merge → blacklist pipeline
            // authorName, korunan yazar tespiti için ResolveFromCategories'e iletilir.
            string ResolveGenre(VolumeInfo v)
            {
                var authors = v.Authors is { Count: > 0 } ? string.Join(", ", v.Authors) : authorName;
                return GenreNormalizer.ResolveFromCategories(
                    v.Categories, v.Description, authorGenre, author: authors);
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

                    // Açıklama zorunlu — 50 karakterden kısa veya boş ise kitabı reddet
                    var description = v.Description?.Trim() ?? string.Empty;
                    if (description.Length < 50)
                    {
                        Console.WriteLine($"[Skip] \"{v.Title}\" - Sebep: Açıklama çok kısa veya eksik ({description.Length} karakter)");
                        return null;
                    }

                    // Yayınlanma tarihi
                    var publishedAt = DateTime.UtcNow;
                    if (!string.IsNullOrWhiteSpace(v.PublishedDate) &&
                        DateTime.TryParse(v.PublishedDate, out var parsed))
                        publishedAt = parsed;

                    // ISBN — ISBN-13 tercih edilir, yoksa ISBN-10, yoksa benzersiz placeholder
                    var isbn = v.IndustryIdentifiers
                        ?.FirstOrDefault(x => x.Type.Equals("ISBN_13", StringComparison.OrdinalIgnoreCase))
                        ?.Identifier
                        ?? v.IndustryIdentifiers
                            ?.FirstOrDefault(x => x.Type.Equals("ISBN_10", StringComparison.OrdinalIgnoreCase))
                            ?.Identifier;
                    if (string.IsNullOrWhiteSpace(isbn))
                        isbn = $"ISBN_MISSING_{item.Id}";

                    var thumbnailUrl = ResolveThumbnail(v.ImageLinks?.Thumbnail, isbn);

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
                        ThumbnailUrl = thumbnailUrl,
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
        catch (HttpRequestException)
        {
            throw; // 429 ve diğer HTTP hatalarını ProcessSearchAsync'e ilet (backoff için)
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SearchBooksAsync Hatası] {ex.Message}");
            return [];
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Thumbnail URL İşleme — Fallback Zinciri
    // ──────────────────────────────────────────────────────────────────────────

    // Google'ın gerçek görsel yerine döndürdüğü 'boş/kötü' URL parçaları
    private static readonly string[] BadThumbnailSignals =
    [
        "static.googleusercontent.com",  // gerçek kapak değil, genel placeholder
        "books.google.com/books?id=",     // kapak yok, genel kitap sayfası — görsel URL değil
        "img1.doubanio.com",              // Çin kaynağı, kapak değil
        "imnotabook",                     // Google'a özel 'görsel yok' işareti
        "no_cover",                       // açık 'kapağ yok' etiketi
        "missing_cover",                  // açık 'eksik kapak' etiketi
        "fife.googleusercontent.com",     // pikselli arka plan görselleri
    ];

    /// <summary>
    /// Thumbnail URL çözümleme zinciri:
    /// <list type="number">
    ///   <item>Google URL geçerliyse SanitizeThumbnail ile temizle.</item>
    ///   <item>URL null/boş veya 'kötü' domain içeriyorsa OpenLibrary'den ISBN ile kapak dene.</item>
    ///   <item>ISBN placeholder ise veya ISBN_MISSING ise direkt son placeholder'a düş.</item>
    ///   <item>Hiçbiri yoksa profesyonel placehold.co döner.</item>
    /// </list>
    /// </summary>
    internal static string ResolveThumbnail(string? rawGoogleUrl, string isbn)
    {
        var isBadOrMissing = string.IsNullOrWhiteSpace(rawGoogleUrl)
            || BadThumbnailSignals.Any(sig =>
                rawGoogleUrl!.Contains(sig, StringComparison.OrdinalIgnoreCase));

        if (!isBadOrMissing)
            return SanitizeThumbnail(rawGoogleUrl); // Google URL iyiyse sanitize edip dön

        // ISBN gerçekse OpenLibrary'den dene
        var cleanIsbn = isbn?.Trim();
        if (!string.IsNullOrWhiteSpace(cleanIsbn) &&
            !cleanIsbn.StartsWith("ISBN_MISSING", StringComparison.OrdinalIgnoreCase))
        {
            var openLibraryUrl = $"https://covers.openlibrary.org/b/isbn/{cleanIsbn}-L.jpg";
            Console.WriteLine($"[Thumbnail] Google görseli yok/kötü → OpenLibrary deneniyor: {openLibraryUrl}");
            return openLibraryUrl;
        }

        // Son çare: placehold.co
        return "https://placehold.co/128x192/1a1a2e/e0e0e0?text=No+Cover";
    }

    /// <summary>
    /// Ham Google Books thumbnail URL'ini Smart Sanitize ile temizler:
    /// <list type="bullet">
    ///   <item>OpenLibrary URL'lerine (covers.openlibrary.org) zoom/edge değişikliği YAPILMAZ.</item>
    ///   <item>Google URL'leri: zoom=1 → zoom=2, &amp;edge=curl kaldır, http → https.</item>
    ///   <item>Boş/null URL → placehold.co döner.</item>
    /// </list>
    /// </summary>
    internal static string SanitizeThumbnail(string? raw)
    {
        const string Placeholder =
            "https://placehold.co/128x192/1a1a2e/e0e0e0?text=No+Cover";

        if (string.IsNullOrWhiteSpace(raw))
            return Placeholder;

        var url = raw.Trim();

        // 1. http → https (her kaynak için zorunlu)
        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            url = "https://" + url["http://".Length..];

        // 2. OpenLibrary URL'lerine zoom/edge manipülasyonu yapma
        if (url.Contains("covers.openlibrary.org", StringComparison.OrdinalIgnoreCase))
            return url;

        // 3. Yalnızca Google URL'leri için: zoom=1 → zoom=2
        url = url.Replace("zoom=1", "zoom=2", StringComparison.OrdinalIgnoreCase);

        // 4. &edge=curl kaldır (kıvrık köşe efekti)
        url = url.Replace("&edge=curl", string.Empty, StringComparison.OrdinalIgnoreCase);

        return url;
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
