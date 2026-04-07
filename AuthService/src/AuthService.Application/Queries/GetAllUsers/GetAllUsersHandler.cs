using MediatR;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Queries.GetAllUsers;

public class GetAllUsersHandler
    : IRequestHandler<GetAllUsersQuery, IEnumerable<UserDTO>>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetAllUsersHandler> _logger;

    public GetAllUsersHandler(
        IUserRepository userRepository,
        ILogger<GetAllUsersHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<UserDTO>> Handle(
        GetAllUsersQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetAllUsersQuery received");

        var users = await _userRepository.GetAllAsync(cancellationToken);

        _logger.LogInformation("Users retrieved from repository. Count: {Count}", users.Count());

        return users.Select(u => new UserDTO
        {
            UserId = u.Id,
            Email = u.Email.Value,
            Role = u.Role.ToString(),
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt
        });
    }
}
