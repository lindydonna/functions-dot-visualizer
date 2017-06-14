using Microsoft.Azure.WebJobs.Script.Description;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVisualizerLib
{
    public class Visualizer
    {
        public const string FunctionNodeStyle = "[shape=note, fillcolor=\"beige\"]";
        public const string TriggerNodeStyle = "[fillcolor=\"/bugn3/1\"]";
        public const string InputNodeStyle = "[fillcolor=\"/bugn3/2\"]";
        public const string OutputNodeStyle = "[fillcolor=\"/bugn3/3\"]";
        public const string TriggerArrow = "[arrowhead = vee, label=\"   Trigger\"]";
        public const string InputArrow = "[arrowhead = dot, label=\"   Input\"]";
        public const string OutputArrow = "[arrowhead = box, label=\"   Output\"]";

        public const string Preamble = "digraph Functions {\n   graph [fontname = \"Segoe UI\"];\n   node[fontname = \"Segoe UI\", shape = box, style = filled];\n   edge[fontname = \"Segoe UI\", fontsize = 10];";

        public static void DotFileFromFunctionDirectory(string directory, string outputFilename)
        {
            var outputLines = new List<string>();

            var files = Directory.GetFiles(directory, "function.json", SearchOption.AllDirectories);

            foreach (var f in files) {
                var functionName = new DirectoryInfo(f).Parent.Name;
                var output = ProcessFunction(functionName, f);
                outputLines.AddRange(output);
            }

            using (var file = new StreamWriter(outputFilename)) {
                file.WriteLine(Preamble);

                foreach (string line in outputLines) {
                    file.WriteLine($"   {line}");
                }

                file.WriteLine("}");
            }
        }

        public static List<string> ProcessFunction(string functionName, string filename)
        {
            var outputLines = new List<string>();

            var metadata = ParseFunctionJson(functionName, filename);

            outputLines.Add($"\"{functionName}\" {FunctionNodeStyle}");

            foreach (var binding in metadata.Bindings) {

                var (nodeLabel, nodeIdentifier) = GenerateBindingIdentifier(functionName, binding);
                nodeIdentifier = '"' + nodeIdentifier + '"'; // surround with quotes
                nodeLabel = $"[label = \"{nodeLabel}\"]";

                if (binding.IsTrigger) {
                    outputLines.Add($"{nodeIdentifier} {nodeLabel} {TriggerNodeStyle}");
                    outputLines.Add($"{nodeIdentifier} -> \"{functionName}\" {TriggerArrow}");
                }
                else if (binding.Direction == BindingDirection.Out) {
                    outputLines.Add($"{nodeIdentifier} {nodeLabel} {OutputNodeStyle}");
                    outputLines.Add($"\"{functionName}\" -> {nodeIdentifier} {OutputArrow}");
                }
                else {
                    outputLines.Add($"{nodeIdentifier} {nodeLabel} {InputNodeStyle}");
                    outputLines.Add($"{nodeIdentifier} -> \"{functionName}\" {InputArrow}");
                }
            }

            return outputLines;
        }

        public static (string, string) GenerateBindingIdentifier(string functionName, BindingMetadata binding)
        {
            // common parameters
            var queueName = binding.Raw["queueName"];
            var connection = binding.Raw["connection"] ?? "AzureWebJobsStorage";
            var path = binding.Raw["path"];
            var topicName = binding.Raw["topicName"];
            var subscriptionName = binding.Raw["subscriptionName"];

            var defaultId = $"{binding.Type} - {functionName}";

            switch (binding.Type) {
                case "queueTrigger":
                case "queue":
                    return ("Queue", $"Queue - {queueName} - {connection}");

                case "httpTrigger":
                case "http":
                    return ("HTTP", defaultId);
                case "blobTrigger":
                case "blob":
                    return ("Blob", $"Blob - {path} - {connection}");

                case "serviceBusTrigger":
                case "serviceBus":
                    return ("Service Bus", $"ServiceBus - {queueName} - {topicName} - {subscriptionName} - {connection}");

                case "timerTrigger":
                    var schedule = binding.Raw["schedule"];
                    return ($"Timer\n{schedule}", defaultId);

                case "eventHubTrigger":
                case "eventHub":
                    return ("Event Hub", $"EventHub - {path} - {connection}");
                case "documentDB":
                    var collectionName = binding.Raw["collectionName"];
                    var databaseName = binding.Raw["databaseName"];
                    return ("DocumentDB", $"DocumentDB - {databaseName} - {collectionName} - {connection}");
                case "manualTrigger":
                    return ("Manual", defaultId);
                case "table":
                    var tableName = binding.Raw["tableName"];
                    return ("Table", $"Table - {tableName} - {connection}");
                default:
                    return (binding.Type, defaultId);
            }
        }

        private static FunctionMetadata ParseFunctionJson(string functionName, string filename)
        {
            var functionJson = JObject.Parse(File.ReadAllText(filename));
            return ParseFunctionMetadata(functionName, functionJson, ".");
        }

        private static FunctionMetadata ParseFunctionMetadata(string functionName, JObject configMetadata, string scriptDirectory)
        {
            FunctionMetadata functionMetadata = new FunctionMetadata {
                Name = functionName,
                FunctionDirectory = scriptDirectory
            };

            JValue triggerDisabledValue = null;
            JArray bindingArray = (JArray)configMetadata["bindings"];
            if (bindingArray == null || bindingArray.Count == 0) {
                throw new FormatException("At least one binding must be declared.");
            }

            if (bindingArray != null) {
                foreach (JObject binding in bindingArray) {
                    BindingMetadata bindingMetadata = BindingMetadata.Create(binding);
                    functionMetadata.Bindings.Add(bindingMetadata);
                    if (bindingMetadata.IsTrigger) {
                        triggerDisabledValue = (JValue)binding["disabled"];
                    }
                }
            }

            JToken value = null;
            if (configMetadata.TryGetValue("excluded", StringComparison.OrdinalIgnoreCase, out value) &&
                value.Type == JTokenType.Boolean) {
                functionMetadata.IsExcluded = (bool)value;
            }

            return functionMetadata;
        }
    }
}
