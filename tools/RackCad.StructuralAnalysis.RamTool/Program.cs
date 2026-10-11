using System;

namespace RackCad.StructuralAnalysis.RamTool
{
    /// <summary>
    /// Entry point of the console tool. All the work is in <see cref="RamToolCommands"/>, which the tests call with their own writers;
    /// <c>Main</c> only connects it to the console.
    /// </summary>
    public static class Program
    {
        public static int Main(string[] args) => RamToolCommands.Run(args, Console.Out, Console.Error);
    }
}
