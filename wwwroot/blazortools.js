// This file is to show how a library package may provide JavaScript interop features
// wrapped in a .NET API

window.BlazorTools = {
  showPrompt: function (message) {
    return prompt(message, 'Type anything here');
  },

  FormatJson: function (value) {
    return JSON.stringify(JSON.parse(value), null, 3)
  }
};

