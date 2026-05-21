using UserSegmentationService.Domain.Entities;

namespace UserSegmentationService.Application.Interfaces;

public interface IUserMetricRepository
{
    Task<UserMetric?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken );

    Task<bool> ExistsAsync(
        Guid userId,
        CancellationToken cancellationToken );

    Task<IReadOnlyList<Guid>> GetActiveUserIdsAsync(
        DateTime activeSince,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Guid>> GetVipUserIdsAsync(
        decimal minimumSpend,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Guid>> GetRiskUserIdsAsync(
        DateTime inactiveSince,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<UserMetric>> GetRandomAsync(
        int count,
        CancellationToken cancellationToken);

    void Add(UserMetric userMetric);
}
