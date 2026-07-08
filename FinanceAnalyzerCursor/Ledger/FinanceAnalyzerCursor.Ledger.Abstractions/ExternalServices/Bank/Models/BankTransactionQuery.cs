namespace FinanceAnalyzerCursor.Ledger.Abstractions.ExternalServices.Bank.Models;

public sealed record BankTransactionQuery(
    DateOnly From,
    DateOnly To,
    int Offset,
    int PageSize,
    string AuthToken);
