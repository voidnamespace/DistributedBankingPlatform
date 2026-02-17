using BankCardService.Domain.Entities;
using BankCardService.Application.Interfaces;
using BankCardService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankCardService.Infrastructure.Repositories;

public class BankCardRepository : IBankCardRepository
{
    private readonly BankCardDbContext _context;

    public BankCardRepository (BankCardDbContext context)
    {
        _context = context;
    }

    public async Task<BankCard?> GetByIdAsync(Guid id)
    => await _context.BankCards.FindAsync(id);

    public async Task<IEnumerable<BankCard>> GetAllAsync()
        => await _context.BankCards.ToListAsync();

    public async Task AddAsync(BankCard card)
        => await _context.BankCards.AddAsync(card);

    public Task RemoveAsync(BankCard card)
    {
        _context.BankCards.Remove(card);
        return Task.CompletedTask;
    }
    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}
