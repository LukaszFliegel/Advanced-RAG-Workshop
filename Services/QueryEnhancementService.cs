using Advanced_RAG_Workshop.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Advanced_RAG_Workshop.Services;

public enum QueryType
{
    Factual,          // "What is chocolate?" "When was chocolate discovered?"
    SmallTalk,        // "Tell me a joke", "How are you?"
    Ambiguous         // Unclear intent
}

public class QueryAnalysis
{
    public QueryType Type { get; set; }
    public string RewrittenQuery { get; set; } = "";
    public string Reasoning { get; set; } = "";
}

public class QueryEnhancementService
{
    private readonly IChatCompletionService _chatService;
    
    public QueryEnhancementService(AzureOpenAIConfig config)
    {
        var kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: config.DeploymentName,
                endpoint: config.Endpoint,
                apiKey: config.ApiKey)
            .Build();
            
        _chatService = kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<string> RewriteQueryAsync(string query)
    {
        // First analyze the query to understand its type and intent
        var analysis = await AnalyzeQueryAsync(query);

        // Use the analysis to create a better, more targeted rewrite prompt
        var rewritePrompt = $$"""
        Rewrite this search query to be more specific and searchable for a knowledge base.
        
        Original Query: "{{query}}"
        Query Type: {{analysis.Type}}
        Analysis Reasoning: {{analysis.Reasoning}}
        
        Based on the query type ({{analysis.Type}}), rewrite the query to be more specific:
        
        {{GetRewriteGuidelines(analysis.Type)}}
        
        Return only the rewritten query, no explanation.
        """;

        var response = await _chatService.GetChatMessageContentAsync(rewritePrompt);
        return response.Content?.Trim() ?? query;
    }

    public async Task<QueryAnalysis> AnalyzeQueryAsync(string query)
    {
        var analysisPrompt = $$"""
        Analyze this user query for a knowledge base search system:
        Query: "{{query}}"
        
        Provide analysis in this JSON format:
        {
            "Type": "Factual|SmallTalk|Ambiguous",
            "RewrittenQuery": "clearer version of the query",
            "Reasoning": "explanation of your analysis"
        }
        
        Classification guidelines:
        - Factual: Direct questions about facts, definitions (What is chocolate?)
        - SmallTalk: Casual conversation, jokes, greetings (Tell me a joke)
        - Ambiguous: Unclear intent, needs clarification
        """;

        var response = await _chatService.GetChatMessageContentAsync(analysisPrompt);
        
        return ParseQueryAnalysis(response.Content ?? "");
    }    

    private string GetRewriteGuidelines(QueryType queryType)
    {
        return queryType switch
        {
            QueryType.Factual => "- Add specific aspects you want to know (definition, properties, characteristics)\n- Be explicit about what kind of facts are needed",
            QueryType.SmallTalk => "",
            QueryType.Ambiguous => "- Add context about chocolate/cocoa/food domain\n- Make the intent more specific and clear",
            _ => ""
        };
    }

    private QueryAnalysis ParseQueryAnalysis(string jsonResponse)
    {
        var cleanedResponse = jsonResponse.Replace("```json", "").Replace("```", "");

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        var analysis = JsonSerializer.Deserialize<QueryAnalysis>(cleanedResponse, options);
        return analysis;
    }
}
