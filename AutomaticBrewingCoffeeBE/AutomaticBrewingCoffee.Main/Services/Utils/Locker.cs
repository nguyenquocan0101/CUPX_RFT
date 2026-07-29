using System.Collections.Concurrent;

namespace Services.Utils;

/// <summary>
/// Multi-target synchronization mechanism with lazy-controlled, fine-grained locking.
/// </summary>
public static class Locker
{
    // Nested Lazy wrapper to reduce contention + support clean disposal later if needed
    private static readonly ConcurrentDictionary<string, Lazy<SemaphoreSlim>> _locks =
        new(StringComparer.OrdinalIgnoreCase); // Case-insensitive keys

    /// <summary>
    /// Retrieves or initializes a keyed semaphore for serialized access per resource.
    /// </summary>
    /// <param name="key">The unique identifier of the critical section scope.</param>
    /// <returns>SemaphoreSlim instance (1 concurrent slot per key).</returns>
    public static SemaphoreSlim Acquire(string key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        return _locks.GetOrAdd(key, static k =>
            new Lazy<SemaphoreSlim>(() => new SemaphoreSlim(1, 1), LazyThreadSafetyMode.ExecutionAndPublication)
        ).Value;
    }

    /// <summary>
    /// Optional cleanup if you want to remove locks after long idleness.
    /// </summary>
    /// <param name="key">The target key to clear from the pool.</param>
    /// <returns>True if removed.</returns>
    public static bool TryRelease(string key)
    {
        return _locks.TryRemove(key, out _);
    }
}