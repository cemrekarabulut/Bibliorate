using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiblioRate.Application.Interfaces;

namespace BiblioRate.Infrastructure.Services
{
 // Infrastructure/Services/BookSimilarityScorer.cs
public class BookSimilarityScorer : IBookSimilarityScorer
{
    private const double Threshold = 0.75;

    public double Score(string title1, string author1, string title2, string author2)
    {
        var titleScore = LevenshteinSimilarity(Normalize(title1), Normalize(title2));
        var authorScore = LevenshteinSimilarity(AuthorFingerprint(author1), AuthorFingerprint(author2));
        return (titleScore * 0.7) + (authorScore * 0.3);
    }

    public bool IsDuplicate(string title1, string author1, string title2, string author2)
        => Score(title1, author1, title2, author2) >= Threshold;

    private static string Normalize(string input) =>
        new string(input.ToLowerInvariant()
            .Where(c => char.IsLetterOrDigit(c) || c == ' ')
            .ToArray()).Trim();

    private static string AuthorFingerprint(string author)
    {
        if (string.IsNullOrWhiteSpace(author)) return string.Empty;
        var parts = author.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return string.Empty;
        return parts.Length > 1 ? $"{parts[0][0]}{parts[^1]}" : parts[0];
    }

    private static double LevenshteinSimilarity(string a, string b)
    {
        if (a == b) return 1.0;                            // identical (including both empty)
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0.0;
        var distance = LevenshteinDistance(a, b);
        return 1.0 - (double)distance / Math.Max(a.Length, b.Length);
    }

    private static int LevenshteinDistance(string a, string b)
    {
        var dp = new int[a.Length + 1, b.Length + 1];
        for (int i = 0; i <= a.Length; i++) dp[i, 0] = i;
        for (int j = 0; j <= b.Length; j++) dp[0, j] = j;

        for (int i = 1; i <= a.Length; i++)
            for (int j = 1; j <= b.Length; j++)
                dp[i, j] = a[i-1] == b[j-1]
                    ? dp[i-1, j-1]
                    : 1 + Math.Min(dp[i-1, j-1], Math.Min(dp[i-1, j], dp[i, j-1]));

        return dp[a.Length, b.Length];
    }
}   
}