using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI.Ollama;
using Microsoft.KernelMemory.AI.OpenAI;
using Microsoft.KernelMemory.Diagnostics;
using Microsoft.SemanticKernel;

using System.Collections.Concurrent;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

SensitiveDataLogger.Enabled = false;

const string model = "llama3.2:latest";

var config = new OllamaConfig
{
    Endpoint = "http://127.0.0.1:11434",
    TextModel = new(model, 131072),
    EmbeddingModel = new(model, 204800)
};

var memory = new KernelMemoryBuilder()
    .WithOllamaTextGeneration(config, new GPT4Tokenizer())
    .WithOllamaTextEmbeddingGeneration(config, new GPT4oTokenizer())
    .Build();

var filesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "docs");

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.Services.AddAntiforgery();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Enable the app to accept large file uploads
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
});

var app = builder.Build();

app.UseAntiforgery();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

ConcurrentDictionary<string, string> dictionary = new();

app.MapGet("/query", async (string query) =>
{
    // Generate an answer - This uses OpenAI for embeddings and finding relevant data, and LM Studio to generate an answer
    var answer = await memory.AskAsync(query);

    if (answer.NoResult) return answer.NoResultReason is { } reason ? Results.Ok(reason) : Results.Empty;

    return Results.Text(FormatAnswer(answer));
})
.DisableAntiforgery()
.WithName("QueryDocuments");

app.MapGet("/fileNotReady/{fileName}", async ([FromRoute] string fileName) =>
{
    // Generate an answer - This uses OpenAI for embeddings and finding relevant data, and LM Studio to generate an answer
    return !await memory.IsDocumentReadyAsync(Utils.FileNameCleaner.Replace(fileName, ""));
})
.DisableAntiforgery()
.WithName("DocNotReady");

app.MapPost("/upload", async (IFormFile file, CancellationToken token) =>
{
    await TryAddDocumentAsync(file, dictionary, token).ConfigureAwait(false);
    return Results.Ok();
})
.DisableAntiforgery()
.WithName("PostFile");

app.Lifetime.ApplicationStarted.Register(async () =>
{
    if (Directory.Exists(filesDirectory))
    {
        foreach (var file in Directory.EnumerateFiles(filesDirectory))
        {
            await memory.ImportDocumentAsync(file, Utils.FileNameCleaner.Replace(file, ""));
        }
    }
});

app.MapDefaultEndpoints();

app.Run();

async ValueTask<bool> TryAddDocumentAsync(IFormFile file, ConcurrentDictionary<string, string> dictionary, CancellationToken token)
{
    string path = Path.Combine(filesDirectory, file.FileName), fileName = Utils.FileNameCleaner.Replace(file.FileName, "");

    if (!dictionary.TryAdd(file.FileName, fileName) || File.Exists(path)) return false;

    var folder = Path.GetDirectoryName(path)!;

    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

    using FileStream fs = new(path, FileMode.CreateNew);

    await file.CopyToAsync(fs, token);

    await fs.FlushAsync(token);

    if (await memory.GetDocumentStatusAsync(file.Name, cancellationToken: token) is { Completed: true, Empty: false }) return false;

    var id = await memory.ImportDocumentAsync(path, file.FileName, cancellationToken: token);

    return true;

}

static string FormatAnswer(MemoryAnswer answer)
{
    return $@"{answer.Result}

------
{(answer.RelevantSources.Count > 0
            ? string.Join("\n", answer.RelevantSources.Select(c => $"{c.SourceName}: {c.SourceUrl}"))
            : "")}".Trim();
}

internal static partial class Utils
{
    [GeneratedRegex(@"[^A-Za-z0-9._-]+")]
    public static partial Regex FileNameCleaner { get; }
}