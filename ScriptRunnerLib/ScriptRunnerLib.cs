
using System.Runtime;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;

namespace ScriptRunnerLib;

public class ScriptRunner<T>
{
private readonly ScriptOptions  _options;
private readonly T _globals;

    public ScriptRunner(T globals)
    {
        _options = ScriptOptions.Default
            .AddReferences(
                typeof(object).Assembly,           // System.Private.CoreLib
                typeof(Enumerable).Assembly        // System.Linq
                // Add your domain assemblies here
            )
            .AddImports(
                "System",
                "System.Linq",
                "System.Threading.Tasks",
                "System.Collections.Generic"
            );

            _globals = globals;
    }

    public async Task<string> RunScript(string code)
    {
        try
        {
            var result = runInternal(code).Result;

            string output = result switch
            {
                string s => s,
                int i => i.ToString(),
                bool b => b.ToString(),
                null => "null",
                System.Collections.IEnumerable enumerable when result is not string =>
                    string.Join(", ", enumerable.Cast<object>().Select(x => x?.ToString() ?? "null")),
                _ => result.ToString() ?? "No result"
            };

            return output;
        }
        catch (Exception err)
        {
            return err.Message;
        }
    }

    private async Task<object?> runInternal(string code)
    {
        try
        {
            var result = await CSharpScript.EvaluateAsync(
                code,
                _options,
                globals: _globals,
                globalsType: typeof(T)
            );

            return result;
        }
        catch (CompilationErrorException ex)
        {
            // Compiler diagnostics: send back to user/telemetry
            var diagnostics = string.Join(Environment.NewLine, ex.Diagnostics);
            throw new InvalidOperationException($"Compilation failed:\n{diagnostics}", ex);
        }
    }
}

