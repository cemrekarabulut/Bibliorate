using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiblioRate.Domain.Entities;         // Book nesnesi için
using BiblioRate.Application.Interfaces;

namespace BiblioRate.Application.Interfaces
{
    public interface IBookQualityEvaluator
    {
     /// <summary>
    /// Bir kitabın veritabanı kalitesini 0-100 arası bir skorla hesaplar.
    /// </summary>
    int Evaluate(Book book);   
    }
}