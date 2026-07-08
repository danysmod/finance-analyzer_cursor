using System.Text.Json.Serialization;
using FinanceAnalyzerCursor.Ledger.ExternalServices.AnonBank1.Helpers;

namespace FinanceAnalyzerCursor.Ledger.ExternalServices.AnonBank1.Models;

public class GetOperationsResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("body")]
    public GetOperationsBodyResponse? Body { get; set; }

    public class GetOperationsBodyResponse
    {
        [JsonPropertyName("operations")]
        public List<Operation>  Operations { get; set; }
        
        public class Operation
        {
            [JsonPropertyName("uohId")]
            public string UohID { get; set; }
            
            [JsonPropertyName("date")]
            [JsonConverter(typeof(AnonBank1DateTimeSerializer))]
            public DateTime Date { get; set; }
            
            [JsonPropertyName("form")]
            public string Form { get; set; }
            
            [JsonPropertyName("description")]
            public string Description { get; set; }
            
            [JsonPropertyName("correspondent")]
            public string Correspondent  { get; set; }
            
            [JsonPropertyName("operationAmount")]
            public AnonBank1Amount? OperationAmount  { get; set; }
            
            [JsonPropertyName("nationalAmount")]
            public AnonBank1Amount? NationalAmount  { get; set; }
            
            [JsonPropertyName("commission")]
            public AnonBank1Amount? CommissionAmount  { get; set; }

            public class AnonBank1Amount
            {
                [JsonPropertyName("amount")]
                public decimal? Amount { get; set; }
            }
        }
    }
}

