using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SemanticTextAnnotator {
    public class TextManager {
        public static void ReadText() {

        }

        public static void SaveAsHTMLDocument(string body) {
            string htmlHeader = 
                @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Document</title>
    <link rel=""stylesheet"" href=""styles/index.css"" />
    <script src=""scripts/script.js""></script>
</head>";
            string htmlBody = @"
<body>";
            string[] paragraphs = body.Split("\r\n");
            var htmlDivs = new StringBuilder();
            foreach (var paragraph in paragraphs) {
                htmlDivs.Append(@"
    <div class=""text"">
        " + paragraph + @"
    </div>");
            }

            string htmlFooter = @"
</body>
</html>";
            if (!Directory.Exists("Document/")) {
                Directory.CreateDirectory("Document/");
            }
            File.WriteAllText("Document/index.html", htmlHeader + htmlBody + htmlDivs + htmlFooter);
        }

        public static string BindEntities(string text, List<NamedEntity> entitySpans, List<DBPediaSearchRequestJSON> dbPediaResults) {
            var sb = new StringBuilder();
            int currentPos = 0;
            for (int i = 0; i < entitySpans.Count; i++) {
                sb.Append(text.Substring(currentPos, entitySpans[i].start - currentPos));
                sb.Append($"<a href=\"{dbPediaResults[i].docs[0].resource[0]}\" class=\"entity\" --tooltip=\"{dbPediaResults[i].docs[0].comment[0]}\">{entitySpans[i].text}</a>");
                currentPos = entitySpans[i].stop;
            }

            return sb.ToString();
        }

        public static string BindEntities(string text, List<NamedEntity> entitySpans, Dictionary<string, DBPediaSearchRequestRDF> dbPediaResults) {
            var sb = new StringBuilder();
            int currentPos = 0;
            for (int i = 0; i < entitySpans.Count; i++) {
                sb.Append(text.Substring(currentPos, entitySpans[i].start - currentPos));
                if (dbPediaResults.ContainsKey(entitySpans[i].normal)) {
                    sb.Append($"<a href=\"{dbPediaResults[entitySpans[i].normal].URI}\" class=\"entity\" --tooltip=\"{dbPediaResults[entitySpans[i].normal].Description.Truncate(200)}\">{entitySpans[i].text}</a>");
                } else {
                    sb.Append(entitySpans[i].text);
                }
                currentPos = entitySpans[i].stop;
            }

            sb.Append(text[currentPos..]);
            return sb.ToString();
        }
    }
}