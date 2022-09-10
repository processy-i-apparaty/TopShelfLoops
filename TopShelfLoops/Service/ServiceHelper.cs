using System;
using System.Diagnostics;
using System.Threading;

namespace TopShelfLoops.Service
{
    internal static class ServiceHelper
    {
        public static void Wait(TimeSpan timeToWait, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < timeToWait && !cancellationToken.IsCancellationRequested) Thread.Sleep(30);
        }

        public static void WaitAndThrow(TimeSpan timeToWait, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < timeToWait)
            {
                Thread.Sleep(30);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        public static void WaitMultiple(TimeSpan timeToWait, params CancellationToken[] tokens)
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < timeToWait)
            {
                Thread.Sleep(30);
                foreach (CancellationToken cancellationToken in tokens)
                    if (cancellationToken.IsCancellationRequested)
                        return;
            }
        }

        public static void WaitAndThrowMultiple(TimeSpan timeToWait, params CancellationToken[] tokens)
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < timeToWait)
            {
                Thread.Sleep(30);
                foreach (CancellationToken cancellationToken in tokens)
                    cancellationToken.ThrowIfCancellationRequested();
            }
        }


    }
}