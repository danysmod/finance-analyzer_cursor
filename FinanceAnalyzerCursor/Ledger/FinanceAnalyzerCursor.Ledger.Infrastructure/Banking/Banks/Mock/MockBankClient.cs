using FinanceAnalyzerCursor.Ledger.Application.Banking;
using FinanceAnalyzerCursor.Ledger.Application.Ports;
using FinanceAnalyzerCursor.Ledger.Domain.Entities;

namespace FinanceAnalyzerCursor.Ledger.Infrastructure.Banking.Banks.Mock;

public sealed class MockBankClient : IBankClient
{
    private readonly IReadOnlyList<BankTransaction> _transactions;

    public MockBankClient(IReadOnlyList<BankTransaction> transactions)
    {
        _transactions = transactions;
    }

    public string BankId => "mock";

    public Task<IReadOnlyList<BankTransaction>> GetTransactionsAsync(
        BankTransactionQuery query,
        CancellationToken cancellationToken = default)
    {
        var filtered = _transactions
            .Where(t => DateOnly.FromDateTime(t.Date.DateTime) >= query.From
                        && DateOnly.FromDateTime(t.Date.DateTime) <= query.To)
            .OrderBy(t => t.Date)
            .ThenBy(t => t.ExternalId)
            .Skip(query.Offset)
            .Take(query.PageSize)
            .ToList();

        return Task.FromResult<IReadOnlyList<BankTransaction>>(filtered);
    }

    public static MockBankClient CreateSample(int totalTransactions = 250)
    {
        var transactions = Enumerable
            .Range(0, totalTransactions)
            .Select(i => new BankTransaction(
                ExternalId: $"tx-{i:D5}",
                Date: new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero).AddDays(i % 30),
                Amount: -100 - i,
                Currency: "RUB",
                Description: $"Mock transaction #{i}"))
            .ToList();

        return new MockBankClient(transactions);
    }
}
