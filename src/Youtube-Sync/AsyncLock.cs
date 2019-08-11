using System;
using System.Threading;
using System.Threading.Tasks;
 
namespace Youtube_Sync
{
    public class AsyncLock : IDisposable
    {
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
 
        public async Task<AsyncLock> LockAsync()
        {
            await _semaphoreSlim.WaitAsync();
            return this;
        }
 
        public void Dispose()
        {
            _semaphoreSlim.Release();
        }
    }
}