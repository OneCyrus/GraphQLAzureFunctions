
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Http;
using GraphQL.Types;

namespace GraphQLAzureFunctions
{
    public static class GraphQL
    {
        [FunctionName("graphql")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            TraceWriter log,
            [Inject(typeof(IDocumentWriter))] IDocumentWriter _writer,
            [Inject(typeof(IDocumentExecuter))] IDocumentExecuter _executer,
            [Inject(typeof(ISchema))]ISchema schema
        )
        {
            var request = Deserialize<GraphQLRequest>(req.Body);

            var result = await _executer.ExecuteAsync(_ =>
            {
                _.Schema = schema;
                _.Query = request.Query;
                _.OperationName = request.OperationName;
                _.Inputs = request.Variables.ToInputs();
            });

            return (ActionResult)new OkObjectResult(_writer.Write(result));
        }

        private static T Deserialize<T>(Stream s)
        {
            using (var reader = new StreamReader(s))
            using (var jsonReader = new JsonTextReader(reader))
            {
                var ser = new JsonSerializer();
                return ser.Deserialize<T>(jsonReader);
            }
        }
    }
}
