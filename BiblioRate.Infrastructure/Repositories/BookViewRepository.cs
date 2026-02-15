using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using BiblioRate.Infrastructure.Context;

namespace BiblioRate.Infrastructure.Repositories
{
 public class BookViewRepository : IBookViewRepository
    {
        private readonly ApplicationDbContext _context;

        public BookViewRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddViewAsync(BookView bookView)
        {
            await _context.BookViews.AddAsync(bookView);
            await _context.SaveChangesAsync();
        }
    }   
}