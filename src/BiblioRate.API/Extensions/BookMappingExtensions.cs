using BiblioRate.Domain.Entities;
using BiblioRate.Domain.Models;

namespace BiblioRate.API.Extensions;

/// <summary>
/// Book → BookDto dönüşümünü tek yerden yönetir.
/// BooksController ve FavoritesController'daki tekrarlanan mapping kodunu ortadan kaldırır.
/// </summary>
public static class BookMappingExtensions
{
    public static BookDto ToDto(this Book book, double ratingAvg = 0.0, int ratingCount = 0)
    {
        return new BookDto
        {
            BookId       = book.BookId,
            Title        = book.Title,
            Authors      = string.IsNullOrEmpty(book.Author)
                               ? []
                               : [.. book.Author.Split(',', StringSplitOptions.TrimEntries)],
            Description  = book.Description,
            ThumbnailUrl = book.ThumbnailUrl,
            RatingAvg    = ratingAvg,
            RatingCount  = ratingCount
        };
    }
}
