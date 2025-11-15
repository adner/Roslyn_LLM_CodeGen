using OpenAI;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.local.json", optional: false, reloadOnChange: true)
    .Build();

string apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key not found in configuration.");

var agent = new OpenAIClient(apiKey)
    .GetOpenAIResponseClient("gpt-5.1")
    .CreateAIAgent(name: "HaikuBot", instructions: "You are an upbeat assistant that writes beautifully.");

Console.WriteLine(await agent.RunAsync("Write a haiku about Microsoft Agent Framework."));
