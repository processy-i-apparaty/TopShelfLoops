using System;
using System.Threading;

namespace TopShelfLoops.Service
{
    internal interface ICustomLogic
    {
        /// <summary>
        ///     Custom logic name. Will be used in log filename.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Custom logic Action. Takes ApplicationConfiguration and Cancellation token
        /// </summary>
        /// <returns>Action</returns>
        Action<ApplicationConfiguration, CancellationToken> GetCustomAction();
    }
}