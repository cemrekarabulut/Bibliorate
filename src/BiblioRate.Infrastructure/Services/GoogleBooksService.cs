using System.Net.Http.Json;
using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;

namespace BiblioRate.Infrastructure.Services;

public class GoogleBooksService : IGoogleBooksService
{
    private readonly HttpClient _httpClient;

    public GoogleBooksService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<Book>> SearchBooksAsync(string query)
    {
        try
        {
            var url =
                $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(query)}&maxResults=10";
            var response = await _httpClient.GetFromJsonAsync<GoogleBooksResponse>(url);

            if (response?.Items is null) return [];

            static string ResolveGenre(VolumeInfo volumeInfo, string keyword)
            {
                if (volumeInfo.Categories is { Count: > 0 })
                {
                    var first = volumeInfo.Categories[0].Trim();
                    if (!string.IsNullOrWhiteSpace(first) &&
                        !first.Equals("Genel", StringComparison.OrdinalIgnoreCase))
                        return first;
                }

                return keyword;
            }

            static int ResolveYear(string? publishedDate)
            {
                if (string.IsNullOrWhiteSpace(publishedDate) || publishedDate.Length < 4)
                    return 0;

                var yearPart = publishedDate.Substring(0, 4);
                return int.TryParse(yearPart, out var year) ? year : 0;
            }

            Book? TryMapToBook(GoogleBookItem item, string keyword)
            {
                var description = item.VolumeInfo.Description?.Trim() ?? string.Empty;
                if (description.Length < 50)
                    return null;

                var publishedDate = item.VolumeInfo.PublishedDate;
                var publishedAt = DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(publishedDate) && DateTime.TryParse(publishedDate, out var parsed))
                    publishedAt = parsed;

                return new Book
                {
                    Title = item.VolumeInfo.Title,
                    Author = item.VolumeInfo.Authors is { Count: > 0 }
                        ? string.Join(", ", item.VolumeInfo.Authors)
                        : "Bilinmiyor",
                    Genre = ResolveGenre(item.VolumeInfo, keyword),
                    Year = ResolveYear(publishedDate),
                    Isbn = item.VolumeInfo.IndustryIdentifiers?.FirstOrDefault()?.Identifier ?? "0000000000",
                    Description = description,
                    ThumbnailUrl = item.VolumeInfo.ImageLinks?.Thumbnail ?? string.Empty,
                    PublishedAt = publishedAt,
                    GoogleBookId = string.IsNullOrWhiteSpace(item.Id) ? null : item.Id
                };
            }

            return response.Items
                .Select(item => TryMapToBook(item, query))
                .Where(book => book is not null)
                .Cast<Book>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Google Books SearchBooksAsync hatası: {ex}");
            return [];
        }
    }
}

// Google Books API yanıtını okumak için yardımcı sınıflar
file sealed class GoogleBooksResponse { public List<GoogleBookItem>? Items { get; set; } }

file sealed class GoogleBookItem
{
    public string Id { get; set; } = string.Empty;
    public VolumeInfo VolumeInfo { get; set; } = new();
}

file sealed class VolumeInfo
{
    public string Title { get; set; } = string.Empty;
    public List<string>? Authors { get; set; }
    public List<string>? Categories { get; set; }
    public string? Description { get; set; }
    public string? PublishedDate { get; set; }
    public List<IndustryId>? IndustryIdentifiers { get; set; }
    public ImageLinks? ImageLinks { get; set; }
}

file sealed class IndustryId { public string Identifier { get; set; } = string.Empty; }

file sealed class ImageLinks { public string? Thumbnail { get; set; } }
