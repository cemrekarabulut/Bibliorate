using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiblioRate.Domain.Entities;

namespace BiblioRate.Application.Interfaces
{
    public interface IBookViewRepository
    {
    Task AddViewAsync(BookView bookView);   
    }
}