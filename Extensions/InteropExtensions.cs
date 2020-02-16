using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Proliminal.BlazorTools.Extensions
{
    public static class InteropExtensions
    {
        public static async Task<string> FormatJson(this IJSRuntime _JSRuntime, string json)
        {
            return await _JSRuntime.InvokeAsync<string>("exampleJsFunctions.FormatJson", json);
        }
    }
}