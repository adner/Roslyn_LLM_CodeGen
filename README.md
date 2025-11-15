# Roslyn LLM Code Generation

A demonstration of an approach to LLM tool calling that has the potential of reducing context window usage by having the LLM generate and execute C# code instead of using traditional function calling. This was inspired by the [article](https://www.anthropic.com/engineering/code-execution-with-mcp) posted by Anthropic that addresses this topic, as well as the pretty funny [flamewar](https://www.youtube.com/watch?v=1piFEKA9XL0) that followed, where the imminent death of MCP was predicted, among other things.

**The problem:**
- Each tool that the LLM uses requires that a tool schema definition is passed to the LLM, in every call.
- Multiple functions = lots of tokens consumed, potentially blowing up the context window which leads to bad results.
- Potentielly very large tool call resulta are passed between tool calls - also adding to the context.

**The idea:**
- Let the LLM generate code that orchestrates multiple tool calls (from code). 
- This has the benefit of keeping a lot of the tokens out of the context (tool call results, etc).

**The project in this repo:**
- Exposes a single tool: `RunScript(string code)` that allows the LLM to invoke dynamically generated C# code at runtime.
- A simple framework for making custom tools available to be called by the LLM, from the custom code that it generates.

**Result:** Some tokens saved, lots of tool calls saved. 

**The insight:** Probably a lot of tokens (and performance) to be gained, in some circumstances. Is MCP dead? Nah, probably not. :-)

## Project Structure

### ScriptRunnerLib
Core library providing generic C# script execution using Roslyn functionality.

- Generic `ScriptRunner<T>` class that accepts a class that has member methods that can be invoked from dynamically generated code.
- Uses [Microsoft.CodeAnalysis.CSharp.Scripting ](https://www.nuget.org/packages/Microsoft.CodeAnalysis.CSharp.Scripting/)(Roslyn) for runtime compilation
- Configurable assemblies and imports

### Microsoft Agent Framework and DevUI
I wanted to try out the new C# flavor of the [DevUI](https://github.com/microsoft/agent-framework/tree/main/dotnet/src/Microsoft.Agents.AI.DevUI) that is part of the [Microsoft Agent Framework](https://github.com/microsoft/agent-framework). 

## How It Works

### 1. Agent Configuration

The agent is configured with a single tool, `RunScript`, that can be called by the LLM to run C# scripts:

```csharp
var agent = new OpenAIClient(apiKey)
    .GetOpenAIResponseClient("gpt-5.1")
    .CreateAIAgent(
        name: "CodeGenAgent",
        instructions: agentInstructions,
        tools: [AIFunctionFactory.Create(scriptRunner.RunScript)]
    );
```

There are two agents defined that can be used in DevUI:

- **Gpt51_code_gen_agent** - An agent that uses code generation.
- **Gpt51_tool_call_agent** - An agent that uses good old tool calling.
  
To the code-gen agent, we supply instructions that tells it how to do code generation. I created two instructions:

- [agentinstructions.md](https://github.com/adner/Roslyn_LLM_CodeGen/blob/main/DevUI/agentInstructions.md) - Very verbose and details instructions, that yield good results and works most of the time.
- [agentinstructions_2.md](https://github.com/adner/Roslyn_LLM_CodeGen/blob/main/DevUI/agentInstructions_2.md) - An attempt at a minimal, compact instruction that still generates working code (most of the time). This is the instruction used in the video in my [LinkedIn post](https://www.linkedin.com/posts/andreasadner_efficient-llm-tool-calling-using-dynamic-activity-7395423560443498497-Jwu6?utm_source=share&utm_medium=member_desktop&rcm=ACoAAACM8rsBEgQIrYgb4NZAbnxwfDRk_Tu5e3w).

The main entry point is [DevUI/Program.cs](https://github.com/adner/Roslyn_LLM_CodeGen/blob/main/DevUI/Program.cs), so this is where to make changes.

### Code Generation

When a user asks "What's the weather in Uppsala, Stockholm, and Dublin?", (if we are lucky) the agent generates:

```csharp
var results = new List<string>
{
    GetWeather("Uppsala"),
    GetWeather("Stockholm"),
    GetWeather("Dublin")
};
return string.Join("\n", results);
```

The `ScriptRunner` compiles and executes the code using Roslyn, returning the results.

## Building and running

Requires:
- .NET 9.0 SDK
- OpenAI API key

1. Set your OpenAI API key in `appsettings.json`:

```json
{
  "OpenAI": {
    "ApiKey": "your-api-key-here"
  }
}
```
Running the DevUI:

```bash
cd DevUI
dotnet run
```
Then navigate to `https://localhost:57966/devui`

