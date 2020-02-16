using System.Text;

namespace Proliminal.BlazorTools.Extensions
{
    public static class PrettyMaker
    {
        public static string Decorate(this string row)
        {
            var sb = new StringBuilder();

            // split into key/value
            if (row.Contains(":"))
            {
                var kv = row.Split(':');
                var style = (double.TryParse(kv[1], out double _) || int.TryParse(kv[1], out int _)) ? "val": "str";
                sb.Append(@"<span class=""key"">");
                sb.Append(kv[0]);
                sb.Append(@"</span>");
                sb.Append(@"<span class=""pun"">:</span>");
                sb.Append($"<span class=\"{style}\">");
                sb.Append(kv[1]);
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
    }
}