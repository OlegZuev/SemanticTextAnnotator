using System;
using System.IO;
using System.Linq;
using Python.Runtime;

namespace SemanticTextAnnotator {
    class Program {
        static void Main(string[] args) {
            string filepath = "Input.txt";
            if (args.Length > 0) {
                filepath = args[0];
            }

            string text = File.ReadAllText(filepath);
            
            var entitySpans = SemanticTextAnnotator.ParseEntities(text);
            
            string query = DBPediaManager.BuildSPARQLQuery(entitySpans.Select(span => span.normal).Distinct().ToList());
            var dbPediaSPARQLRequestResults = DBPediaManager.SPARQLRequest(query);
            var bindedText = TextManager.BindEntities(text, entitySpans, dbPediaSPARQLRequestResults);
           
            TextManager.SaveAsHTMLDocument(bindedText);
            PythonEngine.Shutdown();
            Console.WriteLine("Finish");
        }
    }
}