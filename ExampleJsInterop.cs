using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Proliminal.BlazorTools.Interop
{
    public class ExampleJsInterop
    {
        public static ValueTask<string> Prompt(IJSRuntime jsRuntime, string message)
        {
            // Implemented in exampleJsInterop.js
            return jsRuntime.InvokeAsync<string>(
                "exampleJsFunctions.showPrompt",
                message);
        }
    }

    public static class Util
    {
        public static async Task<string> FormatJson(IJSRuntime _JSRuntime, string json)
        {
            return await _JSRuntime.InvokeAsync<string>("exampleJsFunctions.FormatJson", json);
        }
    }
}
