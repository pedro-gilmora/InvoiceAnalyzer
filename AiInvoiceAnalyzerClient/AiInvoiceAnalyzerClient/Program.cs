using AiInvoiceAnalyzerClient.Client.Models;
using AiInvoiceAnalyzerClient.Components;

using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;

var builder = WebApplication
    .CreateBuilder(args)
    .AddServiceDefaults();

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents().Services
    .AddOpenAIChatCompletion("o3-mini", new OpenAI.OpenAIClient(Environment.GetEnvironmentVariable("OPENAI_API_KEY")!))
    .AddFluentUIComponents()
    .AddHttpClient<EnterpriseAgent>(static client => client.BaseAddress = new("https+http://apiservice"))
    .Services.AddTransient<EnterpriseAgent>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection()
    .UseStaticFiles()
    .UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(AiInvoiceAnalyzerClient.Client._Imports).Assembly);

app.Run();
