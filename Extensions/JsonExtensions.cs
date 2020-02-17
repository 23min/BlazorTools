using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace Proliminal.BlazorTools.Extensions
{
    public static class JsonExtensions
    {
        public static string Colorize(this string value)
        {
            var lines = Lines(value);
            return string.Join("", lines.Select(l => l.Decorate()).ToArray());
        }

        public static string Decorate(this string row)
        {
            var sb = new StringBuilder();

            Console.WriteLine(row);

            // split into key/value
            if (row.Contains(":"))
            {
                var kv = row.Split(':');
                var style = (double.TryParse(kv[1], out double _) || int.TryParse(kv[1], out int _)) ? "val" : "str";
                sb.Append(@"<span class=""key"">");
                sb.Append(kv[0]);
                sb.Append(@"</span>");
                sb.Append(@"<span class=""pun"">:</span>");
                sb.Append($"<span class=\"{style}\">");
                sb.Append(string.Join(":", kv.Skip(1)));
                sb.Append(@"</span>");
            }
            else
            {
                sb.Append(@"<span class=""kwd"">");
                sb.Append(row);
                sb.Append(@"</span>");
            }
            sb.Append(@"<span class=""pln"">");
            sb.Append(System.Environment.NewLine);
            sb.Append("</span>");

            return sb.ToString();
        }

        private static IEnumerable<string> Lines(string s)
        {
            using (var tr = new System.IO.StringReader(s))
                while (tr.ReadLine() is string L)
                    yield return L;
        }
    }
}