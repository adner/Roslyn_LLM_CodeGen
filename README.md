# Roslyn LLM Code Generation

An innovative approach to LLM tool calling that significantly reduces context window usage by having the LLM generate and execute C# code instead of using traditional function calling schemas.

## Project Purpose

This project explores how custom code generation can be used to achieve more efficient LLM tool calls and reduce context window usage. Instead of providing detailed function schemas to the LLM (which consume loads of tokens per tool), the agent is instructed to write C# code that directly calls available methods.

## Key Innovation

**Traditional Approach Problem:**
- Each function requires extensive schema definition (name, parameters, descriptions, types, examples)
- Multiple functions = hundreds or thousands of tokens consumed
- Context window fills up quickly with tool definitions

**This Project's Solution:**
- Expose a single tool: `RunScript(string code)`
- Provide brief instructions mentioning available methods (e.g., `GetWeather(string location)`)
- LLM generates complete C# code to accomplish tasks
- Roslyn compiles and executes the code with access to pre-approved methods

**Result:** Substantial token savings while maintaining flexibility and type safety.

## Project Structure

### ScriptRunnerLib
Core library providing generic C# script execution using Roslyn.

**Key Features:**
- Generic `ScriptRunner<T>` class that accepts a globals object
- Uses Microsoft.CodeAnalysis.CSharp.Scripting (Roslyn) for runtime compilation
- Safe execution with error handling and compilation diagnostics
- Configurable assemblies and imports

**Location:** [ScriptRunnerLib/ScriptRunnerLib.cs](ScriptRunnerLib/ScriptRunnerLib.cs)

### RoslynLLMCodeGen
Console application demonstrating the code generation approach.

**Features:**
- Simple command-line interface
- Integration with OpenAI GPT-5.1
- Single `RunScript` tool exposed to the agent
- Comprehensive agent instructions for code generation

**Location:** [RoslynLLMCodeGen/Program.cs](RoslynLLMCodeGen/Program.cs)

### DevUI
Web-based development UI for testing and comparing agents.

**Features:**
- ASP.NET Core web application
- Interactive UI at `https://localhost:57966/devui`
- Two agent configurations for comparison:
  - Code generation agent (ultra-efficient)
  - Traditional tool call agent (baseline)

**Location:** [DevUI/Program.cs](DevUI/Program.cs)

## How It Works

### 1. Agent Configuration

The agent is configured with a single tool:

```csharp
var agent = new OpenAIClient(apiKey)
    .GetOpenAIResponseClient("gpt-5.1")
    .CreateAIAgent(
        name: "CodeGenAgent",
        instructions: agentInstructions,
        tools: [AIFunctionFactory.Create(scriptRunner.RunScript)]
    );
```

### 2. Minimal Instructions

Instead of verbose function schemas, provide concise instructions:

```markdown
You are a C# Code Execution Agent. When a user request can be solved with code,
generate a complete C# script and execute it with the RunScript tool.

The host environment already provides a global function:
    string GetWeather(string location)
You must ONLY CALL GetWeather; NEVER declare, define, or wrap it.
```

### 3. Code Generation

When a user asks "What's the weather in Uppsala, Stockholm, and Dublin?", the agent generates:

```csharp
var results = new List<string>
{
    GetWeather("Uppsala"),
    GetWeather("Stockholm"),
    GetWeather("Dublin")
};
return string.Join("\n", results);
```

### 4. Execution

The `ScriptRunner` compiles and executes the code using Roslyn, returning the results.

## Security Model

The project uses a "whitelist" security model through the `ScriptGlobals` pattern:

```csharp
public class ScriptGlobals
{
    public string GetWeather(string location)
    {
        // Only safe, pre-approved operations
        return $"Weather data for {location}";
    }
}
```

**Safety Features:**
- Scripts run in isolated Roslyn environment
- Only methods explicitly exposed in ScriptGlobals are accessible
- No file system, network, or OS access by default
- Compilation errors caught and returned as messages
- Runtime exceptions handled gracefully

## Technologies

- **.NET 9.0** - Latest .NET framework
- **Roslyn** (Microsoft.CodeAnalysis.CSharp.Scripting) - C# compiler as a service
- **Microsoft.Agents.AI** - Agent framework (preview)
- **OpenAI GPT-5.1** - Language model
- **ASP.NET Core** - Web hosting for DevUI

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- OpenAI API key

### Configuration

1. Set your OpenAI API key in `appsettings.json`:

```json
{
  "OpenAI": {
    "ApiKey": "your-api-key-here"
  }
}
```

### Running the Console App

```bash
cd RoslynLLMCodeGen
dotnet run
```

### Running the DevUI

```bash
cd DevUI
dotnet run
```

Then navigate to `https://localhost:57966/devui`

## Project Benefits

1. **Massive Token Savings** - Single tool vs. many function schemas
2. **Flexibility** - LLM can combine operations in complex ways
3. **Type Safety** - Roslyn compilation catches errors at runtime
4. **Extensibility** - Easy to add new capabilities via ScriptGlobals
5. **Transparency** - Generated code is visible and debuggable
6. **Reduced Latency** - Fewer tokens = faster API calls

## Key Files

| File | Description |
|------|-------------|
| [ScriptRunnerLib/ScriptRunnerLib.cs](ScriptRunnerLib/ScriptRunnerLib.cs) | Core script execution engine |
| [RoslynLLMCodeGen/Program.cs](RoslynLLMCodeGen/Program.cs) | Console application |
| [RoslynLLMCodeGen/agentInstructions.md](RoslynLLMCodeGen/agentInstructions.md) | Comprehensive agent instructions |
| [DevUI/Program.cs](DevUI/Program.cs) | Web UI application |
| [DevUI/agentInstructions_2.md](DevUI/agentInstructions_2.md) | Ultra-concise optimized instructions |

## Future Extensibility

The architecture allows easy expansion:
- Add more methods to `ScriptGlobals`
- Include additional assemblies in `ScriptOptions`
- Add custom imports for domain-specific libraries
- Implement streaming results
- Add code sandboxing and resource limits

## Example Usage

**User:** "What is the weather in Uppsala, Stockholm and Dublin?"

**Agent generates:**
```csharp
var results = new List<string>
{
    GetWeather("Uppsala"),
    GetWeather("Stockholm"),
    GetWeather("Dublin")
};
return string.Join("\n", results);
```

**Output:**
```
Weather data for Uppsala
Weather data for Stockholm
Weather data for Dublin
```

## License

This project is for research and educational purposes, exploring efficient LLM tool calling patterns.

## Contributing

This is an experimental project demonstrating a novel approach to LLM agent architectures. Feel free to explore and adapt the concepts to your own use cases.
