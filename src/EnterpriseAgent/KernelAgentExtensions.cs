using EnterpriseAgent.Client.Models;

using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public static class KernelAgentExtensions
{
    const string modelId = "o3-mini";
    public static WebApplicationBuilder AddKernelAgent(this WebApplicationBuilder appBuilder)
    {
        var openAI = new OpenAIConfig
        {
            EmbeddingModel = "text-embedding-ada-002",
            TextModel = "gpt-4.5-preview",
            APIKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!
        };

#pragma warning disable SKEXP0070, SKEXP0010
        appBuilder
            .Services
                .AddKernelMemory<MemoryServerless>(b => b.WithOpenAIDefaults(openAI.APIKey))
                .AddKernel()
                .AddOpenAIChatCompletion(modelId, openAI.APIKey)
                .AddOpenAITextToImage(modelId, openAI.APIKey)
                .AddOpenAITextEmbeddingGeneration(modelId, openAI.APIKey)
            .Plugins
                .AddFromType<AgentFunctions>();

        appBuilder.Services
            .AddSingleton<OpenAIPromptExecutionSettings>(_ => new()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            })
            .AddTransient<AgentService>();
#pragma warning restore SKEXP0070, SKEXP0010

        return appBuilder;
    }
}