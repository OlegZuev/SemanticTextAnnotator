using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using VDS.RDF.Query;

namespace SemanticTextAnnotator {
    public class DBPediaManager {
        private static string API_URL = "https://lookup.dbpedia.org/api";

        public static DBPediaSearchRequestJSON SearchEntity(string entityName) {
            var rc = new RestClient(API_URL) {Timeout = -1};
            rc.UseNewtonsoftJson();
            var request = new RestRequest($"search?", Method.GET, DataFormat.Json);
            request.AddParameter("format", "JSON");
            request.AddParameter("query", entityName);

            var restResponse = rc.Execute<DBPediaSearchRequestJSON>(request);
            ResponseStatus responseStatus = restResponse.ResponseStatus;
            return responseStatus switch {
                ResponseStatus.Completed => restResponse.Data,
                ResponseStatus.Error => throw new Exception(restResponse.ErrorMessage),
                ResponseStatus.TimedOut => throw new Exception("TimedOut"),
                ResponseStatus.Aborted => throw new Exception("Aborted"),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static Task<IRestResponse<DBPediaSearchRequestJSON>> SearchEntityTask(string entityName) {
            var rc = new RestClient(API_URL) {Timeout = -1};
            rc.UseNewtonsoftJson();
            var request = new RestRequest($"search?", Method.GET, DataFormat.Json);
            request.AddParameter("format", "JSON_RAW");
            request.AddParameter("query", entityName);
            request.AddParameter("maxResults", 2);

            return rc.ExecuteAsync<DBPediaSearchRequestJSON>(request);
        }


        public static async Task<List<DBPediaSearchRequestJSON>> SearchAllEntitiesAsync(List<string> entityNames) {
            var tasks = entityNames.Select(SearchEntityTask).ToList();

            await Task.WhenAll(tasks);

            return tasks.Select(task => task.Result.Data).ToList();
        }

        public static string BuildSPARQLQuery(List<string> entityNames) {
            var sb = new StringBuilder();
            sb.Append("SELECT DISTINCT ?URI ?label ?description WHERE { ");
            bool firstFlag = true;
            foreach (var name in entityNames) {
                if (!firstFlag) {
                    sb.Append(" UNION ");
                } else {
                    firstFlag = false;
                }

                sb.Append(
                    $"{{ ?URI rdfs:label \"{name}\"@ru ; rdfs:label ?label ; dbo:abstract ?description . FILTER (lang(?description ) = 'ru') FILTER (lang(?label ) = 'ru') }}");
            }

            sb.Append(" } ");

            return sb.ToString();
        }

        public static Dictionary<string, DBPediaSearchRequestRDF> SPARQLRequest(string query) {
            //Define a remote endpoint
            //Use the DBPedia SPARQL endpoint with the default Graph set to DBPedia
            SparqlRemoteEndpoint endpoint =
                new SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql"), "http://dbpedia.org") {
                    HttpMode = "GET"
                };

            //Make a SELECT query against the Endpoint
            SparqlResultSet results = endpoint.QueryWithResultSet(query);
            var dic = new Dictionary<string, DBPediaSearchRequestRDF>();

            foreach (SparqlResult result in results) {
                string key = result["label"].ToString()[..^3];
                if (!dic.ContainsKey(key)) {
                    dic[key] = new DBPediaSearchRequestRDF {
                        URI = result["URI"].ToString(),
                        Description = result["description"].ToString(),
                    };
                }
            }

            return dic;
        }
    }
}