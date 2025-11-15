using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;
using OpenAI;
using System.ComponentModel;
using ScriptRunnerLib;

var builder = WebApplication.CreateBuilder(args);

string apiKey = builder.Configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key not found in configuration.");

var chatClient = new OpenAIClient(apiKey).GetChatClient("gpt-5.1");

builder.Services.AddChatClient(chatClient.AsIChatClient());
// Register your agents
builder.AddAIAgent("Gpt51_assistant", "You are a helpful assistant.").WithAITool(AIFunctionFactory.Create(GetWeather));

// Register services for OpenAI responses and conversations (also required for DevUI)
builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

var app = builder.Build();

// Map endpoints for OpenAI responses and conversations (also required for DevUI)
app.MapOpenAIResponses();
app.MapOpenAIConversations();

if (builder.Environment.IsDevelopment())
{
    // Map DevUI endpoint to /devui
    app.MapDevUI();
}

app.Run();

[Description("Call this tool when the user wants to know the weather.")]
static string GetWeather()
{
    return "The weather is bad!";
}