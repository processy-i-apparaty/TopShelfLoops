using System;
using Topshelf;
using TopShelfLoops.Logic;
using TopShelfLoops.Service;

namespace TopShelfLoops
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var customLogic = new CustomLogicSample();

            var configuration = new ApplicationConfiguration();
            configuration.Initialize();
            configuration.Write();

            TopshelfExitCode rc = CustomService.RunService(
                "CustomService69",
                "CustomService69",
                "CustomService69",
                customLogic, configuration);

            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
            Environment.ExitCode = exitCode;
        }
    }
}