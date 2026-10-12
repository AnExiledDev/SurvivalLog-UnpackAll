using System;

namespace UnpackAll.Core;

/// <summary>
/// The game's own tests for what a package is and how many it will unpack, copied from the unpack effect
/// (AE_GenerateBagFurniture.Run) and the use check (ItemManager.VerifyItemUse) so the mod never asks for an unpack the
/// game would refuse.
/// </summary>
public static class PackageRules
{
    public const int PackageCategory = 14;
    public const string UnpackEffect = "Action/Item_Generate_BagFurniture";

    /// <summary>
    /// A package is an item of the package category that names the furniture inside it and whose use action runs the
    /// unpack effect. The effect refuses anything else, so nothing else is worth queueing.
    /// </summary>
    public static bool IsPackage(int category, int targetFurnitureId, string? useEffectName) =>
        category == PackageCategory && targetFurnitureId > 0 && useEffectName == UnpackEffect;

    /// <summary>
    /// How many more packages the home takes: one free package spot each, and none at all once the build-shop
    /// purchases reach the home's capacity, which is when the game answers "full".
    /// </summary>
    public static int FreeSpots(int purchaseCount, int maxCapacity, int freePackagePoints) =>
        purchaseCount < maxCapacity ? Math.Max(0, freePackagePoints) : 0;
}
