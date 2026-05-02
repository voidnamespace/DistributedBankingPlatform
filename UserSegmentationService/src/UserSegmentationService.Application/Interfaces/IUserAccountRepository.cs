using UserSegmentationService.Domain.Entities;

namespace UserSegmentationService.Application.Interfaces;

public interface IUserAccountRepository
{
    Task<UserAccount?> GetByAccountNumberAsync(
        string accountNumber,
        CancellationToken cancellationToken = default);

    void Add(UserAccount userAccount);
}
