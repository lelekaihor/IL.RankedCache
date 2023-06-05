# Ranked Cache

* Easy to setup
* Measures how many times each cache entry was accessed
* Allows automatic cleanup based on policy, that you can setup during service registration (MaxItems, Frequency)
* Manual mode, which allows to execute cleanup manually, which will do the same as automatic without scheduling new cleanups
* After each cleanup counters of existing cache entries reset to 1 allowing new cache entries to take leadership in ranking board and survive next cleanup

## Usage

* Package comes with `ServiceCollectionExtensions: AddRankedCache()` that have to be used on startup. Extension has different overloads to cover different invariants of setup.