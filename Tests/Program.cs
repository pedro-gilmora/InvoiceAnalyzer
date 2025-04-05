#pragma warning disable SKEXP0070, SKEXP0010, SKEXP0001
// Create a kernel with OpenAI chat completion
// Warning due to the experimental state of some Semantic Kernel SDK features.


using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Markdig;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

const string modelId = "o3-mini", key = "";

// Environment.SetEnvironmentVariable("ASPIRE_ALLOW_UNSECURED_TRANSPORT", "true");
//VolatileMemoryStore
var builder = Kernel.CreateBuilder()
        .AddOpenAIChatCompletion(modelId, key)
        .AddOpenAITextEmbeddingGeneration(modelId, key)
//.AddOllamaChatCompletion(modelId, endpointUri)
//.AddOllamaTextEmbeddingGeneration(modelId, endpointUri)
;
// Using OpenAI for embeddings
var openAI = new OpenAIConfig
{
    EmbeddingModel = "text-embedding-ada-002",
    TextModel = "o3-mini",
    EmbeddingModelMaxTokenTotal = 8191,
    APIKey = key
};


var memory = new KernelMemoryBuilder()
    .WithOpenAITextEmbeddingGeneration(openAI) // OpenAI
    .WithOpenAITextGeneration(openAI)
    .WithPostgresMemoryDb("")
    .Build(new()
    {
        AllowMixingVolatileAndPersistentData = true
    });

builder.Services.AddSingleton(memory);

builder.Plugins.AddFromType<AnalyzerFunctions>();

await memory.ImportDocumentAsync(@"C:\Users\pedro\Downloads\Telegram Desktop\Think_Data_Structures_Algorithms_and_Information_Retrieval_in_Java.pdf");

var kernel = builder.Build();

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

var chatHistory = new ChatHistory();

var defaultColor = Console.ForegroundColor;

ChatHistory chatMessages = [];

// Get the chat completions
Microsoft.SemanticKernel.Connectors.OpenAI.OpenAIPromptExecutionSettings promptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

while (!AnalyzerFunctions.Stop)
{
    // Get user input;

    var user = Console.ReadLine()!;

    chatMessages.AddUserMessage(user);

    Console.WriteLine("");

    var content =
        await chatCompletionService.GetChatMessageContentAsync(
            chatMessages,
            executionSettings: promptExecutionSettings,
            kernel: kernel);

    // Print the chat completions

    if (content.Content?.Trim() is not { Length: > 0 } message) continue;

    chatMessages.AddAssistantMessage(message);

    Console.Write($"Assistant ({modelId}): {message}");

    // assistantLabel.HtmlElement.InnerHtml = Markdown.ToHtml(contentMessage);

    Console.WriteLine("");
}

public class Factura
{
    [Required]
    [Display(Name = "Número de Factura")]
    public string NumeroFactura { get; set; } = null!;

    [Required]
    [Display(Name = "Fecha de Emisión")]
    public DateTime FechaEmision { get; set; }

    [Required]
    [Display(Name = "Cliente")]
    public Cliente Cliente { get; set; } = new Cliente();

    [Required]
    [Display(Name = "Proveedor")]
    public Proveedor Proveedor { get; set; } = new Proveedor();

    [Required]
    [Display(Name = "Productos")]
    public List<Producto> Productos { get; set; } = new List<Producto>();

    [Required]
    [Display(Name = "Total de la Factura")]
    [Range(0, double.MaxValue, ErrorMessage = "El total debe ser un valor positivo.")]
    public decimal TotalFactura { get; set; }
}

public class Cliente
{
    [Required]
    [StringLength(100, ErrorMessage = "El nombre del cliente no puede exceder los 100 caracteres.")]
    public string Nombre { get; set; } = null!;

    [Required]
    [StringLength(200, ErrorMessage = "La dirección del cliente no puede exceder los 200 caracteres.")]
    public string Direccion { get; set; } = null!;
}

public class Proveedor
{
    [Required]
    [StringLength(100, ErrorMessage = "El nombre del proveedor no puede exceder los 100 caracteres.")]
    public string Nombre { get; set; } = null!;

    [Required]
    [StringLength(200, ErrorMessage = "La dirección del proveedor no puede exceder los 200 caracteres.")]
    public string Direccion { get; set; } = null!;
}

public class Producto
{
    [Required]
    [StringLength(100, ErrorMessage = "El nombre del producto no puede exceder los 100 caracteres.")]
    public string Nombre { get; set; } = null!;

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser un valor no negativo.")]
    public int Cantidad { get; set; }

    [Required]
    [Range(0.0, double.MaxValue, ErrorMessage = "El precio unitario debe ser un valor positivo.")]
    public decimal PrecioUnitario { get; set; }

    [Required]
    [Range(0.0, double.MaxValue, ErrorMessage = "El total del producto debe ser un valor positivo.")]
    public decimal Total => Cantidad * PrecioUnitario;
}
/// <summary>
/// A Sematic Kernel skill that interacts with ChatGPT
/// </summary>
internal class AnalyzerFunctions(IKernelMemory memory)
{
    internal static bool Stop = false;
    public bool IsOn { get; set; } = false;

    [KernelFunction("GetState")]
    [Description("Gets the state of the light.")]
    public string GetState() => this.IsOn ? "on" : "off";

    [KernelFunction("ChangeState")]
    [Description("Changes the state of the light.'")]
    public string ChangeState(bool newState)
    {
        this.IsOn = newState;
        var state = this.GetState();
        // Print the state to the console
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.WriteLine($"[Light is now {state}]");
        Console.ResetColor();

        return state;
    }

    [KernelFunction("GetBillInfo")]
    [Description("Obtiene la factura estructurada")]
    public void GetBillInfo(Factura factura)
    {
        Console.WriteLine(factura);
    }

    [KernelFunction("GetFromOwnMemory")]
    [Description("Useful when user ask something he saved before or given for analysis. Should pass the query he wants to find in stored memory. If empty string is returned, should return from your trained data")]
    public async Task<string> GetFromMemory(string query)
    {
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

    [KernelFunction("EndSession")]
    [Description("Termina la sesión de conversación cuando el usuario lo decida")]
    public void EndSession()
    {

        Stop = true;
    }
}
#pragma warning restore SKEXP0070, SKEXP0010
