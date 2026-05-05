using BiblioRate.Application.DTOs;
using FluentAssertions;

namespace BiblioRate.Tests.DTOs;

public class DtoTests
{
    [Fact]
    public void QualityReportDto_ShouldStoreValuesCorrectly()
    {
        // Arrange & Act
        var dto = new QualityReportDto
        {
            TotalBooks = 100,
            PerfectBooks = 10,
            AverageQuality = 85.5,
            LowQualityBooks = new List<LowQualityBookDto>
            {
                new LowQualityBookDto { BookId = 1, Title = "Bad Book", QualityScore = 20 }
            }
        };

        // Assert
        dto.TotalBooks.Should().Be(100);
        dto.PerfectBooks.Should().Be(10);
        dto.AverageQuality.Should().Be(85.5);
        dto.LowQualityBooks.Should().HaveCount(1);
        dto.LowQualityBooks[0].BookId.Should().Be(1);
        dto.LowQualityBooks[0].Title.Should().Be("Bad Book");
        dto.LowQualityBooks[0].QualityScore.Should().Be(20);
    }

    [Fact]
    public void LowQualityBookDto_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var dto = new LowQualityBookDto();

        // Assert
        dto.Title.Should().Be(string.Empty);
        dto.QualityScore.Should().Be(0);
        dto.BookId.Should().Be(0);
    }
}
