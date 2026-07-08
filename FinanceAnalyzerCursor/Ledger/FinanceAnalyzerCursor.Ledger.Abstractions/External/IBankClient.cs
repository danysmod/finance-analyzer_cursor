using FinanceAnalyzerCursor.Ledger.Domain.Entities;

namespace FinanceAnalyzerCursor.Ledger.Abstractions.External;

public interface IBankClient
{
    string BankId { get; }

    Task<IReadOnlyList<BankTransaction>> GetTransactionsAsync(
        BankTransactionQuery query,
        CancellationToken cancellationToken = default);
}
