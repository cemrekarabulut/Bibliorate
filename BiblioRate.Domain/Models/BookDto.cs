using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BiblioRate.Domain.Models
{
    public class BookDto
    {
        [JsonPropertyName("id")]
        public int BookId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        // Çağlar'ın kodunda Array.isArray kontrolü olduğu için null dönmemesi önemli
        [JsonPropertyName("authors")]
        public List<string> Authors { get; set; } = new List<string>();

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("thumbnailUrl")]
        public string ThumbnailUrl { get; set; }

        [JsonPropertyName("ratingAvg")]
        public double RatingAvg { get; set; }

        [JsonPropertyName("ratingCount")]
        public int RatingCount { get; set; }
        
        // Opsiyonel: Berra'nın analizleri için kategoriyi de DTO'ya ekleyebiliriz
        [JsonPropertyName("categories")]
        public List<string> Categories { get; set; } = new List<string>();
    }
}