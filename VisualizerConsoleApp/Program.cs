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