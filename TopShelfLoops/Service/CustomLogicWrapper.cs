using System;
using System.Threading;
using Serilog;

namespace TopShelfLoops.Service
{
    internal class CustomLogicWrapper
    {
        private readonly ApplicationConfiguration _configuration;
        private readonly Action<ApplicationConfiguration, CancellationToken> _customAction;

        public CustomLogicWrapper(ICustomLogic customLogic, ApplicationConfiguration configuration)
        {
            _configuration = configuration;
            _customAction = customLogic.GetCustomAction();
        }

        public void Run(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _customAction.Invoke(_configuration, cancellationToken);
                    ServiceHelper.Wait(_configuration.TimeToWait, cancellationToken);
                }
                catch (Exception e)
                {
                    Log.Logger.Error("{0}; exception; {1}",
                        nameof(CustomLogicWrapper), e.ToString());
                }
            }
        }
    }
}