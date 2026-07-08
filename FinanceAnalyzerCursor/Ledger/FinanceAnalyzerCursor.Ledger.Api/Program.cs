using FinanceAnalyzerCursor.Ledger.Abstractions.ExternalServices;
using FinanceAnalyzerCursor.Ledger.Abstractions.ExternalServices.Bank;
using FinanceAnalyzerCursor.Ledger.Api.Extensions;
using FinanceAnalyzerCursor.Ledger.Application.Banking;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddLedgerApplication(options =>
{
    options.PageSize = 100;
    options.MaxParallelRequests = 4;
});

// Register IBankClient when a bank adapter is available:
// services.AddSberBankClient(configuration);

using var provider = services.BuildServiceProvider();

if (provider.GetService<IBankClient>() is null)
{
    Console.WriteLine("Register IBankClient in composition root to fetch transactions.");
    return;
}

var bankDataProvider = provider.GetRequiredService<IBankDataProvider>();

var transactions = await bankDataProvider.GetTransactionsAsync(
    from: new DateOnly(2026, 1, 1),
    to: new DateOnly(2026, 1, 31));

Console.WriteLine($"Fetched {transactions.Count} transactions.");
