using UserSegmentationService.Domain.Entities;

namespace UserSegmentationService.Application.Interfaces;

public interface IUserMetricRepository
{
    Task<UserMetric?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetActiveUserIdsAsync(
        DateTime activeSince,
        CancellationToken cancellationToken = default);

    void Add(UserMetric userMetric);
}
