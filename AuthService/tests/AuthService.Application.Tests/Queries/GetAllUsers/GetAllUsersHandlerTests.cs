using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Application.Queries.GetAllUsers;
using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AuthService.Application.Tests.Queries.GetAllUsers;

public class GetAllUsersHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();

    [Fact]
    public async Task Handle_WithUsersFromRepository_ShouldReturnMappedUserDtos()
    {
        // Arrange
        var firstUser = CreateUser("alice@example.com", "SecurePassword123");
        var secondUser = CreateUser("bob@example.com", "SecurePassword456");
        var query = new GetAllUsersQuery();
        var cancellationToken = CancellationToken.None;
        var users = new List<User> { firstUser, secondUser };

        _userRepositoryMock
            .Setup(repository => repository.GetAllAsync(cancellationToken))
            .ReturnsAsync(users);

        var handler = CreateHandler();

        // Act
        var result = (await handler.Handle(query, cancellationToken)).ToList();

        // Assert
        result.Should().HaveCount(2);

        result[0].Should().BeEquivalentTo(new UserDTO
        {
            UserId = firstUser.Id,
            Email = firstUser.Email.Value,
            Role = firstUser.Role.ToString()
        }, options => options.Excluding(dto => dto.IsActive)
            .Excluding(dto => dto.CreatedAt)
            .Excluding(dto => dto.UpdatedAt));

        result[1].Should().BeEquivalentTo(new UserDTO
        {
            UserId = secondUser.Id,
            Email = secondUser.Email.Value,
            Role = secondUser.Role.ToString()
        }, options => options.Excluding(dto => dto.IsActive)
            .Excluding(dto => dto.CreatedAt)
            .Excluding(dto => dto.UpdatedAt));
    }

    [Fact]
    public async Task Handle_WithEmptyRepositoryResult_ShouldReturnEmptyCollection()
    {
        // Arrange
        var query = new GetAllUsersQuery();
        var cancellationToken = CancellationToken.None;

        _userRepositoryMock
            .Setup(repository => repository.GetAllAsync(cancellationToken))
            .ReturnsAsync(Enumerable.Empty<User>());

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var query = new GetAllUsersQuery();
        var cancellationToken = CancellationToken.None;

        _userRepositoryMock
            .Setup(repository => repository.GetAllAsync(cancellationToken))
            .ThrowsAsync(new InvalidOperationException("Repository failure"));

        var handler = CreateHandler();

        // Act
        Func<Task> act = async () => _ = await handler.Handle(query, cancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Repository failure");
    }

    private GetAllUsersHandler CreateHandler()
    {
        return new GetAllUsersHandler(
            _userRepositoryMock.Object,
            NullLogger<GetAllUsersHandler>.Instance);
    }

    private static User CreateUser(string email, string password)
    {
        return new User(
            new EmailVO(email),
            new PasswordVO(password));
    }
}
