namespace FinanceAnalyzerCursor.Ledger.Abstractions.External;

public sealed record BankTransactionQuery(
    DateOnly From,
    DateOnly To,
    int Offset,
    int PageSize);
