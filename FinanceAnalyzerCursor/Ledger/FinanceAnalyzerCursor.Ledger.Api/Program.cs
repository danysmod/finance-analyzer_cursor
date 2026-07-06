using FinanceAnalyzerCursor.Ledger.Api.Extensions;
using FinanceAnalyzerCursor.Ledger.Application.Banking;
using FinanceAnalyzerCursor.Ledger.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddMockBankClient(totalTransactions: 250);
services.AddLedgerApplication(options =>
{
    options.PageSize = 100;
    options.MaxParallelRequests = 4;
});

using var provider = services.BuildServiceProvider();
var bankDataProvider = provider.GetRequiredService<IBankDataProvider>();

var transactions = await bankDataProvider.GetTransactionsAsync(
    from: new DateOnly(2026, 1, 1),
    to: new DateOnly(2026, 1, 31));

Console.WriteLine($"Fetched {transactions.Count} transactions.");
