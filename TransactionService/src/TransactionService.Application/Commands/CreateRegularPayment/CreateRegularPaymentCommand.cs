using MediatR;

namespace TransactionService.Application.Commands.CreateRegularPayment;

public sealed record CreateRegularPaymentRequest(
    string FromAccountId,
    string ToAccountId,
    decimal Amount,
    int DayOfMonth
    ) : IRequest;
