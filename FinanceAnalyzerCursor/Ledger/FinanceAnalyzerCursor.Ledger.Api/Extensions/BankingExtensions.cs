using FinanceAnalyzerCursor.Ledger.Abstractions.Configurations;
using FinanceAnalyzerCursor.Ledger.Abstractions.ExternalServices.Bank;
using FinanceAnalyzerCursor.Ledger.Application.Banking;
using FinanceAnalyzerCursor.Ledger.Application.DataProvider;
using FinanceAnalyzerCursor.Ledger.ExternalServices.AnonBank1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FinanceAnalyzerCursor.Ledger.Api.Extensions;

public static class BankingExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddLedgerApplication(Action<BankDataProviderOptions>? configureBankDataProvider = null)
        {
            services.AddOptions<BankDataProviderOptions>();

            if (configureBankDataProvider is not null)
            {
                services.Configure(configureBankDataProvider);
            }

            services.TryAddSingleton<IBankDataProvider, BankDataProvider>();

            return services;
        }

        public IServiceCollection AddAnonBank1Client(IConfiguration configuration)
        {
            var section = configuration.GetRequiredSection(AnonBank1ClientConfiguration.SectionName);

            services
                .AddOptions<AnonBank1ClientConfiguration>()
                .Bind(section);
            services.AddHttpClient<IBankClient, AnonBank1Client>();

            return services;
        }
    }
}