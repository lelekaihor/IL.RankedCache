using System.Collections.Concurrent;

namespace IL.RankedCache.Concurrency
{
    internal static class LockManager
    {
        private static readonly ConcurrentDictionary<string, Lazy<Lock>> Locks = new();

        public static IDisposable GetLock(string key)
        {
            var lazyLock = Locks.GetOrAdd(key, new Lazy<Lock>(() => new Lock()));
            var concurrentLock = lazyLock.Value;
            concurrentLock.Wait();
            return concurrentLock;
        }

        public static async Task<IDisposable> GetLockAsync(string key)
        {
            var lazyLock = Locks.GetOrAdd(key, new Lazy<Lock>(() => new Lock()));
            var concurrentLock = lazyLock.Value;
            await concurrentLock.WaitAsync();
            return concurrentLock;
        }

        internal class Lock : IDisposable
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

            public int GetState()
            {
                return _semaphoreSlim.CurrentCount;
            }
        }
    }
}