using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using BiblioRate.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BiblioRate.Infrastructure.Services;

public class DataSeederService
{
    private static readonly string[] AuthorKeywords =
    [
        "Gillian Flynn",
        "Matt Haig",
        "Stefan Zweig",
        "Dostoyevsky",
        "J.K. Rowling",
        "Dan Brown",
        "John Steinbeck",
        "Suzanne Collins",
        "Arthur Conan Doyle",
        "Stephen King",
        "Agatha Christie",
        "José Saramago",
        "Irvin D. Yalom",
        "George Orwell",
        "Tolkien",
        "Charles Dickens",
        "Tess Gerritsen",
        "Mark Twain",
        "Jane Austen",
        "Megan Lally",
        "Riley Sager",
        "Linwood Barclay",
        "Freida McFadden",
        "Victor Hugo",
        "Herman Melville",
        "Emily Bronte",
        "F. Scott Fitzgerald",
        "Oscar Wilde"
    ];

    /// <summary>
    /// Kitap adı + yazar adı sorgu listesi. Her sorgu Google'a doğrudan gönderilir.
    /// </summary>
    private static readonly string[] BookQueries =
    [
        "Gone Girl Gillian Flynn",              "Sharp Objects Gillian Flynn",
        "The Midnight Library Matt Haig",       "How to Stop Time Matt Haig",
        "Chess Story Stefan Zweig",             "Letter from an Unknown Woman Stefan Zweig",
        "Crime and Punishment Dostoyevsky",     "The Idiot Dostoyevsky",
        "Harry Potter and the Philosophers Stone Rowling",
        "The Da Vinci Code Dan Brown",          "Angels and Demons Dan Brown",
        "Of Mice and Men John Steinbeck",       "The Grapes of Wrath John Steinbeck",
        "The Hunger Games Suzanne Collins",     "Catching Fire Suzanne Collins",
        "The Adventures of Sherlock Holmes Arthur Conan Doyle",
        "The Shining Stephen King",             "It Stephen King",             "Misery Stephen King",
        "And Then There Were None Agatha Christie", "Murder on the Orient Express Agatha Christie",
        "Blindness Jose Saramago",              "When Nietzsche Wept Irvin Yalom",
        "1984 George Orwell",                   "Animal Farm George Orwell",
        "The Hobbit Tolkien",                   "The Lord of the Rings Tolkien",
        "Great Expectations Charles Dickens",   "A Tale of Two Cities Charles Dickens",
        "The Surgeon Tess Gerritsen",           "Rizzoli and Isles Tess Gerritsen",
        "The Adventures of Huckleberry Finn Mark Twain",
        "Pride and Prejudice Jane Austen",      "Emma Jane Austen",
        "Thats Not My Name Megan Lally",
        "The House Across the Lake Riley Sager", "Final Girls Riley Sager",
        "No Safe House Linwood Barclay",        "Elevator Pitch Linwood Barclay",
        "The Housemaid Freida McFadden",        "The Teacher Freida McFadden",
        "Les Miserables Victor Hugo",           "Moby Dick Herman Melville",
        "Wuthering Heights Emily Bronte",       "The Great Gatsby F Scott Fitzgerald",
        "The Picture of Dorian Gray Oscar Wilde"
    ];

    /// <summary>
    /// Sorgu → Yazar adı eşleşmesi. AuthorGenreMap ile birleşerek doğru türü bulur.
    /// Her sorgu için yazarların kısa/tanınan adları kullanılır.
    /// </summary>
    private static readonly Dictionary<string, string> QueryAuthorMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Gone Girl Gillian Flynn"]                             = "Gillian Flynn",
        ["Sharp Objects Gillian Flynn"]                         = "Gillian Flynn",
        ["The Midnight Library Matt Haig"]                      = "Matt Haig",
        ["How to Stop Time Matt Haig"]                          = "Matt Haig",
        ["Chess Story Stefan Zweig"]                            = "Stefan Zweig",
        ["Letter from an Unknown Woman Stefan Zweig"]           = "Stefan Zweig",
        ["Crime and Punishment Dostoyevsky"]                    = "Dostoyevsky",
        ["The Idiot Dostoyevsky"]                               = "Dostoyevsky",
        ["Harry Potter and the Philosophers Stone Rowling"]     = "J.K. Rowling",
        ["The Da Vinci Code Dan Brown"]                         = "Dan Brown",
        ["Angels and Demons Dan Brown"]                         = "Dan Brown",
        ["Of Mice and Men John Steinbeck"]                      = "John Steinbeck",
        ["The Grapes of Wrath John Steinbeck"]                  = "John Steinbeck",
        ["The Hunger Games Suzanne Collins"]                    = "Suzanne Collins",
        ["Catching Fire Suzanne Collins"]                       = "Suzanne Collins",
        ["The Adventures of Sherlock Holmes Arthur Conan Doyle"]= "Arthur Conan Doyle",
        ["The Shining Stephen King"]                            = "Stephen King",
        ["It Stephen King"]                                     = "Stephen King",
        ["Misery Stephen King"]                                 = "Stephen King",
        ["And Then There Were None Agatha Christie"]            = "Agatha Christie",
        ["Murder on the Orient Express Agatha Christie"]        = "Agatha Christie",
        ["Blindness Jose Saramago"]                             = "José Saramago",
        ["When Nietzsche Wept Irvin Yalom"]                     = "Irvin D. Yalom",
        ["1984 George Orwell"]                                  = "George Orwell",
        ["Animal Farm George Orwell"]                           = "George Orwell",
        ["The Hobbit Tolkien"]                                  = "Tolkien",
        ["The Lord of the Rings Tolkien"]                       = "Tolkien",
        ["Great Expectations Charles Dickens"]                  = "Charles Dickens",
        ["A Tale of Two Cities Charles Dickens"]                = "Charles Dickens",
        ["The Surgeon Tess Gerritsen"]                          = "Tess Gerritsen",
        ["Rizzoli and Isles Tess Gerritsen"]                    = "Tess Gerritsen",
        ["The Adventures of Huckleberry Finn Mark Twain"]       = "Mark Twain",
        ["Pride and Prejudice Jane Austen"]                     = "Jane Austen",
        ["Emma Jane Austen"]                                    = "Jane Austen",
        ["Thats Not My Name Megan Lally"]                       = "Megan Lally",
        ["The House Across the Lake Riley Sager"]               = "Riley Sager",
        ["Final Girls Riley Sager"]                             = "Riley Sager",
        ["No Safe House Linwood Barclay"]                       = "Linwood Barclay",
        ["Elevator Pitch Linwood Barclay"]                      = "Linwood Barclay",
        ["The Housemaid Freida McFadden"]                       = "Freida McFadden",
        ["The Teacher Freida McFadden"]                         = "Freida McFadden",
        ["Les Miserables Victor Hugo"]                          = "Victor Hugo",
        ["Moby Dick Herman Melville"]                           = "Herman Melville",
        ["Wuthering Heights Emily Bronte"]                      = "Emily Bronte",
        ["The Great Gatsby F Scott Fitzgerald"]                 = "F. Scott Fitzgerald",
        ["The Picture of Dorian Gray Oscar Wilde"]              = "Oscar Wilde",
    };

    /// <summary>
    /// Yazar → Tür eşleşmesi. Google 'Fiction' veya 'General' dönerse bu sözlük devreye girer.
    /// </summary>
    private static readonly Dictionary<string, string> AuthorGenreMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // Mystery
        ["Agatha Christie"]      = "Mystery",
        ["Arthur Conan Doyle"]   = "Mystery",

        // Thriller
        ["Tess Gerritsen"]       = "Thriller",
        ["Dan Brown"]            = "Thriller",
        ["Riley Sager"]          = "Thriller",
        ["Linwood Barclay"]      = "Thriller",
        ["Gillian Flynn"]        = "Thriller",

        // Psychological Thriller
        ["Freida McFadden"]      = "Psychological Thriller",
        ["Megan Lally"]          = "Psychological Thriller",

        // Fantasy
        ["Tolkien"]              = "Fantasy",
        ["J.K. Rowling"]         = "Fantasy",

        // Dystopian
        ["George Orwell"]        = "Dystopian",
        ["Suzanne Collins"]      = "Dystopian",

        // Horror
        ["Stephen King"]         = "Horror",

        // Classic
        ["Jane Austen"]          = "Classic",
        ["Charles Dickens"]      = "Classic",
        ["Oscar Wilde"]          = "Classic",
        ["Victor Hugo"]          = "Classic",
        ["Herman Melville"]      = "Classic",
        ["Emily Bronte"]         = "Classic",
        ["F. Scott Fitzgerald"]  = "Classic",
        ["Mark Twain"]           = "Classic",

        // Philosophy
        ["Dostoyevsky"]          = "Philosophy",
        ["Stefan Zweig"]         = "Philosophy",
        ["Irvin D. Yalom"]       = "Philosophy",

        // Drama
        ["John Steinbeck"]       = "Drama",
        ["José Saramago"]        = "Drama",
        ["Matt Haig"]            = "Drama",
    };

    private readonly IGoogleBooksService _googleBooksService;
    private readonly IBookRepository _bookRepository;
    private readonly ApplicationDbContext _context;

    public DataSeederService(
        IGoogleBooksService googleBooksService,
        IBookRepository bookRepository,
        ApplicationDbContext context)
    {
        _googleBooksService = googleBooksService;
        _bookRepository = bookRepository;
        _context = context;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // Mevcut kitapları yükle — duplikat kontrolü için ve kaldığı yerden devam etmek için
        var existingBooks = await _context.Books.ToListAsync(cancellationToken);
        Console.WriteLine($"[Seeder] Veritabanında {existingBooks.Count} kitap mevcut. Eksikler tamamlanıyor...");

        foreach (var bookQuery in BookQueries)
        {
            // Sorgudan yazara, yazardan türe ulaş
            var author = QueryAuthorMap.TryGetValue(bookQuery, out var a) ? a : bookQuery;
            var genre  = AuthorGenreMap.TryGetValue(author,    out var g) ? g : "General";
            await ProcessSearchAsync(bookQuery, author, genre, existingBooks, cancellationToken);
        }

        Console.WriteLine("[Seeder] Seed işlemi başarıyla tamamlandı.");
    }

    private async Task ProcessSearchAsync(
        string query,
        string authorName,
        string authorGenre,
        List<Book> existingBooks,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var fetchedBooks = await _googleBooksService.SearchBooksAsync(query, authorName, authorGenre);

            foreach (var fetchedBook in fetchedBooks)
            {
                if (BookExists(existingBooks, fetchedBook))
                    continue;

                Console.WriteLine($"[Attempting to Save] {fetchedBook.Title}");
                await _bookRepository.AddBookAsync(fetchedBook);
                existingBooks.Add(fetchedBook);
                Console.WriteLine($"[Saved] {fetchedBook.Title} - {fetchedBook.Author} (Genre: {fetchedBook.Genre})");
            }
        }
        catch (Exception ex)
        {
            // 503 veya diğer hatalarda uygulamanın çökmesini engeller, log yazar ve beklemeye geçer
            Console.WriteLine($"[Error] {query} aranırken bir sorun oluştu: {ex.Message}");
        }

        // --- RATE LIMIT GÜNCELLEMESİ ---
        // Google'ın IP banlamasını önlemek için 10 saniye bekliyoruz.
        Console.WriteLine($"[Wait] Google API'yi dinlendirmek için 10 saniye bekleniyor...");
        await Task.Delay(10000, cancellationToken);
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