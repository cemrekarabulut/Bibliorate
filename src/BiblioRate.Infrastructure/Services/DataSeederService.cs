using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;

namespace BiblioRate.Infrastructure.Services;

public class DataSeederService
{
    private static readonly string[] DefaultKeywords =
    [
        "bilim kurgu",
        "roman",
        "psikoloji",
        "macera",
        "felsefe",
        "polisiye",
        "Alice Feeney",
        "Freida McFadden",
        "Tess Gerritsen"
    ];

    private readonly IGoogleBooksService _googleBooksService;
    private readonly IBookRepository _bookRepository;

    public DataSeederService(IGoogleBooksService googleBooksService, IBookRepository bookRepository)
    {
        _googleBooksService = googleBooksService;
        _bookRepository = bookRepository;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var existingBooks = (await _bookRepository.GetAllBooksAsync()).ToList();

        foreach (var keyword in DefaultKeywords)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fetchedBooks = await _googleBooksService.SearchBooksAsync(keyword);
            foreach (var fetchedBook in fetchedBooks)
            {
                if (BookExists(existingBooks, fetchedBook))
                {
                    continue;
                }

                await _bookRepository.AddBookAsync(fetchedBook);
                existingBooks.Add(fetchedBook);
            }

            await Task.Delay(2000);
        }
    }

    private static bool BookExists(IEnumerable<Book> existingBooks, Book candidate)
    {
        var candidateIsbn = Normalize(candidate.Isbn);
        var candidateTitle = Normalize(candidate.Title);
        var candidateAuthor = Normalize(candidate.Author);

        return existingBooks.Any(book =>
        {
            var bookIsbn = Normalize(book.Isbn);
            if (!string.IsNullOrWhiteSpace(candidateIsbn) &&
                candidateIsbn != "0000000000" &&
                bookIsbn == candidateIsbn)
            {
                return true;
            }

            return Normalize(book.Title) == candidateTitle &&
                   Normalize(book.Author) == candidateAuthor;
        });
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }
}
