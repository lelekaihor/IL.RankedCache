﻿using IL.RankedCache.CacheProvider;
using IL.RankedCache.Extensions;
using IL.RankedCache.Policies;
using IL.RankedCache.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace IL.RankedCache.Tests.Extensions
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddRankedCache_NonSpecialized_DefaultCacheProvider_AddsServices()
        {
            // Arrange
            var services = new ServiceCollection();
        
            // Act
            services.AddRankedCache();
        
            // Assert
            var serviceProvider = services.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<ICacheProvider>());
            Assert.NotNull(serviceProvider.GetService<IRankedCacheService>());
            Assert.IsType<DefaultInMemoryCacheProvider>(serviceProvider.GetService<ICacheProvider>());
            Assert.IsType<RankedCacheService>(serviceProvider.GetService<IRankedCacheService>());
        }

        [Fact]
        public void AddRankedCache_DefaultCacheProvider_AddsServices()
        {
            // Arrange
            var services = new ServiceCollection();
        
            // Act
            services.AddRankedCacheSpecialized<short>();
        
            // Assert
            var serviceProvider = services.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<ICacheProvider>());
            Assert.NotNull(serviceProvider.GetService<IRankedCacheService<short>>());
            Assert.IsType<DefaultInMemoryCacheProvider>(serviceProvider.GetService<ICacheProvider>());
            Assert.IsType<RankedCacheService<short>>(serviceProvider.GetService<IRankedCacheService<short>>());
        }

        [Fact]
        public void AddRankedCache_CustomCacheProvider_AddsServices()
        {
            // Arrange
            var services = new ServiceCollection();
        
            // Act
            services.AddRankedCacheSpecialized<short, CustomCacheProvider>();
        
            // Assert
            var serviceProvider = services.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<ICacheProvider>());
            Assert.NotNull(serviceProvider.GetService<IRankedCacheService<short>>());
            Assert.IsType<CustomCacheProvider>(serviceProvider.GetService<ICacheProvider>());
            Assert.IsType<RankedCacheService<short>>(serviceProvider.GetService<IRankedCacheService<short>>());
        }

        [Fact]
        public void AddRankedCache_WithOptions_ConfiguresOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var expected = 150;
        
            // Act
            services.AddRankedCacheSpecialized<int>(options => options.MaxItems = expected);
        
            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<RankedCachePolicy>>();
            Assert.NotNull(options);
            Assert.Equal(expected, options.Value.MaxItems);
        }

        [Fact]
        public void AddRankedCache_WithUnsupportedType_ThrowsNotSupportedException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => services.AddRankedCacheSpecialized<byte>());
        }

        private class CustomCacheProvider : ICacheProvider
        {
            public Task<T?> GetAsync<T>(string key)
            {
                throw new NotImplementedException();
            }

            public T? Get<T>(string key)
            {
                throw new NotImplementedException();
            }

            public Task AddAsync<T>(string key, T obj, DateTimeOffset? absoluteExpiration)
            {
                throw new NotImplementedException();
            }

            public void Add<T>(string key, T obj, DateTimeOffset? absoluteExpiration)
            {
                throw new NotImplementedException();
            }

            public Task DeleteAsync(string key)
            {
                throw new NotImplementedException();
            }

            public void Delete(string key)
            {
                throw new NotImplementedException();
            }

            public bool HasKey(string key)
            {
                throw new NotImplementedException();
            }
        }
    }
}