using System;
using System.Linq;
using UnpackAll.Core;

namespace UnpackAll.Adapters;

/// <summary>
/// Follows the unpacks a press started, looking twice a second, and logs each change in what the character holds:
/// how many of them are still queued and how many packages have left the bag. When none is left it logs the verdict,
/// and shows a line on the page when an unpack ended without opening its package. A look that finds nothing changed
/// builds no log line.
/// </summary>
internal sealed class UnpackWatcher
{
    private const double LookEverySeconds = 0.5;

    private readonly GamePackages _game;
    private readonly WebPage _page;
    private readonly Action<string> _log;
    private readonly Action<string> _detail;

    private UnpackWatch? _watch;
    private int[] _configIds = Array.Empty<int>();
    private double _nextLook;

    /// <param name="detail">Logs a line to the mod's own file only, for the watch's steps.</param>
    public UnpackWatcher(GamePackages game, WebPage page, Action<string> log, Action<string> detail)
    {
        _game = game;
        _page = page;
        _log = log;
        _detail = detail;
    }

    /// <summary>Starts watching the unpacks a press just started, the ones seen in the queue right after the start.</summary>
    public void Begin(int started, double now)
    {
        if (_watch is { IsFinished: false }) _log("watch: a new press replaced the watch over the last one");

        _watch = null;
        _configIds = _game.LinedUpConfigs.ToArray();

        if (started <= 0 || _configIds.Length == 0) return;

        if (_game.LookAtStarted(_configIds) is not { } first)
        {
            _log("watch: the game could not be read, the started unpacks are not followed");

            return;
        }

        _watch = new UnpackWatch(started, first.BagUnits);
        _nextLook = now + LookEverySeconds;

        _log($"watch: following {started} unpack(s) of config {string.Join(",", _configIds)}, {first.BagUnits} in the bag, {first.Pending} with the character; {_game.QueueLine()}");
    }

    public void Tick(double now)
    {
        if (_watch == null || now < _nextLook) return;

        _nextLook = now + LookEverySeconds;

        if (_game.LookAtStarted(_configIds) is not { } look)
        {
            _log("watch: the game could not be read, the watch is dropped");
            _watch = null;

            return;
        }

        Report(_watch.Observe(look.Pending, look.BagUnits, now));

        if (_watch.IsFinished) _watch = null;
    }

    private void Report(WatchStep step)
    {
        switch (step)
        {
            case WatchStep.Quiet:
                break;
            case WatchStep.Changed changed:
                _detail($"watch: {changed.Pending} with the character, {changed.Consumed} package(s) used so far; {_game.QueueLine()}");
                break;
            case WatchStep.Finished finished:
                Finish(finished);
                break;
            default:
                throw new InvalidOperationException("unknown watch step " + step);
        }
    }

    private void Finish(WatchStep.Finished finished)
    {
        var message = OutcomeMessage.Describe(finished, GameLanguage.IsChinese());

        _log($"watch: done, {finished.Consumed} of {finished.Started} started unpack(s) used their package -> {message ?? "(no message)"}");

        if (message != null) _page.Status(message);
    }
}
