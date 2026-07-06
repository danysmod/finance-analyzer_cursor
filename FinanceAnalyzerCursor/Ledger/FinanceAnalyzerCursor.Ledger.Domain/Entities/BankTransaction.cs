namespace FinanceAnalyzerCursor.Ledger.Domain.Entities;

public sealed record BankTransaction(
    string ExternalId,
    DateTimeOffset Date,
    decimal Amount,
    string Currency,
    string Description,
    string? Category = null);
