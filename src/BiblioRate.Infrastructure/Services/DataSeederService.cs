using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;

namespace BiblioRate.Infrastructure.Services;

public class DataSeederService
{
    private static readonly string[] AuthorKeywords =
    [
        "inauthor:\"Sabahattin Ali\"",
        "inauthor:\"Zülfü Livaneli\"",
        "inauthor:\"Stefan Zweig\"",
        "inauthor:\"Dostoyevsky\"",
        "inauthor:\"J.K. Rowling\"",
        "inauthor:\"Dan Brown\"",
        "inauthor:\"John Steinbeck\"",
        "inauthor:\"Suzanne Collins\"",
        "inauthor:\"Arthur Conan Doyle\"",
        "inauthor:\"Stephen King\"",
        "inauthor:\"Agatha Christie\"",
        "inauthor:\"Ahmet Ümit\"",
        "inauthor:\"Yaşar Kemal\"",
        "inauthor:\"George Orwell\"",
        "inauthor:\"Tolkien\"",
        "inauthor:\"Charles Dickens\"",
        "inauthor:\"Oscar Wilde\""
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

        foreach (var authorKeyword in AuthorKeywords)
        {
            await ProcessSearchAsync(authorKeyword, existingBooks, cancellationToken);
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
