using FinanceAnalyzerCursor.Ledger.Application.Ports;
using FinanceAnalyzerCursor.Ledger.Infrastructure.Banking.Banks.Mock;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceAnalyzerCursor.Ledger.Tests.Extensions;

public static class TestExtensions
{
    public static IServiceCollection AddMockBankClient(
        this IServiceCollection services,
        int totalTransactions = 250)
    {
        services.AddSingleton<IBankClient>(_ => MockBankClient.CreateSample(totalTransactions));
        return services;
    }
}