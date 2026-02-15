using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioRate.Domain.Models
{
    public class BookAnalyticsDto
    {
    public string Title { get; set; }
    public double Rating { get; set; }
    public int Votes { get; set; }    
    }
}