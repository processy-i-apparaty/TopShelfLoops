using System;
using System.Threading;
using TopShelfLoops.ParallelJob;

namespace TopShelfLoops.Logic
{
    internal class SampleParallelJob : ParallelJobBase<int, string>
    {
        public SampleParallelJob(ulong id, Func<int, CancellationToken, CancellationToken, string> job) : base(id, job)
        {
        }
    }
}