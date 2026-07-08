namespace FinanceAnalyzerCursor.Ledger.Abstractions.Configurations;

public sealed class BankDataProviderOptions
{
    public const string SectionName = "BankDataProvider";

    public int PageSize { get; set; } = 100;

    public int MaxParallelRequests { get; set; } = 4;
}
