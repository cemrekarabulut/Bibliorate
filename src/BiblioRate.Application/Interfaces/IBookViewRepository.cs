using BiblioRate.Domain.Entities;

namespace BiblioRate.Application.Interfaces;

public interface IBookViewRepository
{
    Task AddViewAsync(BookView bookView);
}
