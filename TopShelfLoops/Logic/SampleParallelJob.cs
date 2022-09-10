using System;
using System.Threading;
using TopShelfLoops.Paralleljob;

namespace TopShelfLoops.Logic
{
    internal class SampleParallelJob : ParallelJobBase<int, string>
    {
        public SampleParallelJob(ulong id, Func<int, CancellationToken, CancellationToken, string> job) : base(id, job)
        {
        }
    }
}