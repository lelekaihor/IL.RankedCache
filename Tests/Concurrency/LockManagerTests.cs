using IL.RankedCache.Concurrency;
using Xunit;

namespace IL.RankedCache.Tests.Concurrency
{
    public class LockManagerTests
    {
        [Fact]
        public void GetLock_SameKey_ReturnsSameInstance()
        {
            // Arrange
            var key = "testKey";

            // Act
            var lock1 = LockManager.GetLock(key);
            lock1.Dispose();
            var lock2 = LockManager.GetLock(key);
            lock2.Dispose();

            // Assert
            Assert.Same(lock1, lock2);
        }

        [Fact]
        public async Task GetLockAsync_SameKey_ReturnsSameInstance()
        {
            // Arrange
            var key = "testKey";

            // Act
            var lock1 = await LockManager.GetLockAsync(key);
            lock1.Dispose();
            var lock2 = await LockManager.GetLockAsync(key);
            lock2.Dispose();

            // Assert
            Assert.Same(lock1, lock2);
        }

        [Fact]
        public void GetLock_DifferentKeys_ReturnsDifferentInstances()
        {
            // Arrange
            var key1 = "key1";
            var key2 = "key2";

            // Act
            var lock1 = LockManager.GetLock(key1);
            lock1.Dispose();
            var lock2 = LockManager.GetLock(key2);
            lock2.Dispose();

            // Assert
            Assert.NotSame(lock1, lock2);
        }

        [Fact]
        public async Task GetLockAsync_DifferentKeys_ReturnsDifferentInstances()
        {
            // Arrange
            var key1 = "key1";
            var key2 = "key2";

            // Act
            var lock1 = await LockManager.GetLockAsync(key1);
            lock1.Dispose();
            var lock2 = await LockManager.GetLockAsync(key2);
            lock2.Dispose();

            // Assert
            Assert.NotSame(lock1, lock2);
        }

        [Fact]
        public void GetLock_Dispose_Releases_Lock_But_Allows_To_Reuse_It()
        {
            // Arrange
            var key = "testKey";
            IDisposable lockObj1;
            IDisposable lockObj2;
            int countInsideUsing1;
            int countOutsideUsing1;
            int countInsideUsing2;
            int countOutsideUsing2;
            // Act
            using (lockObj1 = LockManager.GetLock(key))
            {
                // Assert
                Assert.NotNull(lockObj1);
                countInsideUsing1 = ((LockManager.Lock)lockObj1).GetState();
            }

            // Lock should be disposed at this point
            countOutsideUsing1 = ((LockManager.Lock)lockObj1).GetState();

            using (lockObj2 = LockManager.GetLock(key))
            {
                // Assert
                Assert.NotNull(lockObj2);
                countInsideUsing2 = ((LockManager.Lock)lockObj2).GetState();
            }

            // Lock should be disposed at this point
            countOutsideUsing2 = ((LockManager.Lock)lockObj2).GetState();

            // Act & Assert
            Assert.NotEqual(countInsideUsing1, countOutsideUsing1);
            Assert.NotEqual(countInsideUsing2, countOutsideUsing2);

            Assert.Equal(countInsideUsing1, countInsideUsing2);
            Assert.Equal(countOutsideUsing1, countOutsideUsing2);

            Assert.Same(lockObj1, lockObj2);
        }

        [Fact]
        public async Task GetLockAsync_Dispose_Releases_Lock_But_Allows_To_Reuse_It()
        {
            // Arrange
            var key = "testKey";
            IDisposable lockObj1;
            IDisposable lockObj2;
            int countInsideUsing1;
            int countOutsideUsing1;
            int countInsideUsing2;
            int countOutsideUsing2;
            // Act
            using (lockObj1 = await LockManager.GetLockAsync(key))
            {
                // Assert
                Assert.NotNull(lockObj1);
                countInsideUsing1 = ((LockManager.Lock)lockObj1).GetState();
            }

            // Lock should be disposed at this point
            countOutsideUsing1 = ((LockManager.Lock)lockObj1).GetState();

            using (lockObj2 = await LockManager.GetLockAsync(key))
            {
                // Assert
                Assert.NotNull(lockObj2);
                countInsideUsing2 = ((LockManager.Lock)lockObj2).GetState();
            }

            // Lock should be disposed at this point
            countOutsideUsing2 = ((LockManager.Lock)lockObj2).GetState();

            // Act & Assert
            Assert.NotEqual(countInsideUsing1, countOutsideUsing1);
            Assert.NotEqual(countInsideUsing2, countOutsideUsing2);

            Assert.Equal(countInsideUsing1, countInsideUsing2);
            Assert.Equal(countOutsideUsing1, countOutsideUsing2);

            Assert.Same(lockObj1, lockObj2);
        }
    }
}