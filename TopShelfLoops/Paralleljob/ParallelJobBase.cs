using System;
using System.Threading;
using Serilog;

namespace TopShelfLoops.Paralleljob
{
    internal abstract class ParallelJobBase<TIn, TOut>
    {
        private readonly Func<TIn, CancellationToken, CancellationToken, TOut> _job;

        protected ParallelJobBase(ulong id, Func<TIn, CancellationToken, CancellationToken, TOut> job)
        {
            _job = job;
            Id = id;
        }

        public ulong Id { get; }

        public ParallelResult<TOut> RunJob(TIn input, CancellationToken localCancellationToken,
            CancellationToken globalCancellationToken)
        {
            var nameOf = $"{nameof(ParallelJobBase<TIn, TOut>)}.{nameof(RunJob)}";

            try
            {
                TOut result = _job.Invoke(input, localCancellationToken, globalCancellationToken);
                return new ParallelResult<TOut>(Id, true, result);
            }
            catch (OperationCanceledException)
            {
                Log.Logger.Warning("{0}; {1}; Operation Cancelled", nameOf, Id);
                return new ParallelResult<TOut>(Id, false, isCancelled: true);
            }
            catch (Exception e)
            {
                Log.Logger.Warning("{0}; {1}; Job exception; {2}", nameOf, Id, e.ToString());
                return new ParallelResult<TOut>(Id, false, exception: e);
            }
        }
    }
}