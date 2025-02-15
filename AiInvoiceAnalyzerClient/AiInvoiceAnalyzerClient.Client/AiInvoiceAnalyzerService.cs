using System.Net.Http.Json;

namespace AiInvoiceAnalyzerClient.Client;

public class AiInvoiceAnalyzerService(HttpClient client)
{
    public async ValueTask<string> UploadAsync(string name, string contentType, byte[] fileBytes)
    {
        StreamContent fileStream = new (new MemoryStream(fileBytes));

        fileStream.Headers.ContentType = new(contentType);

        MultipartFormDataContent content = new()
        {
            { fileStream, "file", name }
        };

        var response = await client.PostAsync("/upload", content);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    public async ValueTask<string> QueryAsync(string query)
    {
        return await client.GetStringAsync($"/query?query={query}");
    }

    public async ValueTask<bool> IsNotReadyDocument(string fileName)
    {
        return await client.GetFromJsonAsync<bool>($"/fileNotReady/{fileName}");
    }
}