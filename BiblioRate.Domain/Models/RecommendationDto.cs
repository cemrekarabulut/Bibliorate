using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioRate.Domain.Models
{
    public class RecommendationDto
    {
    public string Title { get; set; } = string.Empty;
    public double Rating { get; set; }
    public int Votes { get; set; }   
    }
}