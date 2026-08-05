using MediatR;

namespace TransactionService.API.Contracts;

public sealed record CreateRegularPaymentRequest(
    string FromAccountId,
    string ToAccountId,
    decimal Amount,
    int DayOfMonth
    ) : IRequest;
