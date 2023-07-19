[![NuGet version (IL.RankedCache)](https://img.shields.io/nuget/v/IL.RankedCache.svg?style=flat-square)](https://www.nuget.org/packages/IL.RankedCache/)
# Ranked Cache

* Easy to setup
* Measures how many times each cache entry was accessed
* Allows automatic cleanup based on policy, that you can setup during service registration (MaxItems, Frequency)
* Manual mode, which allows to execute cleanup manually, which will do the same as automatic without scheduling new cleanups
* After each cleanup counters of existing cache entries reset to 1 allowing new cache entries to take leadership in ranking board and survive next cleanup

## Usage

* Package comes with `ServiceCollectionExtensions: AddRankedCache()` that have to be used on startup. Extension has different overloads to cover different invariants of setup.

* Ranked cache with default setup options (1000 MaxItems, CleanupMode.Auto, Frequency = TimeSpan.FromHours(1)), constraint sets type to be used for counter - pick counter type based on expected scenario and memory efficiency limitations. If counter reaches MaxValue of selected type no exception will be thrown, following cache item access attempts will keep the counter at MaxValue:
  * `new ServiceCollection().AddRankedCache<short>()` 
  * `new ServiceCollection().AddRankedCache<int>()`
  * `new ServiceCollection().AddRankedCache<long>()`
* Ranked cache with custom options:
  * `new ServiceCollection().AddRankedCache<short>(options => options.MaxItems = 1500)`
  * `new ServiceCollection().AddRankedCache<int>(options => { options.MaxItems = 5000; options.CleanupMode = CleanupMode.Manual; })`
  * `new ServiceCollection().AddRankedCache<long>(options => { options.MaxItems = 1500; options.CleanupMode = CleanupMode.Auto; options.Frequency = TimeSpan.FromHours(12); })`
* Ranked cache with custom cache provider implementation & custom options:
  * `new ServiceCollection().AddRankedCache<int, CustomICacheProviderImplementation>()`
  * `new ServiceCollection().AddRankedCache<int, CustomICacheProviderImplementation>(options => options.MaxItems = 1500)`

## Nuget
  https://www.nuget.org/packages/IL.RankedCache/
