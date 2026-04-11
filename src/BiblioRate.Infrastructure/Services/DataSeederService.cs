using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;

namespace BiblioRate.Infrastructure.Services;

public class DataSeederService
{
    private static readonly string[] CategoryKeywords =
    [
        "bilim kurgu",
        "roman",
        "psikoloji",
        "macera",
        "felsefe",
        "polisiye"
    ];

    private static readonly string[] AuthorNames =
    [
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

        foreach (var keyword in CategoryKeywords)
        {
            await ProcessSearchAsync(keyword, existingBooks, cancellationToken);
        }

        foreach (var author in AuthorNames)
        {
            var authorQuery = $"inauthor:\"{author}\"";
            await ProcessSearchAsync(authorQuery, existingBooks, cancellationToken);
        }
    }

    private async Task ProcessSearchAsync(string query, List<Book> existingBooks, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var fetchedBooks = await _googleBooksService.SearchBooksAsync(query);

        foreach (var fetchedBook in fetchedBooks)
        {
            if (BookExists(existingBooks, fetchedBook))
                continue;

            await _bookRepository.AddBookAsync(fetchedBook);
            existingBooks.Add(fetchedBook);
            Console.WriteLine($"[Saved] {fetchedBook.Title} - {fetchedBook.Author}");
        }

        await Task.Delay(5000, cancellationToken);
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

    private static string Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
}
