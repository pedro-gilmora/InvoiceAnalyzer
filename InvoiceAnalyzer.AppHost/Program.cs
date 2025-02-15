Environment.SetEnvironmentVariable("ASPIRE_ALLOW_UNSECURED_TRANSPORT", "true");

var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.InvoiceAnalyzer_ApiService>("apiservice");

builder.AddProject<Projects.AiInvoiceAnalyzerClient>("webfrontend")
    .WithReference(apiService)
    .WaitFor(apiService);    

builder.Build().Run();
