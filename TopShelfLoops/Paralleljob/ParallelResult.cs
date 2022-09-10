using System;

namespace TopShelfLoops.Paralleljob
{
    internal class ParallelResult<T>
    {
        public ParallelResult(ulong id = 0, bool isOk = false, T payload = default, bool isCancelled = false,
            Exception exception = default)
        {
            Id = id;
            IsOk = isOk;
            Payload = payload;
            IsCancelled = isCancelled;
            Exception = exception;
        }

        public bool IsCancelled { get; }
        public ulong Id { get; }
        public bool IsOk { get; }
        public T Payload { get; }
        public Exception Exception { get; }

        public override string ToString()
        {
            if (IsOk) return $"{Id}; OK; {Payload}";
            if (IsCancelled) return $"{Id}; CANCELLED";
            if (Exception != null) return $"{Id}; EXCEPTION; {Exception.Message}";
            return $"{Id}; FAIL";
        }
    }
}