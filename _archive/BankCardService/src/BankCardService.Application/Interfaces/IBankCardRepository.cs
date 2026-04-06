using BankCardService.Domain.Entities;
namespace BankCardService.Application.Interfaces;
public interface IBankCardRepository
{
    Task<BankCard?> GetByIdAsync (Guid id);
    Task<IEnumerable<BankCard>> GetAllAsync();
    Task AddAsync(BankCard card);
    Task RemoveAsync(BankCard card);
    Task SaveAsync();
}
