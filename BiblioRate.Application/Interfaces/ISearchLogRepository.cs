using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiblioRate.Domain.Entities;

namespace BiblioRate.Application.Interfaces
{
    public interface ISearchLogRepository
    {
    Task AddLogAsync(SearchLog log);
    Task<IEnumerable<SearchLog>> GetLastLogsAsync(int count);    
    }
}