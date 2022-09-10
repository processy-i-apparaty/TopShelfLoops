using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Serilog;

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

        public static T GetAppConfig<T>()
        {
            Type t = typeof(T);
            PropertyInfo[] properties = t.GetProperties();
            Log.Logger.Debug("{0}; type: {1}", nameof(GetAppConfig), t.Name);
            foreach (PropertyInfo propertyInfo in properties)
            {
                Log.Logger.Debug("prop name: {0}; prop type: {1}",
                    propertyInfo.Name, propertyInfo.PropertyType.Name);
            }

            return (T)Activator.CreateInstance(t);
        }
    }
}