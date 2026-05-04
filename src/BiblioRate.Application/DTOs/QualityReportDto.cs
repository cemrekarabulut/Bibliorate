using System.Collections.Generic;

namespace BiblioRate.Application.DTOs;

public class QualityReportDto
{
    public int TotalBooks { get; set; }
    public int PerfectBooks { get; set; }
    public double AverageQuality { get; set; }
    public List<LowQualityBookDto> LowQualityBooks { get; set; } = [];
}

public class LowQualityBookDto
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int QualityScore { get; set; }
}
