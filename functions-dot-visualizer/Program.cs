using Microsoft.Azure.WebJobs.Script.Description;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionsDotVisualizer
{
    class Program
    {
        const string FunctionNodeStyle  = "[shape=note, fillcolor=\"beige\"]";
        const string TriggerNodeStyle   = "[fillcolor=\"/bugn3/1\"]";
        const string InputNodeStyle     = "[fillcolor=\"/bugn3/2\"]";
        const string OutputNodeStyle    = "[fillcolor=\"/bugn3/3\"]";
        const string TriggerArrow       = "[arrowhead = vee, label=\"   Trigger\"]";
        const string InputArrow         = "[arrowhead = dot, label=\"   Input\"]";
        const string OutputArrow        = "[arrowhead = box, label=\"   Output\"]";

        const string Preamble = "digraph Functions {\n   graph [fontname = \"Segoe UI\"];\n   node[fontname = \"Segoe UI\", shape = box, style = filled];\n   edge[fontname = \"Segoe UI\", fontsize = 10];";

        static void Main(string[] args)
        {
            var outputLines = new List<string>();

            var files = Directory.GetFiles(args[0], "function.json", SearchOption.AllDirectories);
            
            foreach (var f in files) {
                var functionName = new DirectoryInfo(f).Parent.Name;
                var output = ProcessFunction(functionName, f);
                outputLines.AddRange(output);
            }

            using (var file = new StreamWriter(@"output.dot")) {
                file.WriteLine(Preamble);

                foreach (string line in outputLines) {
                    file.WriteLine($"   {line}");
                }

                file.WriteLine("}");
            }
        }

        // TODO: make the code work across all functions globally. Need to compare input and output config to make sure that
        // they are actually the same node. For instance, check that queue output and input are the same queue

        private static List<string> ProcessFunction(string functionName, string filename)
        {
            var outputLines = new List<string>();

            var metadata = ParseFunctionJson(functionName, filename);

            functionName = $"\"{functionName}\""; // surround with quotes
            outputLines.Add($"{functionName} {FunctionNodeStyle}");

            foreach (var binding in metadata.Bindings) {

                var nodeText = $"\"{binding.Type} - {binding.Name}\"";

                if (binding.IsTrigger) {
                    outputLines.Add($"{nodeText} {TriggerNodeStyle}");
                    outputLines.Add($"{nodeText} -> {functionName} {TriggerArrow}");
                }
                else if (binding.Direction == BindingDirection.Out) {
                    outputLines.Add($"{nodeText} {OutputNodeStyle}");
                    outputLines.Add($"{functionName} -> {nodeText} {OutputArrow}");
                }
                else {
                    outputLines.Add($"{nodeText} {InputNodeStyle}");
                    outputLines.Add($"{nodeText} -> {functionName} {InputArrow}");
                }
            }

            return outputLines;
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
