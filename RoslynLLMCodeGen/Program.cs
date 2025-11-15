using OpenAI;
using Microsoft.Extensions.Configuration;
using ScriptRunnerLib;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory()+"/RoslynLLMCodeGen")
    .AddJsonFile("appsettings.local.json", optional: false, reloadOnChange: true)
    .Build();

string apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key not found in configuration.");

// Load agent instructions from file
string agentInstructions = await File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory()+"/RoslynLLMCodeGen", "agentInstructions.md"));

var scriptRunner = new ScriptRunner<ScriptGlobals>(new ScriptGlobals());

var agent = new OpenAIClient(apiKey)
.GetOpenAIResponseClient("gpt-5.1")
.CreateAIAgent(name: "CodeGenAgent", instructions: agentInstructions, tools: [AIFunctionFactory.Create(scriptRunner.RunScript)]);

var result = await agent.RunAsync("What is the weather like in Uppsala, Stockholm and Dublin?");

Console.WriteLine(result);

public class ScriptGlobals
{
    // Safe operations you explicitly allow the script to perform
    public string GetWeather(string location)
    {
        // Call your own services/repositories here
        return $"The weather in {location} is 24 degress and sunny.";
    }
}

