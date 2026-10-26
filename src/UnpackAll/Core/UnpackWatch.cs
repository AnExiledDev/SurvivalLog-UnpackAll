using System;

namespace UnpackAll.Core;

/// <summary>
/// Follows the unpacks a press started until none is left with the character, and tells apart an unpack that used its
/// package from one that left the queue with the package still in the bag. The game drops an unpack it cannot carry
/// out without a word, so this is the only way such a press is found out. Used packages are read off the bag: an
/// unpack takes its package out of the bag when it runs.
/// </summary>
public sealed class UnpackWatch
{
    /// <summary>How long the queue must stay empty before the verdict, so a package used up a moment after its unpack left the queue still counts.</summary>
    public const double SettleSeconds = 2;

    /// <summary>Made once: most looks find nothing changed, and the watcher looks twice a second.</summary>
    private static readonly WatchStep Quiet = new WatchStep.Quiet();

    private readonly int _started;
    private readonly int _bagUnitsBefore;
    private int _lastPending;
    private int _lastConsumed;
    private double? _emptySince;

    /// <param name="started">The unpacks seen in the character's queue right after the press started them.</param>
    /// <param name="bagUnits">The units of those packages' kinds in the bag at that moment.</param>
    public UnpackWatch(int started, int bagUnits)
    {
        _started = Math.Max(0, started);
        _bagUnitsBefore = Math.Max(0, bagUnits);
        _lastPending = _started;
    }

    public bool IsFinished { get; private set; }

    /// <param name="pending">The uses of the started packages' kinds still with the character, waiting or running.</param>
    /// <param name="bagUnits">The units of those kinds in the bag now.</param>
    /// <param name="now">Seconds on any clock that only moves forward.</param>
    public WatchStep Observe(int pending, int bagUnits, double now)
    {
        if (IsFinished) return Quiet;

        var consumed = Math.Clamp(_bagUnitsBefore - bagUnits, 0, _started);
        var isChanged = pending != _lastPending || consumed != _lastConsumed;
        _lastPending = pending;
        _lastConsumed = consumed;

        if (pending > 0) _emptySince = null;
        else _emptySince ??= now;

        if (_emptySince is { } emptySince && now - emptySince >= SettleSeconds)
        {
            IsFinished = true;

            return new WatchStep.Finished(_started, consumed);
        }

        return isChanged ? new WatchStep.Changed(pending, consumed) : Quiet;
    }
}

/// <summary>What one look at the started unpacks found.</summary>
public abstract record WatchStep
{
    private WatchStep()
    {
    }

    public sealed record Quiet : WatchStep;

    /// <summary>Pending uses are still with the character; Consumed packages have left the bag since the start.</summary>
    public sealed record Changed(int Pending, int Consumed) : WatchStep;

    /// <summary>The queue has held none of the started unpacks for a while: Consumed of the Started used their package.</summary>
    public sealed record Finished(int Started, int Consumed) : WatchStep;
}
