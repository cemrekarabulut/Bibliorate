using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        var url = $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(query)}&maxResults=10";
        var response = await _httpClient.GetFromJsonAsync<GoogleBooksResponse>(url);

        if (response?.Items == null) return Enumerable.Empty<Book>();

        return response.Items.Select(item => new Book
        {
            Title = item.VolumeInfo.Title,
            Author = item.VolumeInfo.Authors != null ? string.Join(", ", item.VolumeInfo.Authors) : "Bilinmiyor",
            Isbn = item.VolumeInfo.IndustryIdentifiers?.FirstOrDefault()?.Identifier ?? "0000000000",
            Description = item.VolumeInfo.Description ?? "Açıklama bulunmuyor.",
            PublishedAt = DateTime.TryParse(item.VolumeInfo.PublishedDate, out var date) ? date : DateTime.Now
            // Berra'nın SQL yapısına göre diğer alanları buraya ekleyebiliriz
        });
    }
}

// Google'dan gelen karmaşık JSON'ı okumak için basit yardımcı sınıflar
public class GoogleBooksResponse { public List<GoogleBookItem>? Items { get; set; } }
public class GoogleBookItem { public VolumeInfo VolumeInfo { get; set; } = new(); }
public class VolumeInfo { 
    public string Title { get; set; } = ""; 
    public List<string>? Authors { get; set; }
    public string? Description { get; set; }
    public string? PublishedDate { get; set; }
    public List<IndustryIdentifier>? IndustryIdentifiers { get; set; }
}
public class IndustryIdentifier { public string Identifier { get; set; } = ""; }