namespace FinanceAnalyzerCursor.Ledger.Application.Banking;

public sealed record BankTransactionQuery(
    DateOnly From,
    DateOnly To,
    int Offset,
    int PageSize);
