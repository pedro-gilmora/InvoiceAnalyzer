using EnterpriseAgent.Components;

using MailKit;
using MailKit.Net.Imap;

using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;

using System.ComponentModel;

var builder = WebApplication
    .CreateBuilder(args);

var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;

builder
    .Services
    .AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .Services
    .AddFluentUIComponents();

builder.AddKernelAgent();

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
    .AddAdditionalAssemblies(typeof(EnterpriseAgent.Client._Imports).Assembly);

app.Run();

internal class AgentFunctions(IKernelMemory memory)
{
    #region info
    const string personnalAppPassword = "oyjqujnwmfrlvwbq";
    const string personnalEmail = "mdrimonakos@gmail.com";
    const string imapServer = "imap.gmail.com";
    const int imapPort = 993;
    //const string smtpServer = "smtp.gmail.com";
    //const int smtpPort = 587;
    #endregion
    [KernelFunction("GetFromOwnMemory")]
    [Description("Useful when user ask something he saved before or given for analysis. Should pass the query he wants to find in stored memory. If empty string is returned, should return from your trained data")]
    public async Task<string> GetFromMemory(string filePath, string query)
    {
        try
        {
            var id = await memory.ImportDocumentAsync(filePath);

            var memoryAnswer = await memory.AskAsync(query);

            if (memoryAnswer is { NoResult: false, Result.Length: > 0 })
            {
                return memoryAnswer.Result;
            }
            else
            {
                return "";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return "An error ocurred";
        }
    }

    [KernelFunction("GetLastNEmails")]
    [Description("Useful when user ask to review his last N emails")]
    public async Task<string> GetLastNEmails(int nLastCount, string query)
    {
        using ImapClient client = new();
        try
        {
            await client.ConnectAsync(imapServer, imapPort, true);
            await client.AuthenticateAsync(personnalEmail, personnalAppPassword);
            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly);

            var emails = (await inbox.FetchAsync(0, nLastCount, MessageSummaryItems.Full));

            return string.Join("\n", emails.Select(w => $@"- From: {w.Envelope.From}
Subject: {w.NormalizedSubject}
Body: {w.HtmlBody?.ToString() ?? w.TextBody?.ToString()}"));
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }
}
