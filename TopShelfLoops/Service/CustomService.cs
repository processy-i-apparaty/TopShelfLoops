using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;
using Topshelf;
using Topshelf.Logging;

namespace TopShelfLoops.Service
{
    internal class CustomService
    {
        private readonly LogWriter _log;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _customTask;

        public CustomService()
        {
            _log = HostLogger.Get<CustomService>();
        }

        public static TopshelfExitCode RunService(string serviceName, string displayName, string description,
            ICustomLogic customLogic, ApplicationConfiguration configuration)
        {
            return HostFactory.Run(x =>
            {
                x.Service<CustomService>(s =>
                {
                    s.ConstructUsing(n => new CustomService());
                    s.WhenStarted(tc => tc.Start(customLogic, configuration));
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();

                x.SetDescription(description);
                x.SetDisplayName(displayName);
                x.SetServiceName(serviceName);

                var name = $"{(string.IsNullOrWhiteSpace(customLogic.Name) ? "app" : customLogic.Name)}";


                LoggerConfiguration loggerConfiguration = new LoggerConfiguration().MinimumLevel.Debug();

                if (configuration.LogDebug)
                {
                    loggerConfiguration.WriteTo.File(name + "_debug.log",
                        rollingInterval: configuration.LogRollingInterval,
                        rollOnFileSizeLimit: true);
                }

                if (configuration.LogWarn)
                {
                    loggerConfiguration.WriteTo.File(name + "_warn.log",
                        LogEventLevel.Warning,
                        rollingInterval: configuration.LogRollingInterval,
                        rollOnFileSizeLimit: true);
                }

                if (configuration.LogNotepad)
                {
                    loggerConfiguration.WriteTo.Notepad();
                }

#if DEBUG
                loggerConfiguration.WriteTo.Console();
#endif

                Log.Logger = loggerConfiguration.CreateLogger();

                x.UseSerilog();
            });
        }

        public void Start(ICustomLogic customLogic, ApplicationConfiguration configuration)
        {
            var logicWrapper = new CustomLogicWrapper(customLogic, configuration);

            _cancellationTokenSource = new CancellationTokenSource();
            _customTask = new Task(() => logicWrapper.Run(_cancellationTokenSource.Token));
            _log.Info("Starting custom logic");
            _customTask.Start();
        }

        public void Stop()
        {
            _log.Warn("Stopping service");
            _cancellationTokenSource.Cancel();
            _customTask.Wait();
            _cancellationTokenSource.Dispose();
            _log.Info("Service stopped");
        }
    }
}