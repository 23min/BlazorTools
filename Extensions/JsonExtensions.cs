using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proliminal.BlazorTools.Extensions
{
    public static class JsonExtensions
    {
        private static readonly HashSet<string> KustoKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "let", "project", "extend", "summarize", "where", "join", "on", "union", "parse", "pack", "unpack", "evaluate",
            "make-series", "mv-expand", "top", "limit", "order by", "distinct", "count", "filter", "search", "render",
            "toscalar", "datatable", "print", "range", "bin", "by", "asc", "desc", "coalesce"
        };  

        private static readonly HashSet<string> KustoFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Initialize this only once
            "avg", "count", "dcount", "sum", "min", "max", "stdev", "variance", "make_list", "make_set", "array_length",
            "isempty", "isnotempty", "isnull", "isnotnull", "todatetime", "tostring", "toint", "tolong", "todouble",
            "tobool", "split", "strcat", "replace", "replace_string", "timestamp", "bin", "timeframe", "ago",
            "row_number", "percentile", "percentile_cont", "percentile_disc", "session_window", "hopping_window",
            "sliding_window", "tumbling_window", "todynamic", "totimespan", "toarray", "toscalar", "toobject", "iif", "case"
        };

        private static readonly HashSet<string> KustoOperators = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "=", ">", "<", ">=", "<=", "==", "!=", "+", "-", "*", "/", "%", "|", "&&", "||", "!", "~", "&", "|", "^", "<<", ">>"
        };

        public static string ColorizeJson(this string value)
        {
            var lines = Lines(value);
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                sb.Append(line.DecorateJson());
                sb.Append("<br>");
            }
            return sb.ToString();
        }
        public static (bool isPlainText, string colorizedText) ColorizeCode(this string text)
        {
            if (!IsLikelyKusto(text))
            {
                return (true, text);
            }

            var lines = Lines(text);
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                var colorizedLine = ColorizeKustoLine(line);

                sb.Append(colorizedLine);
                sb.Append("<br>");
            }

            // Return the fully colorized query with preserved line breaks
            return (false, sb.ToString());
        }

        private static string DecorateJson(this string row)
        {
            var sb = new StringBuilder();

            // Determine the number of leading spaces for indentation
            int leadingSpaces = row.TakeWhile(Char.IsWhiteSpace).Count();
            string indentation = new String(' ', leadingSpaces);

            // Trim leading whitespace from the row and check if it's likely a key or standalone string
            string trimmedRow = row.TrimStart();
            if (trimmedRow.StartsWith("\"")) // Likely a string property or value.
            {
                // Split on the first colon which separates the key and value
                int colonIndex = trimmedRow.IndexOf(":");
                if (colonIndex > -1)
                {
                    var key = trimmedRow.Substring(0, colonIndex + 1); // Include the colon and following space
                    var value = trimmedRow.Substring(colonIndex + 1).TrimStart(); // Trim leading whitespace after colon

                    // Append indentation, then the key span
                    sb.Append(indentation);
                    sb.Append(@"<span class=""key"">");
                    sb.Append(key.TrimEnd()); // Trim any trailing whitespace from the key
                    sb.Append(@"</span> "); // Add a space after the colon for better readability

                    if (value.StartsWith("\""))
                    {
                        // Value is a string
                        sb.Append(@"<span class=""str"">");
                        sb.Append(value); // String value
                        sb.Append(@"</span>");
                    }
                    else
                    {
                        // Value is non-string (number, boolean, null)
                        sb.Append(@"<span class=""num"">");
                        sb.Append(value); // Non-string value
                        sb.Append(@"</span>");
                    }
                }
                else
                {
                    // It's a standalone string outside of a key-value pair
                    sb.Append(indentation);
                    sb.Append(@"<span class=""str"">");
                    sb.Append(trimmedRow);
                    sb.Append(@"</span>");
                }
            }
            else
            {
                // For handling other JSON structures (like objects or arrays)
                sb.Append(indentation);
                sb.Append(trimmedRow); // No need for a span as it's structural
            }

            return sb.ToString();
        }

        private static IEnumerable<string> Lines(string s)
        {
            using (var tr = new System.IO.StringReader(s))
                while (tr.ReadLine() is string L)
                    yield return L;
        }

        private static string ColorizeKustoLine(string line)
        {
            var sb = new StringBuilder();
            var token = new StringBuilder();
            char? stringDelimiter = null; // Used to remember the type of quote that opened the string

            // Helper function to add non-empty tokens to the StringBuilder
            void AddToken()
            {
                if (token.Length == 0) return;

                var strToken = token.ToString();
                if (stringDelimiter != null)
                {
                    // Colorize the string literal
                    sb.Append($"<span class=\"str\">{strToken}</span>");
                    stringDelimiter = null; // Reset the delimiter after processing the string
                }
                else if (IsKustoKeyword(strToken.Trim()))
                {
                    // Trim the token for keyword matching
                    sb.Append($"<span class=\"kwd\">{strToken.Trim()}</span>");
                }
                else if (IsKustoOperator(strToken.Trim()))
                {
                    sb.Append($"<span class=\"opr\">{strToken.Trim()}</span>");
                }
                else if (IsKustoFunction(strToken.Trim()))
                {
                    sb.Append($"<span class=\"func\">{strToken.Trim()}</span>");
                }
                else if (double.TryParse(strToken.Trim(), out _) || int.TryParse(strToken.Trim(), out _))
                {
                    sb.Append($"<span class=\"num\">{strToken.Trim()}</span>");
                }
                else
                {
                    sb.Append(strToken); // Just append the token without span
                }
                token.Clear();
            }

            foreach (char ch in line)
            {
                if ((ch == '\"' || ch == '\'') && stringDelimiter == null)
                {
                    AddToken(); // Add the previous token if it's not part of the string
                    stringDelimiter = ch; // Set the string delimiter to the current quote type
                    token.Append(ch);
                }
                else if (ch == stringDelimiter)
                {
                    token.Append(ch);
                    AddToken(); // Add the string token
                }
                else if (stringDelimiter != null)
                {
                    token.Append(ch); // Continue adding to the string token
                }
                else if (char.IsWhiteSpace(ch) || "(),=!".Contains(ch))
                {
                    AddToken();
                    if (!char.IsWhiteSpace(ch))
                    {
                        sb.Append($"<span class=\"pun\">{ch}</span>");
                    }
                    else
                    {
                        sb.Append(ch); // Append whitespace directly for formatting
                    }
                }
                else
                {
                    token.Append(ch); // If it's not whitespace, punctuation, or a string, it's part of the current token
                }
            }

            AddToken(); // Add the last token if any

            return sb.ToString();
        }

        private static bool IsKustoKeyword(string token)
        {
            return KustoKeywords.Contains(token);
        }

        private static bool IsKustoOperator(string token)
        {
            return KustoOperators.Contains(token);
        }

        private static bool IsKustoFunction(string token)
        {
            return KustoFunctions.Contains(token);
        }

        private static bool IsLikelyKusto(string input)
        {
            // Normalize the input to ignore case
            var normalizedInput = input.ToLowerInvariant();

            // List of KQL-specific keywords (extend this list based on your needs)
            var kqlKeywords = new HashSet<string> { "datatable", "summarize", "project", "extend", "where", "join", "union", "on" };
            var kqlOperators = new HashSet<string> { "|" }; // KQL-specific operator

            // Check for presence of KQL-specific keywords
            bool containsKqlKeywords = kqlKeywords.Any(keyword => normalizedInput.Contains(keyword));

            // Check for presence of KQL-specific operators
            bool containsKqlOperators = kqlOperators.Any(op => normalizedInput.Contains(op));

            // Heuristic based on starting patterns (e.g., "let", table names)
            bool likelyStartsWithKqlCommand = normalizedInput.TrimStart().StartsWith("let ");

            return containsKqlKeywords || containsKqlOperators || likelyStartsWithKqlCommand;
        }

        public static string TransformJsonForHtml(string colorizedJsonString)
        {
            return colorizedJsonString
                .Replace("\\r\\n", "\n")
                .Replace("\\r", "")
                .Replace("\\n", "\n")
                .Replace("\\\"", "\"");
        }

        public static string TransformKustoForHtml(string colorizedKustoString)
        {
            // Since the Kusto string is already HTML, we should only replace newline characters with <br> tags
            // and avoid altering quotes or other characters that are part of the HTML syntax.
            return colorizedKustoString.Replace("\n", "<br>");
        }

        public  static string TransformForHtmlPlainText(string text)
        {
            // Adjust text transformation for HTML display as needed.
            // For plain text, you might only need to replace certain characters for HTML display.
            // This is a placeholder for whatever HTML transformation you deem necessary.
            return text
                .Replace("\n", "<br>") // Example transformation.
                .Replace("\"", "&quot;");
        }
    }
}