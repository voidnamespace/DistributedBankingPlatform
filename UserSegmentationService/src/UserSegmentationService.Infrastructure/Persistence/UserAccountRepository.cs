using Microsoft.EntityFrameworkCore;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Domain.Entities;
using UserSegmentationService.Infrastructure.Persistence.Database;

namespace UserSegmentationService.Infrastructure.Persistence;

internal class UserAccountRepository : IUserAccountRepository
{
    private readonly SegmentationDbContext _dbContext;

    public UserAccountRepository(SegmentationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<UserAccount?> GetByAccountNumberAsync(
        string accountNumber,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.UserAccounts
            .FirstOrDefaultAsync(x => x.AccountNumber == accountNumber, cancellationToken);
    }

    public void Add(UserAccount userAccount)
    {
        _dbContext.UserAccounts.Add(userAccount);
    }
}
