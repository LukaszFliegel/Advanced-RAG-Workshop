using Advanced_RAG_Workshop.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Advanced_RAG_Workshop.Services;

// TODO milestone-1 : Implement query analysis and rewriting using Azure OpenAI
// Feel free to experiment, for example try different QueryType
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

    public QueryEnhancementService(AzureOpenAIConfig config)
    {

    }

    public async Task<string> RewriteQueryAsync(string query)
    {
        return query;
    }

    public async Task<QueryAnalysis> AnalyzeQueryAsync(string query)
    {        
        var analysis = new QueryAnalysis
        {
            Type = QueryType.Factual,
            RewrittenQuery = await RewriteQueryAsync(query),
            Reasoning = "..."
        };

        return analysis;
    }
}

