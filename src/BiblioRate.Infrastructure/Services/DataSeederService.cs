using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using BiblioRate.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BiblioRate.Infrastructure.Services;

public class DataSeederService
{
    private readonly IGoogleBooksService _googleBooksService;
    private readonly IBookRepository _bookRepository;
    private readonly ApplicationDbContext _context;
    private readonly IBookQualityEvaluator _qualityEvaluator;
    private readonly IBookSimilarityScorer _similarityScorer; // FIX 1: Eklendi

    public DataSeederService(
        IGoogleBooksService googleBooksService,
        IBookRepository bookRepository,
        ApplicationDbContext context,
        IBookQualityEvaluator qualityEvaluator,
        IBookSimilarityScorer similarityScorer) // FIX 1: Constructor'a eklendi
    {
        _googleBooksService = googleBooksService;
        _bookRepository = bookRepository;
        _context = context;
        _qualityEvaluator = qualityEvaluator;
        _similarityScorer = similarityScorer;
    }

    private static readonly string[] BookQueries =
    [
        "Gone Girl Gillian Flynn", "Sharp Objects Gillian Flynn",
        "The Midnight Library Matt Haig", "How to Stop Time Matt Haig",
        "Chess Story Stefan Zweig", "Letter from an Unknown Woman Stefan Zweig",
        "Crime and Punishment Dostoyevsky", "The Idiot Dostoyevsky",
        "Harry Potter and the Philosophers Stone Rowling",
        "The Da Vinci Code Dan Brown", "Angels and Demons Dan Brown",
        "Of Mice and Men John Steinbeck", "The Grapes of Wrath John Steinbeck",
        "The Hunger Games Suzanne Collins", "Catching Fire Suzanne Collins",
        "The Adventures of Sherlock Holmes Arthur Conan Doyle",
        "The Shining Stephen King", "It Stephen King", "Misery Stephen King",
        "And Then There Were None Agatha Christie", "Murder on the Orient Express Agatha Christie",
        "Blindness Jose Saramago", "When Nietzsche Wept Irvin Yalom",
        "1984 George Orwell", "Animal Farm George Orwell",
        "The Hobbit Tolkien", "The Lord of the Rings Tolkien",
        "Great Expectations Charles Dickens", "A Tale of Two Cities Charles Dickens",
        "The Surgeon Tess Gerritsen", "Rizzoli and Isles Tess Gerritsen",
        "The Adventures of Huckleberry Finn Mark Twain",
        "Pride and Prejudice Jane Austen", "Emma Jane Austen",
        "Thats Not My Name Megan Lally",
        "The House Across the Lake Riley Sager", "Final Girls Riley Sager",
        "No Safe House Linwood Barclay", "Elevator Pitch Linwood Barclay",
        "The Housemaid Freida McFadden", "The Teacher Freida McFadden",
        "Les Miserables Victor Hugo", "Moby Dick Herman Melville",
        "Wuthering Heights Emily Bronte", "The Great Gatsby F Scott Fitzgerald",
        "The Picture of Dorian Gray Oscar Wilde",
        "A Flicker in the Dark Stacy Willingham", "Til Death Do Us Part Stacy Willingham",
        "The Book Thief Markus Zusak", "I Am the Messenger Markus Zusak",
        "Feed Them Silence Noelle Ihli", "Run All Night Noelle Ihli",
        "Trigger Wulf Dorn", "The Sinner Wulf Dorn",
        "I Am Still Alive Kate Alice Marshall", "Rules for Vanishing Kate Alice Marshall",
        "The One John Marrs", "The Passengers John Marrs",
        "Twilight Stephenie Meyer", "New Moon Stephenie Meyer",
        "Sometimes I Lie Alice Feeney", "Rock Paper Scissors Alice Feeney",
        "Fahrenheit 451 Ray Bradbury", "The Martian Chronicles Ray Bradbury",
        "Lord of the Flies William Golding", "My Left Foot Christy Brown"
    ];

    // FIX 2: QueryAuthorMap gerçek verilerle dolduruldu
    private static readonly Dictionary<string, string> QueryAuthorMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Gone Girl Gillian Flynn"]                          = "Gillian Flynn",
        ["Sharp Objects Gillian Flynn"]                      = "Gillian Flynn",
        ["The Midnight Library Matt Haig"]                   = "Matt Haig",
        ["How to Stop Time Matt Haig"]                       = "Matt Haig",
        ["Chess Story Stefan Zweig"]                         = "Stefan Zweig",
        ["Letter from an Unknown Woman Stefan Zweig"]        = "Stefan Zweig",
        ["Crime and Punishment Dostoyevsky"]                 = "Dostoyevsky",
        ["The Idiot Dostoyevsky"]                            = "Dostoyevsky",
        ["Harry Potter and the Philosophers Stone Rowling"]  = "J.K. Rowling",
        ["The Da Vinci Code Dan Brown"]                      = "Dan Brown",
        ["Angels and Demons Dan Brown"]                      = "Dan Brown",
        ["Of Mice and Men John Steinbeck"]                   = "John Steinbeck",
        ["The Grapes of Wrath John Steinbeck"]               = "John Steinbeck",
        ["The Hunger Games Suzanne Collins"]                 = "Suzanne Collins",
        ["Catching Fire Suzanne Collins"]                    = "Suzanne Collins",
        ["The Adventures of Sherlock Holmes Arthur Conan Doyle"] = "Arthur Conan Doyle",
        ["The Shining Stephen King"]                         = "Stephen King",
        ["It Stephen King"]                                  = "Stephen King",
        ["Misery Stephen King"]                              = "Stephen King",
        ["And Then There Were None Agatha Christie"]         = "Agatha Christie",
        ["Murder on the Orient Express Agatha Christie"]     = "Agatha Christie",
        ["Blindness Jose Saramago"]                          = "José Saramago",
        ["When Nietzsche Wept Irvin Yalom"]                  = "Irvin D. Yalom",
        ["1984 George Orwell"]                               = "George Orwell",
        ["Animal Farm George Orwell"]                        = "George Orwell",
        ["The Hobbit Tolkien"]                               = "Tolkien",
        ["The Lord of the Rings Tolkien"]                    = "Tolkien",
        ["Great Expectations Charles Dickens"]               = "Charles Dickens",
        ["A Tale of Two Cities Charles Dickens"]             = "Charles Dickens",
        ["The Surgeon Tess Gerritsen"]                       = "Tess Gerritsen",
        ["Rizzoli and Isles Tess Gerritsen"]                 = "Tess Gerritsen",
        ["The Adventures of Huckleberry Finn Mark Twain"]    = "Mark Twain",
        ["Pride and Prejudice Jane Austen"]                  = "Jane Austen",
        ["Emma Jane Austen"]                                 = "Jane Austen",
        ["Thats Not My Name Megan Lally"]                    = "Megan Lally",
        ["The House Across the Lake Riley Sager"]            = "Riley Sager",
        ["Final Girls Riley Sager"]                          = "Riley Sager",
        ["No Safe House Linwood Barclay"]                    = "Linwood Barclay",
        ["Elevator Pitch Linwood Barclay"]                   = "Linwood Barclay",
        ["The Housemaid Freida McFadden"]                    = "Freida McFadden",
        ["The Teacher Freida McFadden"]                      = "Freida McFadden",
        ["Les Miserables Victor Hugo"]                       = "Victor Hugo",
        ["Moby Dick Herman Melville"]                        = "Herman Melville",
        ["Wuthering Heights Emily Bronte"]                   = "Emily Bronte",
        ["The Great Gatsby F Scott Fitzgerald"]              = "F. Scott Fitzgerald",
        ["The Picture of Dorian Gray Oscar Wilde"]           = "Oscar Wilde",
        ["A Flicker in the Dark Stacy Willingham"]           = "Stacy Willingham",
        ["Til Death Do Us Part Stacy Willingham"]            = "Stacy Willingham",
        ["The Book Thief Markus Zusak"]                      = "Markus Zusak",
        ["I Am the Messenger Markus Zusak"]                  = "Markus Zusak",
        ["Feed Them Silence Noelle Ihli"]                    = "Noelle W. Ihli",
        ["Run All Night Noelle Ihli"]                        = "Noelle W. Ihli",
        ["Trigger Wulf Dorn"]                                = "Wulf Dorn",
        ["The Sinner Wulf Dorn"]                             = "Wulf Dorn",
        ["I Am Still Alive Kate Alice Marshall"]             = "Kate Alice Marshall",
        ["Rules for Vanishing Kate Alice Marshall"]          = "Kate Alice Marshall",
        ["The One John Marrs"]                               = "John Marrs",
        ["The Passengers John Marrs"]                        = "John Marrs",
        ["Twilight Stephenie Meyer"]                         = "Stephenie Meyer",
        ["New Moon Stephenie Meyer"]                         = "Stephenie Meyer",
        ["Sometimes I Lie Alice Feeney"]                     = "Alice Feeney",
        ["Rock Paper Scissors Alice Feeney"]                 = "Alice Feeney",
        ["Fahrenheit 451 Ray Bradbury"]                      = "Ray Bradbury",
        ["The Martian Chronicles Ray Bradbury"]              = "Ray Bradbury",
        ["Lord of the Flies William Golding"]                = "William Golding",
        ["My Left Foot Christy Brown"]                       = "Christy Brown",
    };

    private static readonly Dictionary<string, string> AuthorGenreMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Agatha Christie"]      = "Mystery & Thriller",
        ["Arthur Conan Doyle"]   = "Mystery & Thriller",
        ["Tess Gerritsen"]       = "Mystery & Thriller",
        ["Dan Brown"]            = "Mystery & Thriller",
        ["Riley Sager"]          = "Mystery & Thriller",
        ["Linwood Barclay"]      = "Mystery & Thriller",
        ["Gillian Flynn"]        = "Mystery & Thriller",
        ["Stacy Willingham"]     = "Mystery & Thriller",
        ["Wulf Dorn"]            = "Mystery & Thriller",
        ["Kate Alice Marshall"]  = "Mystery & Thriller",
        ["John Marrs"]           = "Mystery & Thriller",
        ["Alice Feeney"]         = "Mystery & Thriller",
        ["Noelle W. Ihli"]       = "Mystery & Thriller",
        ["Freida McFadden"]      = "Psychological Thriller",
        ["Megan Lally"]          = "Psychological Thriller",
        ["Tolkien"]              = "Fantasy",
        ["J.K. Rowling"]         = "Fantasy",
        ["Stephenie Meyer"]      = "Fantasy",
        ["George Orwell"]        = "Dystopian",
        ["Suzanne Collins"]      = "Dystopian",
        ["Ray Bradbury"]         = "Dystopian",
        ["Stephen King"]         = "Horror",
        ["Jane Austen"]          = "Romance",
        ["Emily Bronte"]         = "Romance",
        ["Oscar Wilde"]          = "Classics & Philosophy",
        ["Victor Hugo"]          = "Classics & Philosophy",
        ["Herman Melville"]      = "Classics & Philosophy",
        ["F. Scott Fitzgerald"]  = "Classics & Philosophy",
        ["Mark Twain"]           = "Classics & Philosophy",
        ["Charles Dickens"]      = "Classics & Philosophy",
        ["Stefan Zweig"]         = "Classics & Philosophy",
        ["Dostoyevsky"]          = "Classics & Philosophy",
        ["Irvin D. Yalom"]       = "Classics & Philosophy",
        ["William Golding"]      = "Classics & Philosophy",
        ["José Saramago"]        = "Classics & Philosophy",
        ["John Steinbeck"]       = "Drama",
        ["Matt Haig"]            = "Drama",
        ["Christy Brown"]        = "Drama",
        ["Markus Zusak"]         = "Drama",
    };

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // Seeder Lock: DB'de kayıt varsa seeder kendini anında durdurur (Safety Guard)
        if (await _context.Books.AnyAsync(cancellationToken))
        {
            Console.WriteLine("[Seeder] Safety Guard: DB'de mevcut kayıt tespit edildi. Otomatik veri pompalanması kilitlendi.");
            return;
        }

        var bookCount = await _context.Books.CountAsync(cancellationToken);
        Console.WriteLine($"[Seeder] Mevcut kitap sayısı: {bookCount}. Eksikler kontrol ediliyor...");

        var existingBooks = await _context.Books.AsNoTracking().ToListAsync(cancellationToken);
        var delayMs = 2000;

        foreach (var query in BookQueries)
        {
            var author = QueryAuthorMap.GetValueOrDefault(query, query);
            var genre = AuthorGenreMap.GetValueOrDefault(author, "Fiction");
            delayMs = await ProcessSearchAsync(query, author, genre, existingBooks, delayMs, cancellationToken);
        }
    }

    private async Task<int> ProcessSearchAsync(
        string query, string author, string genre,
        List<Book> existing, int delay, CancellationToken ct)
    {
        try
        {
            var fetched = await _googleBooksService.SearchBooksAsync(query, author, genre);
            foreach (var b in fetched)
            {
                if (IsDuplicate(b, existing)) continue;
                if ((b.Description?.Length ?? 0) < 50 || !IsGoodThumbnail(b.ThumbnailUrl)) continue;

                b.QualityScore = _qualityEvaluator.Evaluate(b);

                await _bookRepository.AddBookAsync(b);
                existing.Add(b);
                Console.WriteLine($"[Save] {b.Title} eklendi. (Kalite: {b.QualityScore})");
            }
            return 2000;
        }
        catch (Exception ex)
        {
            var newDelay = Math.Min(delay * 2, 60000);
            Console.WriteLine($"[Error] {query}: {ex.Message}. Delay: {newDelay / 1000}s");
            await Task.Delay(newDelay, ct);
            return newDelay;
        }
    }

    // FIX 1: IsDuplicate artık BookSimilarityScorer kullanıyor
    private bool IsDuplicate(Book incoming, List<Book> existing)
    {
        return existing.Any(e =>
            (incoming.Isbn != null && incoming.Isbn == e.Isbn) ||
            _similarityScorer.IsDuplicate(
                incoming.Title ?? "", incoming.Author ?? "",
                e.Title ?? "", e.Author ?? ""));
    }

    private static bool IsGoodThumbnail(string? url)
    {
        if (string.IsNullOrEmpty(url) || url.StartsWith("http://")) return false;
        string[] badKws = ["no_image", "placehold.co", "zoom=0", "edge=curl", "fife"];
        return !badKws.Any(kw => url.Contains(kw, StringComparison.OrdinalIgnoreCase));
    }

    // FIX 3: UpdateExistingCategoriesAsync artık tür normalizasyonu da yapıyor ve Levenshtein Dedup barındırıyor
    public async Task UpdateExistingCategoriesAsync(CancellationToken ct = default)
    {
        Console.WriteLine("[Cleanup] Başlatılıyor: Tür normalizasyonu, Dedup Motoru ve Kalite Skorları güncelleniyor...");

        var books = await _context.Books.Where(b => !b.IsDeleted).ToListAsync(ct);
        var booksToDelete = new HashSet<Book>();

        foreach (var book in books)
        {
            // Yazar adı normalizasyonu
            if (!string.IsNullOrEmpty(book.Author))
            {
                book.Author = NormalizeAuthor(book.Author);
                var normalizedGenre = AuthorGenreMap.GetValueOrDefault(book.Author);
                if (normalizedGenre != null)
                    book.Genre = normalizedGenre;
            }

            // Kalite skoru güncelle (dedup öncesi skorlar netleşsin)
            book.QualityScore = _qualityEvaluator.Evaluate(book);
        }

        // Smart Dedup Logic: Author gruplama üzerinden Gelişmiş Normalizasyon ve Levenshtein (%80) & Contains mantığı
        foreach (var group in books.GroupBy(b => b.Author))
        {
            var authorBooks = group.ToList();
            if (authorBooks.Count <= 1) continue;

            for (int i = 0; i < authorBooks.Count; i++)
            {
                var bookA = authorBooks[i];
                if (booksToDelete.Contains(bookA)) continue;

                var similarGroup = new List<Book> { bookA };
                var normTitleA = NormalizeTitleForDedup(bookA.Title, bookA.Author);

                for (int j = i + 1; j < authorBooks.Count; j++)
                {
                    var bookB = authorBooks[j];
                    if (booksToDelete.Contains(bookB)) continue;

                    var normTitleB = NormalizeTitleForDedup(bookB.Title, bookB.Author);

                    bool isDuplicate = false;
                    if (!string.IsNullOrEmpty(normTitleA) && !string.IsNullOrEmpty(normTitleB))
                    {
                        // 1. Contains mantığı
                        if (normTitleA.Contains(normTitleB) || normTitleB.Contains(normTitleA))
                        {
                            isDuplicate = true;
                        }
                        else
                        {
                            // 2. Saf Levenshtein %80 sınırı (Karakter farkına bakmaksızın)
                            int distance = GetLevenshteinDistance(normTitleA, normTitleB);
                            int maxLen = Math.Max(normTitleA.Length, normTitleB.Length);
                            double similarity = maxLen == 0 ? 1.0 : 1.0 - ((double)distance / maxLen);

                            if (similarity >= 0.80)
                            {
                                isDuplicate = true;
                            }
                        }
                    }

                    if (isDuplicate)
                    {
                        similarGroup.Add(bookB);
                    }
                }

                if (similarGroup.Count > 1)
                {
                    var bestBook = similarGroup.OrderByDescending(b => b.QualityScore).First();
                    foreach (var dup in similarGroup.Where(b => b.BookId != bestBook.BookId))
                    {
                        booksToDelete.Add(dup);
                    }
                }
            }
        }

        // Toplu (Batch) fiziksel silme
        if (booksToDelete.Count > 0)
        {
            _context.Books.RemoveRange(booksToDelete);
        }

        await _context.SaveChangesAsync(ct);
        Console.WriteLine($"[Cleanup] {books.Count} kitabın türü/skoru güncellendi. {booksToDelete.Count} kopya kalıcı olarak silindi.");
    }

    private static string NormalizeAuthor(string author)
    {
        if (string.IsNullOrWhiteSpace(author)) return "Bilinmeyen Yazar";

        var words = author.Split([' '], StringSplitOptions.RemoveEmptyEntries)
                          .Select(w => charToUpper(w))
                          .Distinct(StringComparer.OrdinalIgnoreCase)
                          .ToArray();

        return string.Join(" ", words);

        static string charToUpper(string input) =>
            input.Length switch
            {
                0 => "",
                1 => input.ToUpper(),
                _ => char.ToUpper(input[0]) + input.Substring(1).ToLower()
            };
    }

    private static string NormalizeTitleForDedup(string? title, string? author)
    {
        if (string.IsNullOrWhiteSpace(title)) return "";
        var t = title.ToLowerInvariant();
        
        if (!string.IsNullOrWhiteSpace(author))
        {
            var a = author.ToLowerInvariant();
            t = t.Replace(a, ""); // Yazar ismini başlıktan sil
        }

        t = t.Replace(" by ", " "); // " by " kelimesini sil
        
        string[] charsToRemove = [":", "-", "(", ")", ",", ".", "!", "?", "\"", "'"];
        foreach (var c in charsToRemove)
        {
            t = t.Replace(c, " ");
        }

        var words = t.Split([' '], StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", words);
    }

    private static int GetLevenshteinDistance(string a, string b)
    {
        if (string.IsNullOrEmpty(a)) return b?.Length ?? 0;
        if (string.IsNullOrEmpty(b)) return a.Length;

        int[] costs = new int[b.Length + 1];
        for (int i = 0; i <= b.Length; i++) costs[i] = i;

        for (int i = 1; i <= a.Length; i++)
        {
            int previousValue = costs[0];
            costs[0] = i;
            for (int j = 1; j <= b.Length; j++)
            {
                int currentValue = costs[j];
                costs[j] = Math.Min(Math.Min(costs[j - 1] + 1, costs[j] + 1),
                                    previousValue + (a[i - 1] == b[j - 1] ? 0 : 1));
                previousValue = currentValue;
            }
        }
        return costs[b.Length];
    }
}