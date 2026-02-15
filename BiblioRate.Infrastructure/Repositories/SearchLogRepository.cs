using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiblioRate.Application.Interfaces;
using BiblioRate.Domain.Entities;
using BiblioRate.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BiblioRate.Infrastructure.Repositories;

public class SearchLogRepository : ISearchLogRepository
{
    private readonly ApplicationDbContext _context;

    public SearchLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddLogAsync(SearchLog log)
    {
        await _context.SearchLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<SearchLog>> GetLastLogsAsync(int count)
    {
        return await _context.SearchLogs
            .OrderByDescending(l => l.SearchedAt)
            .Take(count)
            .ToListAsync();
    }
}