namespace FinanceAnalyzerCursor.Ledger.ExternalServices.AnonBank1;

public sealed class AnonBank1ClientConfiguration
{
    public const string SectionName = "AnonBank1ClientSettings";

    public string BaseUrl { get; set; } = string.Empty;
    
    public Handler Handlers { get; set; } = new();
}

public sealed class Handler
{
    public string GetOperationsPath { get; set; } = string.Empty;
}