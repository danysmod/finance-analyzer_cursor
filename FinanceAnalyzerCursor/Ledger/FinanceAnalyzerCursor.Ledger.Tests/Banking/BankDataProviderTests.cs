using FinanceAnalyzerCursor.Ledger.Application.Banking;
using FinanceAnalyzerCursor.Ledger.Application.Ports;
using FinanceAnalyzerCursor.Ledger.Domain.Entities;
using FinanceAnalyzerCursor.Ledger.Infrastructure.Banking.Banks.Mock;
using Microsoft.Extensions.Options;

namespace FinanceAnalyzerCursor.Ledger.Tests.Banking;

public sealed class BankDataProviderTests
{
    private static readonly DateOnly From = new(2026, 1, 1);
    private static readonly DateOnly To = new(2026, 1, 31);

    [Fact]
    public async Task GetTransactionsAsync_WhenNoTransactions_ReturnsEmpty()
    {
        var provider = CreateProvider(new StubBankClient([]));

        var result = await provider.GetTransactionsAsync(From, To);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTransactionsAsync_WhenSinglePartialPage_ReturnsAllTransactions()
    {
        var transactions = CreateTransactions(37);
        var provider = CreateProvider(new StubBankClient(transactions), pageSize: 100);

        var result = await provider.GetTransactionsAsync(From, To);

        Assert.Equal(37, result.Count);
        Assert.Equal(transactions.Select(t => t.ExternalId), result.Select(t => t.ExternalId));
    }

    [Fact]
    public async Task GetTransactionsAsync_WhenExactPageSize_ReturnsAllTransactions()
    {
        var transactions = CreateTransactions(100);
        var provider = CreateProvider(new StubBankClient(transactions), pageSize: 100);

        var result = await provider.GetTransactionsAsync(From, To);

        Assert.Equal(100, result.Count);
    }

    [Fact]
    public async Task GetTransactionsAsync_WhenMultipleBatches_ReturnsAllTransactions()
    {
        var transactions = CreateTransactions(250);
        var provider = CreateProvider(new StubBankClient(transactions), pageSize: 100, maxParallel: 4);

        var result = await provider.GetTransactionsAsync(From, To);

        Assert.Equal(250, result.Count);
        Assert.Equal(transactions.Select(t => t.ExternalId), result.Select(t => t.ExternalId));
    }

    [Fact]
    public async Task GetTransactionsAsync_WhenFromGreaterThanTo_Throws()
    {
        var provider = CreateProvider(new StubBankClient([]));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            provider.GetTransactionsAsync(new DateOnly(2026, 2, 1), new DateOnly(2026, 1, 1)));
    }

    [Fact]
    public async Task GetTransactionsAsync_WithMockBankClient_FetchesAllSampleTransactions()
    {
        var mockClient = MockBankClient.CreateSample(250);
        var provider = CreateProvider(mockClient, pageSize: 100, maxParallel: 4);

        var result = await provider.GetTransactionsAsync(From, To);

        Assert.Equal(250, result.Count);
    }

    private static BankDataProvider CreateProvider(
        IBankClient bankClient,
        int pageSize = 100,
        int maxParallel = 4)
    {
        var options = Options.Create(new BankDataProviderOptions
        {
            PageSize = pageSize,
            MaxParallelRequests = maxParallel
        });

        return new BankDataProvider(bankClient, options);
    }

    private static List<BankTransaction> CreateTransactions(int count)
    {
        return Enumerable
            .Range(0, count)
            .Select(i => new BankTransaction(
                ExternalId: $"tx-{i:D5}",
                Date: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero).AddDays(i),
                Amount: -i,
                Currency: "RUB",
                Description: $"Transaction {i}"))
            .ToList();
    }

    private sealed class StubBankClient(IReadOnlyList<BankTransaction> transactions) : IBankClient
    {
        public string BankId => "stub";

        public Task<IReadOnlyList<BankTransaction>> GetTransactionsAsync(
            BankTransactionQuery query,
            CancellationToken cancellationToken = default)
        {
            var page = transactions
                .Skip(query.Offset)
                .Take(query.PageSize)
                .ToList();

            return Task.FromResult<IReadOnlyList<BankTransaction>>(page);
        }
    }
}
