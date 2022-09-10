using System;
using System.Linq;
using System.Threading;
using Serilog;
using TopShelfLoops.Paralleljob;
using TopShelfLoops.Service;

namespace TopShelfLoops.Logic
{
    internal class CustomLogicSample : ICustomLogic
    {
        private readonly Random _random = new Random();

        public CustomLogicSample()
        {
            Name = nameof(CustomLogicSample);
        }

        public Action<ApplicationConfiguration, CancellationToken> GetCustomAction() => CustomAction;

        public string Name { get; }

        /// <summary>
        ///     Sample CustomLogic Action method
        /// </summary>
        /// <param name="configuration">ApplicationConfiguration</param>
        /// <param name="cancellationToken">CancellationToken</param>
        private void CustomAction(ApplicationConfiguration configuration, CancellationToken cancellationToken)
        {
            Log.Logger.Information("Custom action {0}", DateTimeOffset.Now.ToUnixTimeSeconds());

            int jobsCount = configuration.ParallelCount;

            ParallelJobBase<int, string>[] jobs = CreateJobs(jobsCount);
            int[] inputs = Enumerable.Range(10, jobsCount).ToArray();

            var wrapper =
                new ParallelWrapper<int, string>(jobs, inputs, cancellationToken, configuration.CancelAllOnFirstFault);
            ParallelResult<string>[] results = wrapper.Start().ToArray();
            Log.Logger.Information("[{0}]", string.Join("], [",
                results.Select(x => x)));
        }

        private ParallelJobBase<int, string>[] CreateJobs(int count)
        {
            var jobs = new ParallelJobBase<int, string>[count];
            for (var i = 0; i < count; i++) jobs[i] = new SampleParallelJob(IdGen.GetNew(), Job);
            return jobs;
        }

        /// <summary>
        ///     Sample Parallel job
        /// </summary>
        /// <param name="input">TIn parameter</param>
        /// <param name="localCancellationToken">Cancellation token used to cancel parallel jobs</param>
        /// <param name="globalCancellationToken">Cancellation token used to cancel whole process</param>
        /// <returns>TOut parameter</returns>
        /// <exception cref="Exception"></exception>
        private string Job(int input, CancellationToken localCancellationToken,
            CancellationToken globalCancellationToken)
        {
            TimeSpan span = TimeSpan.FromSeconds(_random.Next(10));
            ServiceHelper.WaitAndThrowMultiple(span, localCancellationToken, globalCancellationToken);
            if (_random.Next(10) == 0) throw new Exception("random exception!");
            return $"JOB {input}";
        }
    }
}