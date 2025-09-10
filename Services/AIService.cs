using Advanced_RAG_Workshop.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Advanced_RAG_Workshop.Services;

public class AIService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly ChatHistory _chatHistory;

    public AIService(AzureOpenAIConfig config)
    {
        var builder = Kernel.CreateBuilder();
        
        builder.AddAzureOpenAIChatCompletion(
            deploymentName: config.DeploymentName,
            endpoint: config.Endpoint,
            apiKey: config.ApiKey);

        _kernel = builder.Build();
        _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        _chatHistory = new ChatHistory();

        _chatHistory.AddSystemMessage("You are a helpful assistant.");
    }

    public async IAsyncEnumerable<string> GetStreamingResponse(string prompt)
    {
        _chatHistory.AddUserMessage(prompt);

        var response = _chatCompletionService.GetStreamingChatMessageContentsAsync(_chatHistory);

        var fullResponse = new System.Text.StringBuilder();

        await foreach (var content in response)
        {
            if (!string.IsNullOrEmpty(content.Content))
            {
                fullResponse.Append(content.Content);
                yield return content.Content;
            }
        }

        _chatHistory.AddAssistantMessage(fullResponse.ToString());
    }
}
