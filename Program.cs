using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Polyrific.Catapult.Plugins.Core;

namespace MyPlugin
{
    class Program : CodeGeneratorProvider
    {
        public Program(string[] args) : base(args)
        {
        }

        public override string Name => "MyPlugin";

        static async Task Main(string[] args)
        {
            var app = new Program(args);
            var result = await app.Execute();
            app.ReturnOutput(result);
        }

        public override async System.Threading.Tasks.Task<(string outputLocation, Dictionary<string, string> outputValues, string errorMessage)> Generate()
        {
            Config.OutputLocation = Config.OutputLocation ?? Config.WorkingLocation;

            string greeting = null;
            if (AdditionalConfigs != null && AdditionalConfigs.ContainsKey("Greeting") && !string.IsNullOrEmpty(AdditionalConfigs["Greeting"]))
            {
                greeting = AdditionalConfigs["Greeting"];
            }

            // generate code
            await generateConsoleApp(ProjectName, greeting, Config.OutputLocation);

            return (Config.OutputLocation, null, "");
        }

        private async Task generateConsoleApp(string projectName, string greeting, string outputLocation) 
        {
            var info = new ProcessStartInfo("dotnet")
            {
                UseShellExecute = false,
                Arguments = $"new console -n {projectName.Replace(" ", "").Replace("-", "")} -o \"{outputLocation}\"",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(info))
            {
                process.WaitForExit();
            }

            if (!string.IsNullOrEmpty(greeting))
            {
                var programFile = Path.Combine(outputLocation, "Program.cs");
                var content = await File.ReadAllTextAsync(programFile);
                content = content.Replace("Hello World!", greeting);
                await File.WriteAllTextAsync(programFile, content);
            }
        }
    }
}
