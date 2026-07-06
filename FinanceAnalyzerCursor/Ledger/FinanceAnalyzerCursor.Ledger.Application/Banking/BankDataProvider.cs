using FinanceAnalyzerCursor.Ledger.Application.Ports;
using FinanceAnalyzerCursor.Ledger.Domain.Entities;
using Microsoft.Extensions.Options;

namespace FinanceAnalyzerCursor.Ledger.Application.Banking;

public sealed class BankDataProvider : IBankDataProvider
{
    private readonly IBankClient _bankClient;
    private readonly BankDataProviderOptions _options;

    public BankDataProvider(IBankClient bankClient, IOptions<BankDataProviderOptions> options)
    {
        ArgumentNullException.ThrowIfNull(bankClient);
        ArgumentNullException.ThrowIfNull(options);

        _bankClient = bankClient;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<BankTransaction>> GetTransactionsAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        if (from > to)
        {
            throw new ArgumentException("'from' must be less than or equal to 'to'.", nameof(from));
        }

        var pageSize = _options.PageSize;
        var parallelism = _options.MaxParallelRequests;

        if (pageSize <= 0)
        {
            throw new InvalidOperationException($"{nameof(BankDataProviderOptions.PageSize)} must be greater than zero.");
        }

        if (parallelism <= 0)
        {
            throw new InvalidOperationException($"{nameof(BankDataProviderOptions.MaxParallelRequests)} must be greater than zero.");
        }

        var allTransactions = new List<BankTransaction>();
        var offset = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var batchOffsets = CreateBatchOffsets(offset, pageSize, parallelism);
            var batchTasks = batchOffsets
                .Select(batchOffset => FetchPageAsync(from, to, batchOffset, pageSize, cancellationToken))
                .ToArray();

            var pages = await Task.WhenAll(batchTasks);

            var reachedEnd = false;

            foreach (var page in pages.OrderBy(static p => p.Offset))
            {
                if (page.Transactions.Count == 0)
                {
                    reachedEnd = true;
                    break;
                }

                allTransactions.AddRange(page.Transactions);

                if (page.Transactions.Count < pageSize)
                {
                    reachedEnd = true;
                    break;
                }
            }

            if (reachedEnd)
            {
                break;
            }

            offset += parallelism * pageSize;
        }

        cancellationToken.ThrowIfCancellationRequested();

        return allTransactions;
    }

    private async Task<BankTransactionPage> FetchPageAsync(
        DateOnly from,
        DateOnly to,
        int offset,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = new BankTransactionQuery(from, to, offset, pageSize);
        var transactions = await _bankClient
            .GetTransactionsAsync(query, cancellationToken);
        
        return new BankTransactionPage(offset, transactions);
    }

    private static IEnumerable<int> CreateBatchOffsets(int startOffset, int pageSize, int batchSize)
    {
        for (var i = 0; i < batchSize; i++)
        {
            yield return startOffset + i * pageSize;
        }
    }

    private sealed record BankTransactionPage(int Offset, IReadOnlyList<BankTransaction> Transactions);
}
