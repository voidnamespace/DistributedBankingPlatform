using MediatR;

namespace UserSegmentationService.Application.Commands.Accounts.RequestBackfill;

public sealed record RequestUserAccountsBackfillCommand() : IRequest;

