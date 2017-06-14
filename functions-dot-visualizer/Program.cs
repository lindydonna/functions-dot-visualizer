using Microsoft.Azure.WebJobs.Script.Description;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVisualizerLib;

namespace FunctionsDotVisualizer
{
    class Program
    {
        static void Main(string[] args)
        {
            Visualizer.DotFileFromFunctionDirectory(args[0], args[1]);
        }
    }
}
