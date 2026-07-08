using FinanceAnalyzerCursor.Ledger.Domain.Entities;

namespace FinanceAnalyzerCursor.Ledger.Application.Banking;

public interface IBankDataProvider
{
    Task<IReadOnlyList<BankTransaction>> GetTransactionsAsync(
        DateOnly from,
        DateOnly to,
        string authToken,
        CancellationToken ct = default);
}
