using FinanceAnalyzerCursor.Ledger.Abstractions.ExternalServices.Bank.Models;
using FinanceAnalyzerCursor.Ledger.Domain.Entities;

namespace FinanceAnalyzerCursor.Ledger.Abstractions.ExternalServices.Bank;

public interface IBankClient
{
    Task<IReadOnlyList<BankTransaction>> GetTransactionsAsync(
        BankTransactionQuery query,
        CancellationToken cancellationToken = default);
}
