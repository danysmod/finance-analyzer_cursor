using System.Threading.Channels;
using FinanceAnalyzerCursor.Ledger.Abstractions.Configurations;
using FinanceAnalyzerCursor.Ledger.Abstractions.ExternalServices.Bank;
using FinanceAnalyzerCursor.Ledger.Abstractions.ExternalServices.Bank.Models;
using FinanceAnalyzerCursor.Ledger.Application.Banking;
using FinanceAnalyzerCursor.Ledger.Domain.Entities;
using Microsoft.Extensions.Options;

namespace FinanceAnalyzerCursor.Ledger.Application.DataProvider;

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
        CancellationToken ct = default)
    {
        var pageSize = _options.PageSize;
        var parallelism = _options.MaxParallelRequests;

        var allTransactions = new List<BankTransaction>();
        var pages = FetchTransactionPages(from, to, pageSize, parallelism, ct);

        await foreach (var page in pages.ReadAllAsync(ct))
        {
            allTransactions.AddRange(page.Transactions);
        }

        return allTransactions;
    }

    private ChannelReader<BankTransactionPage> FetchTransactionPages(
        DateOnly from,
        DateOnly to,
        int pageSize,
        int parallelism,
        CancellationToken ct)
    {
        var stopChannel = Channel.CreateBounded<bool>(
            new BoundedChannelOptions(parallelism)
            {
                SingleReader = true,
                SingleWriter = false
            });

        var requests = StreamRequests(from, to, pageSize, parallelism, stopChannel.Reader, ct);
        var results = Channel.CreateBounded<BankTransactionPage>(
            new BoundedChannelOptions(parallelism)
            {
                SingleReader = true,
                SingleWriter = false
            });

        var workers = Enumerable
            .Range(0, parallelism)
            .Select(_ => WorkerAsync(requests, results.Writer, stopChannel.Writer, pageSize, ct))
            .ToArray();

        _ = CompleteResultsWhenWorkersFinishAsync(workers, results.Writer, stopChannel.Writer);

        return results.Reader;
    }

    private ChannelReader<BankTransactionQuery> StreamRequests(
        DateOnly from,
        DateOnly to,
        int pageSize,
        int parallelism,
        ChannelReader<bool> stop,
        CancellationToken ct)
    {
        var requests = Channel.CreateBounded<BankTransactionQuery>(
            new BoundedChannelOptions(parallelism)
            {
                SingleReader = false,
                SingleWriter = true
            });

        _ = StreamRequestsAsync(from, to, pageSize, requests.Writer, stop, ct);

        return requests.Reader;
    }

    private static async Task StreamRequestsAsync(
        DateOnly from,
        DateOnly to,
        int pageSize,
        ChannelWriter<BankTransactionQuery> requests,
        ChannelReader<bool> stop,
        CancellationToken ct)
    {
        var offset = 0;

        try
        {
            while (!ct.IsCancellationRequested)
            {
                if (stop.TryRead(out _))
                {
                    return;
                }

                var query = new BankTransactionQuery(from, to, offset, pageSize);
                await requests.WriteAsync(query, ct);

                offset += pageSize + 1;
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Cancellation is the normal way to stop the producer from the caller side.
        }
        finally
        {
            requests.TryComplete();
        }
    }

    private async Task WorkerAsync(
        ChannelReader<BankTransactionQuery> requests,
        ChannelWriter<BankTransactionPage> results,
        ChannelWriter<bool> stop,
        int pageSize,
        CancellationToken ct)
    {
        await foreach (var request in requests.ReadAllAsync(ct))
        {
            var transactions = await _bankClient.GetTransactionsAsync(request, ct);

            await results.WriteAsync(new BankTransactionPage(request.Offset, transactions), ct);

            if (transactions.Count != 0 && transactions.Count >= pageSize) continue;
            
            stop.TryWrite(true);
            return;
        }
    }

    private static async Task CompleteResultsWhenWorkersFinishAsync(
        IReadOnlyCollection<Task> workers,
        ChannelWriter<BankTransactionPage> results,
        ChannelWriter<bool> stop)
    {
        try
        {
            await Task.WhenAll(workers);
            results.TryComplete();
        }
        catch (Exception ex)
        {
            results.TryComplete(ex);
        }
        finally
        {
            stop.TryComplete();
        }
    }

    private sealed record BankTransactionPage(int Offset, IReadOnlyList<BankTransaction> Transactions);
}
