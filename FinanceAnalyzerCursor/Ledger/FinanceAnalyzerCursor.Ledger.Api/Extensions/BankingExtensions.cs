using FinanceAnalyzerCursor.Ledger.Abstractions.Configurations;
using FinanceAnalyzerCursor.Ledger.Application.Banking;
using FinanceAnalyzerCursor.Ledger.Application.DataProvider;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FinanceAnalyzerCursor.Ledger.Api.Extensions;

public static class BankingExtensions
{
    public static IServiceCollection AddLedgerApplication(
        this IServiceCollection services,
        Action<BankDataProviderOptions>? configureBankDataProvider = null)
    {
        services.AddOptions<BankDataProviderOptions>();

        if (configureBankDataProvider is not null)
        {
            services.Configure(configureBankDataProvider);
        }

        services.TryAddSingleton<IBankDataProvider, BankDataProvider>();

        return services;
    }
}