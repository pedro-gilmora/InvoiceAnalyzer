Environment.SetEnvironmentVariable("ASPIRE_ALLOW_UNSECURED_TRANSPORT", "true");

var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL container with pgvector support
var postgres = builder
    .AddPostgres("postgres-rag")
    .WithImage("pgvector/pgvector")
    .WithImageTag("pg16")
    .WithInitBindMount("resources/init-db")
    .WithDataVolume()
    .WithPgAdmin();

var db = postgres.AddDatabase("rag-db");

var apiService = builder
    .AddProject<Projects.InvoiceAnalyzer_ApiService>("apiservice")
    .WithEnvironment("OPENAI_API_KEY", builder.Configuration["OpenAI:Key"])
    .WithReference(db)
    .WaitFor(db);

builder
    .AddProject<Projects.AiInvoiceAnalyzerClient>("webfrontend")
    .WithEnvironment("OPENAI_API_KEY", builder.Configuration["OpenAI:Key"])
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
