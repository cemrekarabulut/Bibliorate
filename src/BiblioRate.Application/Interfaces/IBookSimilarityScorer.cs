using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioRate.Application.Interfaces
{
   // Application/Interfaces/IBookSimilarityScorer.cs
public interface IBookSimilarityScorer
{
    double Score(string title1, string author1, string title2, string author2);
    bool IsDuplicate(string title1, string author1, string title2, string author2);
} 
}