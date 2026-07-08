using AutoFixture;
using FinanceAnalyzerCursor.Ledger.Abstractions.Configurations;
using FinanceAnalyzerCursor.Ledger.Abstractions.ExternalServices.Bank;
using FinanceAnalyzerCursor.Ledger.Abstractions.ExternalServices.Bank.Models;
using FinanceAnalyzerCursor.Ledger.Application.DataProvider;
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
    public async Task GetTransactionsAsync_WhenLastPageIsPartial_ReturnsCollectedTransactions()
    {
        // Arrange
        var transactions = CreateTransactions(230);
        var pages = new Dictionary<int, IReadOnlyList<BankTransaction>>
        {
            [0] = transactions.Take(100).ToList(),
            [101] = transactions.Skip(100).Take(100).ToList(),
            [202] = transactions.Skip(200).Take(30).ToList()
        };
        var bankClient = CreateBankClientMock(pages);
        var provider = CreateProvider(bankClient.Object, pageSize: 100, maxParallel: 3);

        // Act
        var result = await provider.GetTransactionsAsync(From, To);

        // Assert
        Assert.Equal(230, result.Count);
        Assert.Equal(
            transactions.Select(t => t.ExternalId).OrderBy(static id => id),
            result.Select(t => t.ExternalId).OrderBy(static id => id));
        bankClient.Verify(
            c => c.GetTransactionsAsync(
                It.Is<BankTransactionQuery>(q => q.Offset == 0 && q.PageSize == 100),
                It.IsAny<CancellationToken>()),
            Times.Once);
        bankClient.Verify(
            c => c.GetTransactionsAsync(
                It.Is<BankTransactionQuery>(q => q.Offset == 101 && q.PageSize == 100),
                It.IsAny<CancellationToken>()),
            Times.Once);
        bankClient.Verify(
            c => c.GetTransactionsAsync(
                It.Is<BankTransactionQuery>(q => q.Offset == 202 && q.PageSize == 100),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetTransactionsAsync_WhenNoTransactions_ReturnsEmpty()
    {
        // Arrange
        var bankClient = CreateBankClientMock(new Dictionary<int, IReadOnlyList<BankTransaction>>());
        var provider = CreateProvider(bankClient.Object);

        // Act
        var result = await provider.GetTransactionsAsync(From, To);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTransactionsAsync_WhenFromGreaterThanTo_Throws()
    {
        // Arrange
        var bankClient = CreateBankClientMock(new Dictionary<int, IReadOnlyList<BankTransaction>>());
        var provider = CreateProvider(bankClient.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            provider.GetTransactionsAsync(new DateOnly(2026, 2, 1), new DateOnly(2026, 1, 1)));
    }

    private static Mock<IBankClient> CreateBankClientMock(
        IReadOnlyDictionary<int, IReadOnlyList<BankTransaction>> pages)
    {
        var bankClient = new Mock<IBankClient>();
        bankClient
            .Setup(c => c.GetTransactionsAsync(It.IsAny<BankTransactionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BankTransactionQuery query, CancellationToken _) =>
                pages.GetValueOrDefault(query.Offset, []));
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
