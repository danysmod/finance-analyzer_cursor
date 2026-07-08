using System.Text;
using System.Text.Json;
using FinanceAnalyzerCursor.Ledger.Abstractions.ExternalServices.Bank;
using FinanceAnalyzerCursor.Ledger.Abstractions.ExternalServices.Bank.Models;
using FinanceAnalyzerCursor.Ledger.Domain.Entities;
using FinanceAnalyzerCursor.Ledger.ExternalServices.AnonBank1.Models;
using Microsoft.Extensions.Options;

namespace FinanceAnalyzerCursor.Ledger.ExternalServices.AnonBank1;

public class AnonBank1Client : IBankClient
{
    private readonly HttpClient _httpClient;
    private readonly string _getOperationsPath;

    public AnonBank1Client(
        HttpClient httpClient,
        IOptions<AnonBank1ClientConfiguration> options)
    {
        var configuration = options.Value;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(configuration.BaseUrl, UriKind.Absolute);
        _getOperationsPath = configuration.Handlers.GetOperationsPath;
    }

    public async Task<IReadOnlyList<BankTransaction>> GetOperations(
        BankTransactionQuery query, 
        CancellationToken cancellationToken)
    {
        var requestBody = BuildRequestMessage(query);
        
        var httpResponse = await _httpClient.SendAsync(requestBody, cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception("Error: " + httpResponse.StatusCode);
        }
        
        var responseBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
        var responseModel = JsonSerializer.Deserialize<GetOperationsResponse>(responseBody);

        if (responseModel is not { Success: true })
        {
            throw new Exception("Error on request");
            
            //TODO debug logs for response logger
        }
        
        return Array.Empty<BankTransaction>();
    }

    private HttpRequestMessage BuildRequestMessage(BankTransactionQuery query)
    {
        var requestBody = JsonSerializer.Serialize(new GetOperationsRequest
        {
            From = query.From.ToDateTime(new TimeOnly(0, 0)),
            To = query.To.ToDateTime(new TimeOnly(23, 59)),
            Limit = query.PageSize,
            Offset = query.Offset,
            ShowHidden = false,
            ShowNotTransactionBonuses = false
        });
        var request =  new HttpRequestMessage(HttpMethod.Post, _getOperationsPath);

        request.Headers.Add("Cookie", query.AuthToken);
        request.Content = new StringContent(requestBody, new UTF8Encoding(),  "application/json");

        return request;
    }
}