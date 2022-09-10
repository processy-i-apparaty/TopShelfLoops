using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace TopShelfLoops.Paralleljob
{
    internal class ParallelWrapper<TIn, TOut>
    {
        private readonly bool _cancelAllOnFirstFault;
        private readonly CancellationToken _globalCancellationToken;
        private readonly IList<TIn> _inputs;
        private readonly IList<ParallelJobBase<TIn, TOut>> _jobs;
        private readonly object _lock = new object();
        private CancellationTokenSource _localCancellationTokenSource;

        public ParallelWrapper(IList<ParallelJobBase<TIn, TOut>> jobs, IList<TIn> inputs,
            CancellationToken globalCancellationToken, bool cancelAllOnFirstFault = true)
        {
            if (jobs.Count != inputs.Count)
                throw new ArgumentException($"{nameof(ParallelWrapper<TIn, TOut>)}; jobs.Count != inputs.Count");
            _jobs = jobs;
            _inputs = inputs;
            _globalCancellationToken = globalCancellationToken;
            _cancelAllOnFirstFault = cancelAllOnFirstFault;
        }

        private void Cancel()
        {
            lock (_lock)
            {
                _localCancellationTokenSource?.Cancel();
            }
        }

        private CancellationToken CreateToken()
        {
            lock (_lock)
            {
                _localCancellationTokenSource = new CancellationTokenSource();
                return _localCancellationTokenSource.Token;
            }
        }

        private void DestroyToken()
        {
            lock (_lock)
            {
                _localCancellationTokenSource?.Dispose();
                _localCancellationTokenSource = null;
            }
        }

        public IEnumerable<ParallelResult<TOut>> Start()
        {
            int count = _jobs.Count;
            var results = new ParallelResult<TOut>[count];
            CancellationToken localCancellationToken = CreateToken();
            var parallelOptions = new ParallelOptions
            {
                CancellationToken = localCancellationToken,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            try
            {
                Parallel.For(0, count, parallelOptions, i =>
                {
                    ParallelResult<TOut> result =
                        _jobs[i].RunJob(_inputs[i], localCancellationToken, _globalCancellationToken);
                    results[i] = result;
                    LogTaskInfo(result);
                    if (_cancelAllOnFirstFault && !result.IsOk) Cancel();
                });
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                DestroyToken();
            }

            return results;
        }

        private void LogTaskInfo(ParallelResult<TOut> result)
        {
            Log.Debug("{0}; ID: {1}; isOk: {2}; payload: {3}; isCancelled: {4}; exception: {5}; thread: {6}",
                nameof(LogTaskInfo), result.Id, result.IsOk, result.Payload, result.IsCancelled,
                result.Exception?.Message ?? "null", Thread.CurrentThread.ManagedThreadId);
        }
    }
}