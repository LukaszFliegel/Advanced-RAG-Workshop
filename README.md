# Advanced RAG Workshop

A simple RAG (Retrieval-Augmented Generation) application using Semantic Kernel and Azure OpenAI Service.

## Prerequisites

- .NET 8.0 SDK
- Azure OpenAI Service resource

## Setup

1. **Clone the repository** (if not already done)

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Configure Azure OpenAI Settings**

   You have several options to configure your Azure OpenAI settings:

   ### Option 1: User Secrets (Recommended for development)
   ```bash
   dotnet user-secrets set "AzureOpenAI:Endpoint" "https://your-resource-name.openai.azure.com/"
   dotnet user-secrets set "AzureOpenAI:ApiKey" "your-api-key-here"
   dotnet user-secrets set "AzureOpenAI:DeploymentName" "your-deployment-name"
   ```

   ### Option 2: Environment Variables
   ```bash
   $env:AzureOpenAI__Endpoint="https://your-resource-name.openai.azure.com/"
   $env:AzureOpenAI__ApiKey="your-api-key-here"
   $env:AzureOpenAI__DeploymentName="your-deployment-name"
   ```

   ### Option 3: appsettings.json (Not recommended for API keys)
   Edit the `appsettings.json` file and replace the placeholder values.

## Running the Application

```bash
dotnet run
```

## Goal

Goal of this workshop is to show more advanced RAG concept. It is a continuation of basic RAG workshop, so it is assumed that you are familiar with basic RAG concepts and Semantic Kernel usage.
Workshop is diveded into parts:
- TODO: add workshop parts here

If you are lost, you can check branches for each step of the workshop:
- main - initial skeleton of the console app with basic RAG capabilities
- TODO: add branches for each step of the workshop

## Assumptions

To keep the code short and presentable during online session, some assumptions were made:
- examples will be kept minimal and focused on core concepts to aid understanding
- we won't create interfaces for services and use DI container, but rather simple service classes with single responsibility.
- no error handling/logging/retries etc.