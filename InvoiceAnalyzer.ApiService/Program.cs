using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;

using Microsoft.AspNetCore.Builder;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.DocumentStorage.DevTools;
using Microsoft.KernelMemory.SemanticKernel;
using Microsoft.KernelMemory.Service.AspNetCore;
using Microsoft.SemanticKernel.Connectors.OpenAI;

using Npgsql;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder
    .AddServiceDefaults();

builder.Services
    .AddKernelMemory<MemoryServerless>(memoryBuilder =>
    {
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        memoryBuilder
            .WithPostgresMemoryDb(builder.Configuration.GetConnectionString("rag-db")!)
            .WithSimpleFileStorage(SimpleFileStorageConfig.Persistent)
            .WithSemanticKernelTextGenerationService(
                new OpenAIChatCompletionService(
                    "gpt-4.5-preview",
                    Environment.GetEnvironmentVariable("OPENAI_API_KEY")!),
                new SemanticKernelConfig()
            )
            .WithSemanticKernelTextEmbeddingGenerationService(
                new OpenAITextEmbeddingGenerationService(
                    "text-embedding-ada-002",
                    Environment.GetEnvironmentVariable("OPENAI_API_KEY")!),
                new SemanticKernelConfig()
            );
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    });

if (builder.Environment.IsDevelopment()) 
{
    builder.Services
        .AddEndpointsApiExplorer()
        .AddOpenApi();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.AddKernelMemoryEndpoints();

app.Run();