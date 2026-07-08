using AutoFixture;
using FinanceAnalyzerCursor.Ledger.Abstractions.External;
using FinanceAnalyzerCursor.Ledger.Application.Banking;
using FinanceAnalyzerCursor.Ledger.Domain.Entities;
using Microsoft.Extensions.Options;
using Moq;

namespace FinanceAnalyzerCursor.Ledger.Tests.Banking;

public sealed class BankDataProviderTests
{
    private static readonly DateOnly From = new(2026, 1, 1);
    private static readonly DateOnly To = new(2026, 1, 31);
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task GetTransactionsAsync_WhenNoTransactions_ReturnsEmpty()
    {
        // Arrange
        var bankClient = CreateBankClientMock([]);
        var provider = CreateProvider(bankClient.Object);

        // Act
        var result = await provider.GetTransactionsAsync(From, To);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTransactionsAsync_WhenSinglePartialPage_ReturnsAllTransactions()
    {
        // Arrange
        var transactions = CreateTransactions(37);
        var bankClient = CreateBankClientMock(transactions);
        var provider = CreateProvider(bankClient.Object, pageSize: 100);

        // Act
        var result = await provider.GetTransactionsAsync(From, To);

        // Assert
        Assert.Equal(37, result.Count);
        Assert.Equal(transactions.Select(t => t.ExternalId), result.Select(t => t.ExternalId));
    }

    [Fact]
    public async Task GetTransactionsAsync_WhenExactPageSize_ReturnsAllTransactions()
    {
        // Arrange
        var transactions = CreateTransactions(100);
        var bankClient = CreateBankClientMock(transactions);
        var provider = CreateProvider(bankClient.Object, pageSize: 100);

        // Act
        var result = await provider.GetTransactionsAsync(From, To);

        // Assert
        Assert.Equal(100, result.Count);
    }

    [Fact]
    public async Task GetTransactionsAsync_WhenMultipleBatches_ReturnsAllTransactions()
    {
        // Arrange
        var transactions = CreateTransactions(250);
        var bankClient = CreateBankClientMock(transactions);
        var provider = CreateProvider(bankClient.Object, pageSize: 100, maxParallel: 4);

        // Act
        var result = await provider.GetTransactionsAsync(From, To);

        // Assert
        Assert.Equal(250, result.Count);
        Assert.Equal(transactions.Select(t => t.ExternalId), result.Select(t => t.ExternalId));
    }

    [Fact]
    public async Task GetTransactionsAsync_WhenFromGreaterThanTo_Throws()
    {
        // Arrange
        var bankClient = CreateBankClientMock([]);
        var provider = CreateProvider(bankClient.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            provider.GetTransactionsAsync(new DateOnly(2026, 2, 1), new DateOnly(2026, 1, 1)));
    }

    private Mock<IBankClient> CreateBankClientMock(IReadOnlyList<BankTransaction> transactions)
    {
        var bankClient = new Mock<IBankClient>();
        bankClient
            .Setup(c => c.GetTransactionsAsync(It.IsAny<BankTransactionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BankTransactionQuery query, CancellationToken _) =>
                transactions.Skip(query.Offset).Take(query.PageSize).ToList());
        return bankClient;
    }

    private BankDataProvider CreateProvider(
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

    private List<BankTransaction> CreateTransactions(int count)
    {
        return _fixture
            .Build<BankTransaction>()
            .With(t => t.ExternalId, () => _fixture.Create<string>())
            .With(t => t.Date, () => new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero).AddDays(_fixture.Create<int>() % 30))
            .With(t => t.Amount, () => -_fixture.Create<decimal>())
            .With(t => t.Currency, "RUB")
            .CreateMany(count)
            .Select((transaction, index) => transaction with { ExternalId = $"tx-{index:D5}" })
            .ToList();
    }
}
