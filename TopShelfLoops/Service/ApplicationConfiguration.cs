using System;
using Serilog;
using Westwind.Utilities.Configuration;

namespace TopShelfLoops.Service
{
    internal class ApplicationConfiguration : AppConfiguration
    {
        public ApplicationConfiguration()
        {
            ParallelCount = 5;

            TimeToWait = TimeSpan.FromSeconds(8.5);
            CancelAllOnFirstFault = true;
            LogRollingInterval = RollingInterval.Day;
            LogWarn = true;
            LogDebug = true;
            LogNotepad = false;
        }

        public TimeSpan TimeToWait { get; set; }
        public int ParallelCount { get; set; }
        public bool CancelAllOnFirstFault { get; set; }
        public RollingInterval LogRollingInterval { get; set; }
        public bool LogDebug { get; set; }
        public bool LogWarn { get; set; }
        public bool LogNotepad { get; set; }
    }
}