// This file is to show how a library package may provide JavaScript interop features
// wrapped in a .NET API

window.BlazorTools = {
    showPrompt: function(message) {
        return prompt(message, "Type anything here");
    },

    FormatJson: function(value) {
        try {
            // Parse and then stringify with 3-space indentation
            return JSON.stringify(JSON.parse(value), null, 3);
        } catch (e) {
            console.error("Error parsing JSON for formatting", e);
            throw e; // or handle the error appropriately
        }
    }
}