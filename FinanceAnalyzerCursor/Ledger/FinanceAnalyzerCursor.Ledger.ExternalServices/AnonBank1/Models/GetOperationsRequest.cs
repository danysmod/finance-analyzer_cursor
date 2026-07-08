using System.Text.Json.Serialization;
using FinanceAnalyzerCursor.Ledger.ExternalServices.AnonBank1.Helpers;

namespace FinanceAnalyzerCursor.Ledger.ExternalServices.AnonBank1.Models;

public class GetOperationsRequest
{
    [JsonPropertyName("paginationOffset")] 
    public long Offset { get; set; }
    
    [JsonPropertyName("paginationSize")] 
    public long Limit { get; set; }

    [JsonPropertyName("showHidden")] 
    public bool ShowHidden { get; set; }

    [JsonPropertyName("showNotTransactionBonuses")]
    public bool ShowNotTransactionBonuses { get; set; }

    [JsonPropertyName("from")]
    [JsonConverter(typeof(AnonBank1DateTimeSerializer))]
    public DateTime From { get; set; }

    [JsonPropertyName("to")] 
    [JsonConverter(typeof(AnonBank1DateTimeSerializer))]
    public DateTime To { get; set; }
}