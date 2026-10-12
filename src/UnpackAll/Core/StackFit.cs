using System;
using System.Collections.Generic;
using System.Linq;

namespace UnpackAll.Core;

/// <summary>
/// Which whole stacks to move into a number of free spots, when a stack can only move whole: the set whose packages
/// fill the most spots without going over, then of those the one with the fewest stacks, then the one whose stacks come
/// earliest in the list.
/// </summary>
public static class StackFit
{
    /// <summary>
    /// The most spots one pick looks at. A home has far fewer; the cap only keeps the search table small whatever the
    /// read says, and spots past it are left for the next press.
    /// </summary>
    public const int MaxSpots = 1000;

    private const int Unreachable = int.MaxValue;

    /// <summary>The indexes of the stacks to move, in list order. A size below one never takes part.</summary>
    public static IReadOnlyList<int> Pick(IReadOnlyList<int> sizes, int spots)
    {
        var total = sizes.Sum(size => (long)Math.Max(0, size));
        var capacity = (int)Math.Min(Math.Min(Math.Max(0, spots), MaxSpots), total);
        var fewest = FewestStacks(sizes, capacity);
        var filled = capacity;

        while (filled > 0 && fewest[0, filled] == Unreachable) filled--;

        return Trace(sizes, fewest, filled);
    }

    /// <summary>
    /// fewest[i, units] is the fewest stacks, from index i on, whose packages add up to exactly units, or Unreachable
    /// when none do.
    /// </summary>
    private static int[,] FewestStacks(IReadOnlyList<int> sizes, int capacity)
    {
        var fewest = new int[sizes.Count + 1, capacity + 1];

        for (var units = 1; units <= capacity; units++) fewest[sizes.Count, units] = Unreachable;

        for (var i = sizes.Count - 1; i >= 0; i--)
        {
            for (var units = 0; units <= capacity; units++) fewest[i, units] = Math.Min(fewest[i + 1, units], WithStack(fewest, i, sizes[i], units));
        }

        return fewest;
    }

    /// <summary>The fewest stacks that make exactly units when stack i is one of them.</summary>
    private static int WithStack(int[,] fewest, int i, int size, int units)
    {
        if (size <= 0 || size > units || fewest[i + 1, units - size] == Unreachable) return Unreachable;

        return fewest[i + 1, units - size] + 1;
    }

    /// <summary>Walks the list in order and takes a stack whenever the fewest count still holds with it, so earlier stacks win ties.</summary>
    private static IReadOnlyList<int> Trace(IReadOnlyList<int> sizes, int[,] fewest, int units)
    {
        var picked = new List<int>();

        for (var i = 0; i < sizes.Count && units > 0; i++)
        {
            if (WithStack(fewest, i, sizes[i], units) != fewest[i, units]) continue;

            picked.Add(i);

            units -= sizes[i];
        }

        return picked;
    }
}
