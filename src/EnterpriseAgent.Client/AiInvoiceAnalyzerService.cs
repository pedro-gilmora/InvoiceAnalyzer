using Microsoft.KernelMemory;

using System.Net.Http.Json;

namespace AiInvoiceAnalyzerClient.Client;

public class AiService(MemoryWebClient client)
{
    public async ValueTask<string> UploadAsync(string name, string contentType, byte[] fileBytes)
    {
        using MemoryStream fileStream = new (fileBytes);

        var response = await client.ImportDocumentAsync(fileStream);

        return response;
    }

    public async ValueTask<MemoryAnswer> QueryAsync(string query)
    {
        return await client.AskAsync(query);
    }

    public async ValueTask<bool> IsNotReadyDocument(string fileName)
    {
        return await client.IsDocumentReadyAsync(fileName);
    }
}