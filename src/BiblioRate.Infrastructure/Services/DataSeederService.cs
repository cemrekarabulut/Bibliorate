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
        "Oscar Wilde",
        // --- Yeni yazarlar ---
        "Stacy Willingham",
        "Markus Zusak",
        "Noelle W. Ihli",
        "Wulf Dorn",
        "Kate Alice Marshall",
        "John Marrs",
        "Stephenie Meyer",
        "Alice Feeney",
        "Ray Bradbury",
        "William Golding",
        "Christy Brown"
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
        "The Picture of Dorian Gray Oscar Wilde",
        // --- Yeni sorgular ---
        "A Flicker in the Dark Stacy Willingham", "Til Death Do Us Part Stacy Willingham",
        "The Book Thief Markus Zusak",          "I Am the Messenger Markus Zusak",
        "Feed Them Silence Noelle Ihli",        "Run All Night Noelle Ihli",
        "Trigger Wulf Dorn",                    "The Sinner Wulf Dorn",
        "I Am Still Alive Kate Alice Marshall", "Rules for Vanishing Kate Alice Marshall",
        "The One John Marrs",                   "The Passengers John Marrs",
        "Twilight Stephenie Meyer",             "New Moon Stephenie Meyer",
        "Sometimes I Lie Alice Feeney",         "Rock Paper Scissors Alice Feeney",
        "Fahrenheit 451 Ray Bradbury",          "The Martian Chronicles Ray Bradbury",
        "Lord of the Flies William Golding",
        "My Left Foot Christy Brown"
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
        // --- Yeni yazar sorguları ---
        ["A Flicker in the Dark Stacy Willingham"]              = "Stacy Willingham",
        ["Til Death Do Us Part Stacy Willingham"]               = "Stacy Willingham",
        ["The Book Thief Markus Zusak"]                         = "Markus Zusak",
        ["I Am the Messenger Markus Zusak"]                     = "Markus Zusak",
        ["Feed Them Silence Noelle Ihli"]                       = "Noelle W. Ihli",
        ["Run All Night Noelle Ihli"]                           = "Noelle W. Ihli",
        ["Trigger Wulf Dorn"]                                   = "Wulf Dorn",
        ["The Sinner Wulf Dorn"]                                = "Wulf Dorn",
        ["I Am Still Alive Kate Alice Marshall"]                = "Kate Alice Marshall",
        ["Rules for Vanishing Kate Alice Marshall"]             = "Kate Alice Marshall",
        ["The One John Marrs"]                                  = "John Marrs",
        ["The Passengers John Marrs"]                           = "John Marrs",
        ["Twilight Stephenie Meyer"]                            = "Stephenie Meyer",
        ["New Moon Stephenie Meyer"]                            = "Stephenie Meyer",
        ["Sometimes I Lie Alice Feeney"]                        = "Alice Feeney",
        ["Rock Paper Scissors Alice Feeney"]                    = "Alice Feeney",
        ["Fahrenheit 451 Ray Bradbury"]                         = "Ray Bradbury",
        ["The Martian Chronicles Ray Bradbury"]                 = "Ray Bradbury",
        ["Lord of the Flies William Golding"]                   = "William Golding",
        ["My Left Foot Christy Brown"]                          = "Christy Brown",
    };

    /// <summary>
    /// Yazar → Tür eşleşmesi. Google 'Fiction' veya 'General' dönerse bu sözlük devreye girer.
    /// GenreNormalizer.AuthorProtectedGenres ile senkronize tutulur.
    /// </summary>
    private static readonly Dictionary<string, string> AuthorGenreMap =
        new(StringComparer.OrdinalIgnoreCase)
    {
        // Mystery & Thriller
        ["Agatha Christie"]     = "Mystery & Thriller",
        ["Arthur Conan Doyle"]  = "Mystery & Thriller",
        ["Tess Gerritsen"]      = "Mystery & Thriller",
        ["Dan Brown"]           = "Mystery & Thriller",
        ["Riley Sager"]         = "Mystery & Thriller",
        ["Linwood Barclay"]     = "Mystery & Thriller",
        ["Gillian Flynn"]       = "Mystery & Thriller",
        ["Stacy Willingham"]    = "Mystery & Thriller",
        ["Wulf Dorn"]           = "Mystery & Thriller",
        ["Kate Alice Marshall"] = "Mystery & Thriller",
        ["John Marrs"]          = "Mystery & Thriller",
        ["Alice Feeney"]        = "Mystery & Thriller",
        ["Noelle W. Ihli"]      = "Mystery & Thriller",

        // Psychological Thriller
        ["Freida McFadden"]     = "Psychological Thriller",
        ["Megan Lally"]         = "Psychological Thriller",

        // Fantasy
        ["Tolkien"]             = "Fantasy",
        ["J.K. Rowling"]        = "Fantasy",
        ["Stephenie Meyer"]     = "Fantasy",

        // Dystopian
        ["George Orwell"]       = "Dystopian",
        ["Suzanne Collins"]     = "Dystopian",
        ["Ray Bradbury"]        = "Dystopian",

        // Horror
        ["Stephen King"]        = "Horror",

        // Romance
        ["Jane Austen"]         = "Romance",
        ["Emily Bronte"]        = "Romance",

        // Classics & Philosophy — 'Classic' kategorisi buraya merge edildi
        ["Oscar Wilde"]         = "Classics & Philosophy",
        ["Victor Hugo"]         = "Classics & Philosophy",
        ["Herman Melville"]     = "Classics & Philosophy",
        ["F. Scott Fitzgerald"] = "Classics & Philosophy",
        ["Mark Twain"]          = "Classics & Philosophy",
        ["Charles Dickens"]     = "Classics & Philosophy",
        ["Stefan Zweig"]        = "Classics & Philosophy",
        ["Dostoyevsky"]         = "Classics & Philosophy",
        ["Irvin D. Yalom"]      = "Classics & Philosophy",
        ["William Golding"]     = "Classics & Philosophy",
        ["José Saramago"]       = "Classics & Philosophy",

        // Drama — 'Historical Fiction' ve 'Biography' buraya merge edildi
        ["John Steinbeck"]      = "Drama",
        ["Matt Haig"]           = "Drama",
        ["Christy Brown"]       = "Drama",
        ["Markus Zusak"]        = "Drama",
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
        // ── DB'deki mevcut kitapları başlangıçta belleğe al ──
        // Erken return YOK — her sorguda yalnızca eksik kitaplar eklenir (incremental seed).
        var bookCount = await _context.Books.CountAsync(cancellationToken);
        Console.WriteLine($"[Seeder] Veritabanında {bookCount} kitap mevcut. Eksik kitaplar kontrol ediliyor...");

        // Mevcut kitapları belleğe al — BookExists karşılaştırması bu liste üzerinden yapılır
        var existingBooks = await _context.Books
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Adaptive delay: başarılı kayıtta 2 sn, hata/429'da kademeli artış
        var delayMs = DelayNormalMs;

        foreach (var bookQuery in BookQueries)
        {
            // TryGetValue/out yok — GetValueOrDefault ile async-uyumlu erişim
            var author = QueryAuthorMap.GetValueOrDefault(bookQuery, bookQuery);
            var genre  = AuthorGenreMap.GetValueOrDefault(author,    "Fiction");
            delayMs = await ProcessSearchAsync(bookQuery, author, genre, existingBooks, delayMs, cancellationToken);
        }

        Console.WriteLine("[Seeder] Seed işlemi başarıyla tamamlandı.");
    }

    // ── Adaptive delay sabitleri ──────────────────────────────────────────────
    private const int DelayNormalMs  =  2_000;  // Başarılı akışta bekleme (2 sn)
    private const int DelayStep1Ms   =  5_000;  // İlk hata sonrası (5 sn)
    private const int DelayStep2Ms   = 15_000;  // İkinci hata sonrası (15 sn)
    private const int DelayStep3Ms   = 30_000;  // Üçüncü hata sonrası (30 sn)
    private const int DelayMaxMs     = 60_000;  // Maksimum bekleme (60 sn)

    // async metotlarda 'ref' yasak (CS1988) — delayMs değeri return değeri olarak aktarılır.
    private async Task<int> ProcessSearchAsync(
        string query,
        string authorName,
        string authorGenre,
        List<Book> existingBooks,
        int delayMs,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var savedAny = false;

        try
        {
            var fetchedBooks = await _googleBooksService.SearchBooksAsync(query, authorName, authorGenre);

            foreach (var fetchedBook in fetchedBooks)
            {
                // ── Sıkı Giriş Denetimi (The Great Guard) ──────────────────────
                // Kural 1: Title veya Author boş olan kitabı kaydetme
                if (string.IsNullOrWhiteSpace(fetchedBook.Title))
                {
                    Console.WriteLine("[Guard] Atlandı (title boş): başlıksız kitap");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(fetchedBook.Author) ||
                    fetchedBook.Author.Equals("Unknown Author", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"[Guard] Atlandı (yazar boş/bilinmiyor): \"{fetchedBook.Title}\"");
                    continue;
                }

                // Kural 2: ISBN/Başlık+Yazar bazlı kesin kopya (DB + bu session'da eklenenler)
                if (BookExists(existingBooks, fetchedBook))
                {
                    Console.WriteLine($"[Guard] Atlandı (DB'de mevcut): \"{fetchedBook.Title}\"");
                    continue;
                }

                // Kural 3: Fuzzy başlık kopyası — "A" kitabının başlığı mevcut "B" kitabının
                // içinde geçiyor ya da tersi (örn. "1984" ↔ "1984 Large Print")
                var normCandidate = NormTitle(fetchedBook.Title);
                var fuzzyDuplicate = existingBooks.Any(ex =>
                {
                    var normEx = NormTitle(ex.Title);
                    return normEx == normCandidate ||
                           normEx.Contains(normCandidate, StringComparison.Ordinal) ||
                           normCandidate.Contains(normEx, StringComparison.Ordinal);
                });
                if (fuzzyDuplicate)
                {
                    Console.WriteLine($"[Guard] Atlandı (fuzzy kopya): \"{fetchedBook.Title}\"");
                    continue;
                }

                // Kural 4: Açıklama eksik veya çok kısa (ikinci savunma hattı)
                if (string.IsNullOrWhiteSpace(fetchedBook.Description) ||
                    fetchedBook.Description.Length < 50)
                {
                    Console.WriteLine($"[Guard] Atlandı (kısa açıklama): \"{fetchedBook.Title}\"");
                    continue;
                }

                // Kural 5: Görsel kalitesi — placehold.co, no_image sinyali veya geçersiz URL
                if (!IsGoodThumbnail(fetchedBook.ThumbnailUrl))
                {
                    Console.WriteLine($"[Guard] Atlandı (görsel yok/geçersiz): \"{fetchedBook.Title}\"");
                    continue;
                }

                // Tüm denetimlerden geçti — kaydet
                Console.WriteLine($"[Save] \"{fetchedBook.Title}\" - {fetchedBook.Author} (Genre: {fetchedBook.Genre})");
                await _bookRepository.AddBookAsync(fetchedBook);
                existingBooks.Add(fetchedBook);  // aynı session'da mükerer eklemeyi engeller
                savedAny = true;
            }

            // Başarılı akış: delay'i normal değere sıfırla
            if (delayMs > DelayNormalMs)
            {
                Console.WriteLine($"[Wait] Başarılı yanıt — delay {delayMs / 1000}sn → {DelayNormalMs / 1000}sn'ye sıfırlandı.");
                delayMs = DelayNormalMs;
            }
        }
        catch (HttpRequestException httpEx) when (
            httpEx.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
            httpEx.Message.Contains("429", StringComparison.Ordinal))
        {
            // 429 Too Many Requests — delay'i kademeli artır
            delayMs = delayMs switch
            {
                <= DelayNormalMs => DelayStep1Ms,
                <= DelayStep1Ms  => DelayStep2Ms,
                <= DelayStep2Ms  => DelayStep3Ms,
                _                => DelayMaxMs
            };
            Console.WriteLine($"[RateLimit] 429 alındı — \"{query}\" sorgusu için delay {delayMs / 1000}sn'ye artırıldı.");
        }
        catch (Exception ex)
        {
            // Diğer hatalar: delay'i bir adım artır ama sessizce devam et
            delayMs = Math.Min(delayMs * 2, DelayMaxMs);
            Console.WriteLine($"[Error] \"{query}\" aranırken hata: {ex.Message} (delay → {delayMs / 1000}sn)");
        }

        var waitLabel = savedAny ? "yeni kayıt eklendi" : "değişiklik yok";
        Console.WriteLine($"[Wait] \"{query}\" tamamlandı ({waitLabel}). {delayMs / 1000}sn bekleniyor...");
        await Task.Delay(delayMs, cancellationToken);

        return delayMs;  // güncel değeri çağırana geri taşı
    }

    // Başlık normalizasyonu — fuzzy karşılaştırma için yardımcı (ProcessSearchAsync içi)
    private static string NormTitle(string? t) =>
        string.IsNullOrWhiteSpace(t)
            ? string.Empty
            : new string(t.ToLowerInvariant()
                          .Where(c => char.IsLetterOrDigit(c) || c == ' ')
                          .ToArray()).Trim();

    // ──────────────────────────────────────────────────────────────────────────
    // Mevcut Kitap Kategori Güncellemesi + Akıllı Tekilleştirme
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Veritabanını iki aşamada süpürür:
    /// <list type="number">
    ///   <item>
    ///     <b>Fuzzy Deduplication</b> — tam eşleşmenin yanı sıra "A başlığı B'nin içinde geçiyorsa"
    ///     (ör. "1984" ve "1984 Large Print") aynı grup sayılır. Her gruptan en güçlü kaydı tutar,
    ///     kalanları kalıcı olarak siler.
    ///   </item>
    ///   <item>
    ///     <b>Genre Normalizasyonu</b> — yazar koruma tablosu, Romance tespiti, merge kuralları ve
    ///     blacklist pipeline'ına göre Genre alanını günceller.
    ///   </item>
    /// </list>
    /// Her startup'ta çalışır; idempotent — ikinci çalışmada silinecek/değişecek kayıt kalmaz.
    /// </summary>
    public async Task UpdateExistingCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        var books = await _context.Books
            .Include(b => b.Ratings)
            .Include(b => b.Reviews)
            .Include(b => b.Favorites)
            .ToListAsync(cancellationToken);

        var initialCount = books.Count;
        Console.WriteLine($"[Cleanup] ══════════════════════════════════════");
        Console.WriteLine($"[Cleanup] Başlangıç: {initialCount} kitap.");
        Console.WriteLine($"[Cleanup] ══════════════════════════════════════");

        // ── Aşama 0: Radikal Temizlik (Purge) ────────────────────────────────
        // Açıklaması çok kısa veya gerçek kapak görseli olmayan kayıtları sil.
        // Bu kontrol deduplication'dan önce yapılır; eğer bir kopyanın HEPSI kötüyse
        // hepsi silinir — hiçbiri kazanıcı olamaz.
        var poorQualityBooks = books
            .Where(b =>
                b.Description.Length < 50 ||
                !IsGoodThumbnail(b.ThumbnailUrl))
            .ToList();

        if (poorQualityBooks.Count > 0)
        {
            Console.WriteLine($"[Purge] {poorQualityBooks.Count} düşük kaliteli kayıt siliniyor...");
            _context.Books.RemoveRange(poorQualityBooks);
            await _context.SaveChangesAsync(cancellationToken);

            foreach (var poor in poorQualityBooks)
            {
                string reason;
                if (poor.Description.Length < 50)
                    reason = $"Kısa açıklama ({poor.Description.Length} karakter)";
                else
                    reason = GetThumbnailRejectReason(poor.ThumbnailUrl);

                Console.WriteLine($"[Purge] ✗ Silindi: \"{poor.Title}\" (Sebep: {reason})");
            }

            Console.WriteLine($"[Purge] Toplam {poorQualityBooks.Count} kayıt silindi.");
            books = await _context.Books.ToListAsync(cancellationToken);
        }
        else
        {
            Console.WriteLine("[Purge] Tüm kayıtlar kalite kriterlerini karşılıyor.");
        }

        // ── Aşama 1: Fuzzy Deduplication ─────────────────────────────────────
        var booksToDelete = BuildFuzzyDuplicateDeleteList(books);

        if (booksToDelete.Count > 0)
        {
            Console.WriteLine($"[Dedup] {booksToDelete.Count} duplikat kayıt siliniyor...");
            _context.Books.RemoveRange(booksToDelete);
            await _context.SaveChangesAsync(cancellationToken);

            foreach (var deleted in booksToDelete)
                Console.WriteLine($"[Dedup] ✗ Silindi: \"{deleted.Title}\" (ID:{deleted.BookId}, Yazar:{deleted.Author})");

            Console.WriteLine($"[Dedup] Toplam {booksToDelete.Count} kayıt silindi. Kalan: {books.Count - booksToDelete.Count}");
            books = await _context.Books.ToListAsync(cancellationToken);
        }
        else
        {
            Console.WriteLine("[Dedup] Duplikat bulunamadı.");
        }

        // ── Aşama 2: Genre Normalizasyonu ─────────────────────────────────────
        var updatedCount = 0;

        foreach (var book in books)
        {
            var newGenre = DetermineUpdatedGenre(book);
            if (string.Equals(book.Genre, newGenre, StringComparison.Ordinal)) continue;

            Console.WriteLine($"[Genre] \"{book.Title}\" {book.Genre} ⟹ {newGenre}");
            book.Genre = newGenre;
            updatedCount++;
        }

        if (updatedCount > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
            Console.WriteLine($"[Genre] {updatedCount} kitabın türü güncellendi.");
        }
        else
        {
            Console.WriteLine("[Genre] Tüm türler güncel, değişiklik yok.");
        }

        var totalDeleted = poorQualityBooks.Count + booksToDelete.Count;
        Console.WriteLine($"[Cleanup] ══ Tamamlandı ══");
        Console.WriteLine($"[Cleanup]    Silinen  : {totalDeleted} kayıt");
        Console.WriteLine($"[Cleanup]    Güncellenen: {updatedCount} tür");
        Console.WriteLine($"[Cleanup]    Kalan    : {books.Count} kitap");
        Console.WriteLine($"[Cleanup] ═══════════════");
    }

    // ── Fuzzy Deduplication Çekirdeği ────────────────────────────────────────

    /// <summary>
    /// Fuzzy başlık eşleştirmesiyle duplikat grupları oluşturur ve her gruptan
    /// "en güçlü" kaydı (Survival of the Fittest) koruyarak silinecek listesi döner.
    /// <para>
    /// Eşleştirme mantığı: normalize(A).Contains(normalize(B)) || normalize(B).Contains(normalize(A))
    /// Bu kural "1984" ve "1984 Large Print Edition" gibi varyantları aynı gruba alır.
    /// </para>
    /// </summary>
    private static List<Book> BuildFuzzyDuplicateDeleteList(List<Book> allBooks)
    {
        // Union-Find benzeri yaklaşım: her kitabı bir gruba at
        // Küçük veri setleri (≤500 kitap) için O(n²) yeterince hızlı
        var groupId = new Dictionary<int, int>(); // bookId → groupLeaderId
        var processed = new HashSet<int>();

        foreach (var book in allBooks)
        {
            if (processed.Contains(book.BookId)) continue;

            // Bu kitabı kendi grubunun lideri yap
            groupId[book.BookId] = book.BookId;
            processed.Add(book.BookId);

            var normA = NormalizeTitle(book.Title);

            foreach (var other in allBooks)
            {
                if (other.BookId == book.BookId) continue;

                var normB = NormalizeTitle(other.Title);

                // Fuzzy eşleşme: biri diğerinin içinde mi?
                bool fuzzyMatch =
                    normA == normB ||                       // tam eşleşme
                    normA.Contains(normB, StringComparison.Ordinal) ||
                    normB.Contains(normA, StringComparison.Ordinal);

                if (fuzzyMatch)
                {
                    // other'ı bu gruba bağla
                    if (!groupId.ContainsKey(other.BookId))
                        groupId[other.BookId] = book.BookId;

                    processed.Add(other.BookId);
                }
            }
        }

        // groupLeaderId → kitap listesi
        var groups = allBooks
            .GroupBy(b => groupId.TryGetValue(b.BookId, out var gid) ? gid : b.BookId)
            .Where(g => g.Count() > 1)
            .ToList();

        var toDelete = new List<Book>();

        foreach (var group in groups)
        {
            var winner = SelectWinner(group.ToList());
            var losers = group.Where(b => b.BookId != winner.BookId);

            Console.WriteLine($"[Dedup] Grup ({group.Count()} kayıt) → Kazanıcı: \"{winner.Title}\" (ID:{winner.BookId})");
            toDelete.AddRange(losers);
        }

        return toDelete;
    }

    /// <summary>
    /// Grup içinden en güçlü kaydı seçer — Survival of the Fittest:
    /// <list type="number">
    ///   <item>Kaliteli görseli olan kitaplar aralarında en uzun açıklamalı olanı kazanır.</item>
    ///   <item>Hepsinin görseli kötüyse en uzun açıklamalı olanı kazanır.</item>
    /// </list>
    /// </summary>
    private static Book SelectWinner(List<Book> candidates)
    {
        var withGoodCover = candidates
            .Where(b => IsGoodThumbnail(b.ThumbnailUrl))
            .OrderByDescending(b => b.Description?.Length ?? 0)
            .ToList();

        return withGoodCover.Count > 0
            ? withGoodCover[0]
            : candidates.OrderByDescending(b => b.Description?.Length ?? 0).First();
    }

    /// <summary>
    /// Thumbnail URL'inin gerçek/kaliteli bir kapak görseline işaret edip etmediğini kontrol eder.
    /// <list type="bullet">
    ///   <item>http:// — güvensiz / genellikle eski-kırık bağlantılar</item>
    ///   <item>placehold.co, ISBN_MISSING, no_image — gerçek kapak yok sinyalleri</item>
    ///   <item>&amp;zoom=0 — Google düşük çözünürlükleri</item>
    ///   <item>&amp;edge=curl — Google'ın minyatür kapak işareti</item>
    ///   <item>books.google.com/books?id= — görsel değil, kitap sayfası URL'i</item>
    ///   <item>frontcover/wNNN olmayan — sahte frontcover yolu</item>
    ///   <item>fife/static.googleusercontent.com, imnotabook — bilinen kötü domain'ler</item>
    /// </list>
    /// </summary>
    private static bool IsGoodThumbnail(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;

        // 1. HTTP yasağı — güvensiz + genellikle eski/kırık
        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)) return false;

        // 2. Placeholder / eksik görsel sinyalleri
        if (url.Contains("placehold.co",  StringComparison.OrdinalIgnoreCase)) return false;
        if (url.StartsWith("ISBN_MISSING", StringComparison.OrdinalIgnoreCase)) return false;

        // 3. İçerik 'görsel yok' anahtar kelimeleri
        if (url.Contains("no_image",      StringComparison.OrdinalIgnoreCase)) return false;
        if (url.Contains("no_cover",      StringComparison.OrdinalIgnoreCase)) return false;
        if (url.Contains("missing_cover", StringComparison.OrdinalIgnoreCase)) return false;

        // 4. Google düşük çözünürlük sinyalleri
        if (url.Contains("&zoom=0",    StringComparison.OrdinalIgnoreCase)) return false;
        if (url.Contains("&edge=curl", StringComparison.OrdinalIgnoreCase)) return false;

        // 5. books.google.com/books?id= — bu bir sayfa URL'i, görsel URL'i değil
        if (url.Contains("books.google.com/books?id=", StringComparison.OrdinalIgnoreCase)) return false;

        // 6. Sahte frontcover: /books/content/images/frontcover/ içeren ama
        //    gerçek boyut parametresi (w=NNN veya h=NNN) taşımayan URL'ler
        if (url.Contains("books/content/images/frontcover/", StringComparison.OrdinalIgnoreCase) &&
            !url.Contains("w=", StringComparison.OrdinalIgnoreCase) &&
            !url.Contains("h=", StringComparison.OrdinalIgnoreCase))
            return false;

        // 7. Google'ın bilinen kötü thumbnail domain'leri
        if (url.Contains("static.googleusercontent.com", StringComparison.OrdinalIgnoreCase)) return false;
        if (url.Contains("fife.googleusercontent.com",   StringComparison.OrdinalIgnoreCase)) return false;
        if (url.Contains("imnotabook",                    StringComparison.OrdinalIgnoreCase)) return false;

        return true;
    }

    /// <summary>
    /// IsGoodThumbnail'in reddettiği URL için insan okunabilir Türkçe neden döner.
    /// Purge logu için kullanılır.
    /// </summary>
    private static string GetThumbnailRejectReason(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))                                                        return "Görsel URL boş";
        if (url.StartsWith("http://",                         StringComparison.OrdinalIgnoreCase)) return "Güvensiz HTTP bağlantısı";
        if (url.Contains("placehold.co",                      StringComparison.OrdinalIgnoreCase)) return "Placeholder görsel (placehold.co)";
        if (url.StartsWith("ISBN_MISSING",                    StringComparison.OrdinalIgnoreCase)) return "ISBN eksik, görsel yok";
        if (url.Contains("no_image",                          StringComparison.OrdinalIgnoreCase)) return "'no_image' sinyali";
        if (url.Contains("no_cover",                          StringComparison.OrdinalIgnoreCase)) return "'no_cover' sinyali";
        if (url.Contains("missing_cover",                     StringComparison.OrdinalIgnoreCase)) return "'missing_cover' sinyali";
        if (url.Contains("&zoom=0",                           StringComparison.OrdinalIgnoreCase)) return "Düşük çözünürlük (zoom=0)";
        if (url.Contains("&edge=curl",                        StringComparison.OrdinalIgnoreCase)) return "Minyatür kapak (edge=curl)";
        if (url.Contains("books.google.com/books?id=",        StringComparison.OrdinalIgnoreCase)) return "Kitap sayfası URL'i (görsel değil)";
        if (url.Contains("books/content/images/frontcover/",  StringComparison.OrdinalIgnoreCase)) return "Sahte frontcover (boyut parametresi yok)";
        if (url.Contains("static.googleusercontent.com",      StringComparison.OrdinalIgnoreCase)) return "Google placeholder domain";
        if (url.Contains("fife.googleusercontent.com",        StringComparison.OrdinalIgnoreCase)) return "Google thumbnail domain (fife)";
        if (url.Contains("imnotabook",                        StringComparison.OrdinalIgnoreCase)) return "'imnotabook' sinyali";
        return "Bilinmeyen görsel sorunu";
    }


    /// <summary>
    /// Başlığı fuzzy karşılaştırma için normalize eder:
    /// küçük harf, boşluklar sıkıştırılır, noktalama kaldırılır.
    /// </summary>
    private static string NormalizeTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title)) return string.Empty;

        return new string(
                title
                    .ToLowerInvariant()
                    .Where(c => char.IsLetterOrDigit(c) || c == ' ')
                    .ToArray())
            .Replace("  ", " ")
            .Trim();
    }

    /// <summary>
    /// Mevcut bir kitap için yeni genre değerini belirler. Pipeline:
    /// <list type="number">
    ///   <item>Description'da Romance sinyali → "Romance"</item>
    ///   <item>Mevcut genre'de Romance sinyali → "Romance"</item>
    ///   <item>Merge kuralı eşleşmesi (Thriller, Crime vb.)</item>
    ///   <item>Güncel AuthorGenreMap'ten yazar eşleşmesi</item>
    ///   <item>Mevcut genre blacklist'te değilse koru, yoksa "General"</item>
    /// </list>
    /// </summary>
    private string DetermineUpdatedGenre(Book book)
    {
        // 0. Yazar koruma tablosu — bu yazarlar için kesin tür döner, Romance override engellidir
        var protectedGenre = GenreNormalizer.ResolveProtectedGenre(book.Author);
        if (protectedGenre is not null) return protectedGenre;

        // 1. Description'da romance sinyali
        if (GenreNormalizer.HasRomanceSignal(book.Description))
            return "Romance";

        // 2. Mevcut genre'de romance sinyali
        if (GenreNormalizer.HasRomanceSignal(book.Genre))
            return "Romance";

        // 3. Merge kuralları — Crime/Thriller/Detective vb.
        var merged = GenreNormalizer.TryMerge(book.Genre);
        if (merged is not null) return merged;

        // 4. AuthorGenreMap'ten güncel eşleşme (Jane Austen artık Romance, Dickens Classics&Philosophy)
        var fromAuthor = ResolveAuthorGenre(book.Author);
        if (fromAuthor != "General") return fromAuthor;

        // 5. Mevcut genre geçerliyse koru, blacklist'teyse General'e düş
        return GenreNormalizer.IsBlacklisted(book.Genre) ? "General" : book.Genre;
    }

    /// <summary>
    /// Kitabın yazarını AuthorGenreMap ile karşılaştırır.
    /// Yazar birden fazla isim içerebileceğinden Contains kontrolü kullanılır.
    /// </summary>
    private static string ResolveAuthorGenre(string author)
    {
        foreach (var (key, genre) in AuthorGenreMap)
        {
            if (author.Contains(key, StringComparison.OrdinalIgnoreCase))
                return genre;
        }
        return "General";
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