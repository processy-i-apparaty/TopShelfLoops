namespace TopShelfLoops.Service
{
    internal static class IdGen
    {
        private static ulong _id = 1;
        private static readonly object _lock = new object();

        public static ulong GetNew()
        {
            lock (_lock)
            {
                return _id++;
            }
        }
    }
}