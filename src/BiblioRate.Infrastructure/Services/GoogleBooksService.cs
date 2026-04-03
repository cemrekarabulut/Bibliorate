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
        var url      = $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(query)}&maxResults=10";
        var response = await _httpClient.GetFromJsonAsync<GoogleBooksResponse>(url);

        if (response?.Items is null) return [];

        return response.Items.Select(item => new Book
        {
            Title        = item.VolumeInfo.Title,
            Author       = item.VolumeInfo.Authors is { Count: > 0 }
                               ? string.Join(", ", item.VolumeInfo.Authors)
                               : "Bilinmiyor",
            Isbn         = item.VolumeInfo.IndustryIdentifiers?
                               .FirstOrDefault()?.Identifier ?? "0000000000",
            Description  = item.VolumeInfo.Description ?? "Açıklama bulunmuyor.",
            ThumbnailUrl = item.VolumeInfo.ImageLinks?.Thumbnail ?? string.Empty,
            PublishedAt  = DateTime.TryParse(item.VolumeInfo.PublishedDate, out var date)
                               ? date
                               : DateTime.UtcNow
        });
    }
}

// Google Books API yanıtını okumak için yardımcı sınıflar
file sealed class GoogleBooksResponse { public List<GoogleBookItem>? Items { get; set; } }
file sealed class GoogleBookItem      { public VolumeInfo VolumeInfo { get; set; } = new(); }
file sealed class VolumeInfo
{
    public string             Title                 { get; set; } = string.Empty;
    public List<string>?      Authors               { get; set; }
    public string?            Description           { get; set; }
    public string?            PublishedDate         { get; set; }
    public List<IndustryId>?  IndustryIdentifiers   { get; set; }
    public ImageLinks?        ImageLinks            { get; set; }
}
file sealed class IndustryId  { public string Identifier { get; set; } = string.Empty; }
file sealed class ImageLinks  { public string? Thumbnail { get; set; } }
