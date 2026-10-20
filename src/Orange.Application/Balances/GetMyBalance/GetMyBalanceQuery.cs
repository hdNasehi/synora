using Orange.Domain.Balances;

namespace Orange.Application.Balances.GetMyBalance;

public sealed record GetMyBalanceQuery(int Year);

public sealed record GetMyBalanceResponse(LeaveBalance Balance);
