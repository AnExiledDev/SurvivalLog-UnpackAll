using System.Collections.Generic;

namespace UnpackAll.Core;

/// <summary>
/// The game as an unpack press sees it. Each call is one read or one thing the player could do by hand. None throws:
/// a call that fails answers as if the game had said no.
/// </summary>
public interface IPackageGame
{
    PackageRead Read();

    /// <summary>The game's own floor pickup into the bag. How many of the drop's packages left the floor, 0 when refused.</summary>
    int PickUp(Package floorPackage);

    /// <summary>The bag's packages as they are now, read again after pickups.</summary>
    IReadOnlyList<Package> BagPackages();

    /// <summary>The game's reason for refusing to use this package right now, or null when it would allow it.</summary>
    string? WhyUnusable(Package package);

    /// <summary>Lines up one unpack of one package from the stack for the character. False when the game would not take it.</summary>
    bool TryQueue(Package package);

    /// <summary>Starts everything lined up as one batch, the way the game's Use button does. False when that failed.</summary>
    bool TryStart();
}
