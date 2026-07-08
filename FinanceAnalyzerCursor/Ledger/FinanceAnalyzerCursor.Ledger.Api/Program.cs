using FinanceAnalyzerCursor.Ledger.Api.Extensions;
using FinanceAnalyzerCursor.Ledger.Application.Banking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var services = new ServiceCollection();

services.AddLedgerApplication(options =>
{
    options.PageSize = 100;
    options.MaxParallelRequests = 4;
});

services.AddAnonBank1Client(configuration);

using var provider = services.BuildServiceProvider();

var anonBank1 = provider.GetRequiredService<IBankDataProvider>();
