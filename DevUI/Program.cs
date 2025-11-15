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

var scriptRunner = new ScriptRunner<ScriptGlobals>(new ScriptGlobals());

builder.Services.AddChatClient(chatClient.AsIChatClient());

string agentInstructions = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "agentInstructions_2.md"));
// Register your agents
builder.AddAIAgent("Gpt51_code_gen_agent", agentInstructions).WithAITool(AIFunctionFactory.Create(scriptRunner.RunScript));
builder.AddAIAgent("Gpt51_tool_call_agent", "You are a helpful agent that can get the weather at a location by calling the GetWeather tool. Always respond in the format: Location, Temperature, Weather type (one word).").WithAITool(AIFunctionFactory.Create(new ScriptGlobals().GetWeather));

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

public enum WeatherCondition
{
    Sunny,
    Cloudy,
    Rainy,
    Snowy,
    Foggy,
    Windy,
    Stormy
}

public class ScriptGlobals
{
    private static readonly Random _random = new Random();

    [Description("Gets the weather in a specific location.")]
    public string GetWeather(string location)
    {
        // Call your own services/repositories here
        int temperature = _random.Next(-10, 31); // -10 to 30 inclusive
        var weatherConditions = Enum.GetValues<WeatherCondition>();
        var randomWeather = weatherConditions[_random.Next(weatherConditions.Length)];

        return $"The weather in {location} is {temperature} degrees and {randomWeather.ToString().ToLower()}.";
    }
}
