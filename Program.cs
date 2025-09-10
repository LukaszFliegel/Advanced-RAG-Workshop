using Advanced_RAG_Workshop.Models;
using Advanced_RAG_Workshop.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

// Configure console for Unicode support
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

// Bind Azure OpenAI configuration
var azureOpenAIConfig = new AzureOpenAIConfig();
configuration.GetSection("AzureOpenAI").Bind(azureOpenAIConfig);

// instantiate services
var aiService = new AIService(azureOpenAIConfig);
var vectorDbService = new VectorDatabaseService(azureOpenAIConfig);
var documentService = new DocumentService();
var ragService = new RAGService(aiService, vectorDbService);

// Initialize and vectorize documents at startup
await InitializeDocuments();

// Run the application
try
{
    Console.WriteLine("🤖 Welcome to Advanced RAG Workshop Chat!");
    Console.WriteLine("This is a RAG application using Semantic Kernel and Azure OpenAI.");

    Console.WriteLine("\nCommands:");
    Console.WriteLine("  - Type your message to chat with the AI (with RAG)");
    Console.WriteLine("  - Type '/chat <message>' for chat without RAG");
    Console.WriteLine("  - Type '/search <query>' to search documents only");
    Console.WriteLine("  - Type 'exit' to quit");
    Console.WriteLine();

    while (true)
    {
        // Display user prompt
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("User: ");
        Console.ResetColor();

        var userInput = Console.ReadLine();

        if (string.IsNullOrEmpty(userInput) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("👋 Goodbye!");
            break;
        }

        try
        {
            if (userInput.StartsWith("/chat ", StringComparison.OrdinalIgnoreCase))
            {
                // Regular chat without RAG
                var chatMessage = userInput.Substring(6);
                await HandleRegularChatAsync(chatMessage);
            }
            else if (userInput.StartsWith("/search ", StringComparison.OrdinalIgnoreCase))
            {
                // Search documents only
                var searchQuery = userInput.Substring(8);
                await HandleDocumentSearchAsync(searchQuery);
            }
            else
            {
                // RAG-enhanced chat (default)
                await HandleRAGChatAsync(userInput);
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Error: {ex.Message}\n");
            Console.ResetColor();
        }
    }

    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"Application error: {ex.Message}");
    return 1;
}

async Task InitializeDocuments()
{
    Console.WriteLine("🚀 Initializing RAG system...");

    // Initialize vector database
    await vectorDbService.InitializeAsync();

    // Process documents and add to vector database
    var documentsPath = Path.Combine(Directory.GetCurrentDirectory(), "Documents");
    var documents = await documentService.ProcessDocumentsAsync(documentsPath);

    if (documents.Count > 0)
    {
        await vectorDbService.AddDocumentsAsync(documents);
    }
    else
    {
        Console.WriteLine("⚠️  No documents found to process. Add PDF files to the Documents folder for RAG functionality.");
    }

    Console.WriteLine("✅ RAG system initialization complete!");
}

async Task HandleRegularChatAsync(string message)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("Assistant: ");
    Console.ResetColor();

    var responseStream = aiService.GetStreamingResponse(message);
    await foreach (var chunk in responseStream)
    {
        Console.Write(chunk);
    }
    Console.WriteLine("\n");
}

async Task HandleDocumentSearchAsync(string query)
{
    var searchResults = await ragService.SearchDocumentsAsync(query);

    for (int i = 0; i < searchResults.Count; i++)
    {
        var result = searchResults[i];
        Console.WriteLine($"📄 Result {i + 1} (Relevance: {result.Score:F2})");
        Console.WriteLine($"Source: {result.Record.SourceFile}");
        Console.WriteLine($"Content: {result.Record.Content.Substring(0, 200)}");
        Console.WriteLine(new string('-', 50));
    }
    Console.WriteLine();
}

async Task HandleRAGChatAsync(string query)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("Assistant: ");
    Console.ResetColor();

    var responseStream = await ragService.GetRAGResponseAsync(query);
    await foreach (var chunk in responseStream)
    {
        Console.Write(chunk);
    }
    Console.WriteLine("\n");
}