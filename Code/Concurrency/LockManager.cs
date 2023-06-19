using System.Collections.Concurrent;

namespace IL.RankedCache.Concurrency
{
    internal static class LockManager
    {
        private static readonly ConcurrentDictionary<string, Lock> Locks = new();

        public static IDisposable GetLock(string key)
        {
            var concurrentLock = Locks.GetOrAdd(key, _ => new Lock());
            concurrentLock.Wait();
            return concurrentLock;
        }

        public static async Task<IDisposable> GetLockAsync(string key)
        {
            var concurrentLock = Locks.GetOrAdd(key, _ => new Lock());
            await concurrentLock.WaitAsync();
            return concurrentLock;
        }

        private class Lock : IDisposable
        {
            private readonly SemaphoreSlim _semaphoreSlim = new(1);

            public void Wait()
            {
                _semaphoreSlim.Wait();
            }

            public async Task WaitAsync()
            {
                await _semaphoreSlim.WaitAsync();
            }

            public void Dispose()
            {
                _semaphoreSlim.Release();
            }
        }
    }
}