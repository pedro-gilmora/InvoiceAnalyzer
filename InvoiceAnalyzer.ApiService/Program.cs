#pragma warning disable SKEXP0070, SKEXP0010, SKEXP0001
// Create a kernel with OpenAI chat completion
// Warning due to the experimental state of some Semantic Kernel SDK features.


using MailKit;
using MailKit.Net.Imap;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

using System.ComponentModel;
using System.Reflection.PortableExecutable;

var app = Host.CreateApplicationBuilder(args)
    .AddKernelAgent()
    .Build();

//await app.RunAsync();


await app.Services.GetRequiredService<AgentService>().RunAsync();


// Using OpenAI for embeddings

public class AgentService(Kernel kernel, IChatCompletionService chatCompletionService)
{
    ChatHistory chatMessages = new("You're a helpfull AI assistent able to respond primary from memory using registered functions");


    // Get the chat completions
    Microsoft.SemanticKernel.Connectors.OpenAI.OpenAIPromptExecutionSettings promptExecutionSettings = new()
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
    };

    public async Task RunAsync()
    {
        var content = await chatCompletionService.GetChatMessageContentAsync(chatMessages, promptExecutionSettings, kernel);

        // Print the chat completions

        if (content.Content?.Trim() is { Length: > 0 } message)
        {
            chatMessages.AddAssistantMessage(message);
        }

        chatMessages.AddUserMessage(@"Resume the following file: C:\Users\pedro\Downloads\Telegram Desktop\Think_Data_Structures_Algorithms_and_Information_Retrieval_in_Java.pdf
- Make a list of the most complex data structures
- Which should be the learning order?");

        content = await chatCompletionService.GetChatMessageContentAsync(chatMessages, promptExecutionSettings, kernel);

        // Print the chat completions

        if (content.Content?.Trim() is { Length: > 0 } message2)
        {
            chatMessages.AddAssistantMessage(message2);
        }
    }
}
//while (!AnalyzerFunctions.Stop)
//{
//    // Get user input;

//    var user = Console.ReadLine()!;

//    chatMessages.AddUserMessage(user);

//    Console.WriteLine("");

//    var content =
//        await chatCompletionService.GetChatMessageContentAsync(
//            chatMessages,
//            executionSettings: promptExecutionSettings,
//            kernel: kernel);

//    // Print the chat completions

//    if (content.Content?.Trim() is not { Length: > 0 } message) continue;

//    chatMessages.AddAssistantMessage(message);

//    Console.Write($"Assistant ({modelId}): {message}");

//    // assistantLabel.HtmlElement.InnerHtml = Markdown.ToHtml(contentMessage);

//    Console.WriteLine("");
//}
/// <summary>
/// A Sematic Kernel skill that interacts with ChatGPT
/// </summary>
internal class AgentFunctions(IKernelMemory memory)
{
    const string personnalAppPassword = "oyjqujnwmfrlvwbq";
    const string personnalEmail = "mdrimonakos@gmail.com";
    const string imapServer = "imap.gmail.com";
    const int imapPort = 993;
    const string smtpServer = "smtp.gmail.com";
    const int smtpPort = 587;

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
        using ImapClient client = new ();
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
// using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;

// using Microsoft.AspNetCore.Builder;
// using Microsoft.KernelMemory;
// using Microsoft.KernelMemory.DocumentStorage.DevTools;
// using Microsoft.KernelMemory.SemanticKernel;
// using Microsoft.KernelMemory.Service.AspNetCore;
// using Microsoft.SemanticKernel.Connectors.OpenAI;

// using Npgsql;

// using Scalar.AspNetCore;

// var builder = WebApplication.CreateBuilder(args);

// builder
//     .AddServiceDefaults();

// builder.Services
//     .AddKernelMemory<MemoryServerless>(memoryBuilder =>
//     {
// #pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
//         memoryBuilder
//             .WithPostgresMemoryDb(builder.Configuration.GetConnectionString("rag-db")!)
//             .WithSimpleFileStorage(SimpleFileStorageConfig.Persistent)
//             .WithSemanticKernelTextGenerationService(
//                 new OpenAIChatCompletionService(
//                     "gpt-4.5-preview",
//                     Environment.GetEnvironmentVariable("OPENAI_API_KEY")!),
//                 new SemanticKernelConfig()
//             )
//             .WithSemanticKernelTextEmbeddingGenerationService(
//                 new OpenAITextEmbeddingGenerationService(
//                     "text-embedding-ada-002",
//                     Environment.GetEnvironmentVariable("OPENAI_API_KEY")!),
//                 new SemanticKernelConfig()
//             );
// #pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
//     });

// if (builder.Environment.IsDevelopment()) 
// {
//     builder.Services
//         .AddEndpointsApiExplorer()
//         .AddOpenApi();
// }

// var app = builder.Build();

// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
//     app.MapScalarApiReference();
// }

// app.AddKernelMemoryEndpoints();

// app.Run();

public static class KernelAgentExtensions
{
    const string modelId = "o3-mini";
    public static HostApplicationBuilder AddKernelAgent(this HostApplicationBuilder appBuilder)
    {
        var openAI = new OpenAIConfig
        {
            EmbeddingModel = "text-embedding-ada-002",
            TextModel = "gpt-4.5-preview",
            APIKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!
        };

        appBuilder
            .Services
                .AddKernelMemory(builder =>
                builder
                    .WithOpenAITextEmbeddingGeneration(openAI)
                    .WithOpenAITextGeneration(openAI)
                    .WithSimpleVectorDb(new Microsoft.KernelMemory.MemoryStorage.DevTools.SimpleVectorDbConfig()
                    {
                        StorageType = Microsoft.KernelMemory.FileSystem.DevTools.FileSystemTypes.Volatile,
                        Directory = "D:\\Code\\InvoiceAnalyzer\\InvoiceAnalyzer.ApiService\\bin\\Debug\\net9.0\\SimpleVectorDb"
                    }),
                new()
                {
                    AllowMixingVolatileAndPersistentData = true
                });
        appBuilder
            .Services
                .AddKernel()
                .AddOpenAIChatCompletion(modelId, openAI.APIKey)
                .AddOpenAITextEmbeddingGeneration(modelId, openAI.APIKey)
            .Plugins
                .AddFromType<AgentFunctions>();

        appBuilder.Services.AddTransient<AgentService>();
        return appBuilder;
    }
}
#pragma warning restore SKEXP0070, SKEXP0010