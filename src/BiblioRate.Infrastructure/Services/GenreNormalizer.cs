namespace BiblioRate.Infrastructure.Services;

/// <summary>
/// Merkezi tür (genre) normalizasyon motoru.
/// GoogleBooksService (yeni kitap eşleştirme) ve DataSeederService (mevcut kitap güncelleme)
/// tarafından paylaşılır; her iki kaynakta da aynı kurallar uygulanır.
/// </summary>
internal static class GenreNormalizer
{
    // ── Blacklist ─────────────────────────────────────────────────────────────
    /// <summary>
    /// Anlamsız, lokasyon bazlı veya aşırı jenerik etiketler.
    /// Bu değerler tür olarak saklanmaz; yerine fallback kullanılır.
    /// </summary>
    internal static readonly HashSet<string> Blacklist = new(StringComparer.OrdinalIgnoreCase)
    {
        // Jenerik / anlamsız
        "Fiction", "General", "General Fiction", "Literary Fiction",
        "English Fiction", "American Fiction", "Literary Collections",
        "Juvenile Fiction", "Chess", "Nonfiction",

        // Lokasyon/ülke etiketleri
        "England", "Austria", "Germany", "France", "Europe",

        // Akademik / meta etiketler
        "History", "European History", "Social Science",
    };

    // ── Romance sinyalleri ────────────────────────────────────────────────────
    /// <summary>Genre veya description'da bu kelimeler varsa kitap "Romance" olarak sınıflandırılır.</summary>
    private static readonly string[] RomanceSignals =
    [
        "Romance", "Love", "Romantic", "Relationship", "Jane Austen"
    ];

    // ── Yazar koruma tablosu ──────────────────────────────────────────────────
    /// <summary>
    /// Bu yazarların kitaplarında description'daki "Love", "Romantic" vb. kelimelere bakılmaksızın
    /// AuthorGenreMap'teki sabit tür uygulanır.
    /// Örnek: Dostoyevsky'nin açıklamasında "love" geçse de kitap "Classics &amp; Philosophy" kalır.
    /// </summary>
    internal static readonly Dictionary<string, string> AuthorProtectedGenres =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // Classics & Philosophy — description'daki romance sinyaline rağmen korunur
            ["Stefan Zweig"]       = "Classics & Philosophy",
            ["Dostoyevsky"]        = "Classics & Philosophy",
            ["Charles Dickens"]    = "Classics & Philosophy",
            ["Irvin D. Yalom"]     = "Classics & Philosophy",

            // Drama — korunur
            ["John Steinbeck"]     = "Drama",
            ["José Saramago"]      = "Drama",
            ["Matt Haig"]          = "Drama",

            // Dystopian — korunur
            ["Suzanne Collins"]    = "Dystopian",
            ["George Orwell"]      = "Dystopian",

            // Mystery & Thriller — korunur (Sherlock Holmes asla Romance olamaz)
            ["Gillian Flynn"]      = "Mystery & Thriller",
            ["Agatha Christie"]    = "Mystery & Thriller",
            ["Dan Brown"]          = "Mystery & Thriller",
            ["Arthur Conan Doyle"] = "Mystery & Thriller",   // Sherlock Holmes Fix
        };

    // ── Birleştirme kuralları (sıralı — ilk eşleşen kazanır) ─────────────────
    /// <summary>
    /// Benzer türleri tek başlık altında toplar.
    /// Triggers içindeki herhangi bir kelime genre'de geçiyorsa Output döner.
    /// </summary>
    private static readonly (string[] Triggers, string Output)[] MergeRules =
    [
        // Mystery & Thriller: dedektif, suç, gerilim alt türleri birleştirilir
        (
            ["Detective", "Mystery", "Crime", "Thriller", "Suspense", "Noir"],
            "Mystery & Thriller"
        ),
        // Classics & Philosophy: felsefe ve edebi eleştiri aynı çatı altında
        (
            ["Philosophy", "Literary Criticism", "Philosophical"],
            "Classics & Philosophy"
        ),
    ];

    // ── Genel API ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Ham bir tür stringini normalize eder. Pipeline sırası:
    /// <list type="number">
    ///   <item>Yazar koruma tablosu — korunan yazar varsa kesin döner (Romance override engellenmiş)</item>
    ///   <item>Romance sinyal tespiti (genre + description)</item>
    ///   <item>Birleştirme (merge) kuralları</item>
    ///   <item>Blacklist → fallback</item>
    ///   <item>Geçerliyse raw genre döner</item>
    /// </list>
    /// </summary>
    /// <param name="rawGenre">Ham tür değeri (Google kategorisi veya DB kaydı).</param>
    /// <param name="description">Kitap açıklaması — Romance tespiti için taranır.</param>
    /// <param name="fallback">Blacklist'e düşünce veya boşsa kullanılacak yedek tür.</param>
    /// <param name="author">Yazar adı — koruma tablosu için kontrol edilir.</param>
    internal static string Normalize(
        string?  rawGenre,
        string?  description = null,
        string   fallback    = "General",
        string?  author      = null)
    {
        // 0. Yazar koruma: bu yazarlar için sabit tür döner, Romance override çalışmaz
        if (!string.IsNullOrWhiteSpace(author))
        {
            var protectedGenre = ResolveProtectedGenre(author);
            if (protectedGenre is not null) return protectedGenre;
        }

        // 1. Romance tespiti — genre veya description'da sinyal varsa kesin döner
        if (HasRomanceSignal(rawGenre) ||
            (!string.IsNullOrWhiteSpace(description) && HasRomanceSignal(description)))
            return "Romance";

        // 2. Birleştirme kuralları
        var merged = TryMerge(rawGenre);
        if (merged is not null) return merged;

        // 3. Blacklist veya boş → fallback
        if (string.IsNullOrWhiteSpace(rawGenre) || Blacklist.Contains(rawGenre!))
            return string.IsNullOrWhiteSpace(fallback) ? "General" : fallback;

        return rawGenre!;
    }

    /// <summary>
    /// Google Books'tan gelen kategori listesinden doğru türü seçer.
    /// Önce yazar koruma tablosunu kontrol eder; ardından description + kategorileri dener;
    /// hiçbiri geçerli değilse fallback döner.
    /// </summary>
    /// <param name="author">Yazar adı — koruma tablosu için kontrol edilir.</param>
    internal static string ResolveFromCategories(
        IReadOnlyList<string>? categories,
        string?                description,
        string                 fallback,
        string?                author = null)
    {
        // 0. Yazar koruma: korunan yazar ise kategorilere/description'a bakmadan kesin döner
        if (!string.IsNullOrWhiteSpace(author))
        {
            var protectedGenre = ResolveProtectedGenre(author);
            if (protectedGenre is not null) return protectedGenre;
        }

        // 1. Description'da açık romance sinyali varsa kategorilere bakmadan dön
        if (!string.IsNullOrWhiteSpace(description) && HasRomanceSignal(description))
            return "Romance";

        if (categories is { Count: > 0 })
        {
            foreach (var cat in categories)
            {
                // Her kategoriyi normalize et; boş dönenler (blacklisted) atlanır
                var normalized = Normalize(cat.Trim(), description, fallback: string.Empty, author);
                if (!string.IsNullOrEmpty(normalized))
                    return normalized;
            }
        }

        return fallback;
    }

    /// <summary>Birleştirme kuralını uygular. Eşleşme yoksa <c>null</c> döner.</summary>
    internal static string? TryMerge(string? rawGenre)
    {
        if (string.IsNullOrWhiteSpace(rawGenre)) return null;

        foreach (var (triggers, output) in MergeRules)
        {
            if (triggers.Any(t => rawGenre.Contains(t, StringComparison.OrdinalIgnoreCase)))
                return output;
        }
        return null;
    }

    /// <summary>Verilen metinde Romance sinyali olup olmadığını kontrol eder.</summary>
    internal static bool HasRomanceSignal(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        return RomanceSignals.Any(s => text.Contains(s, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Tür kara listede mi veya boş mu?</summary>
    internal static bool IsBlacklisted(string? genre)
        => string.IsNullOrWhiteSpace(genre) || Blacklist.Contains(genre!);

    /// <summary>
    /// Yazarın koruma tablosundaki sabit türünü döner.
    /// Yazar birden fazla isim içerebilir (Contains kontrolü).
    /// Eşleşme yoksa <c>null</c> döner.
    /// </summary>
    internal static string? ResolveProtectedGenre(string? author)
    {
        if (string.IsNullOrWhiteSpace(author)) return null;

        foreach (var (key, genre) in AuthorProtectedGenres)
        {
            if (author.Contains(key, StringComparison.OrdinalIgnoreCase))
                return genre;
        }
        return null;
    }
}
