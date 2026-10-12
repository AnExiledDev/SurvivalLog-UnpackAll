namespace UnpackAll.Core;

/// <summary>
/// Packages of one kind lying in one place: a bag item, or a floor drop. ItemId is the id the game acts on there (the
/// bag item's id, or the drop's pickup id). Count is above one only when a stacking mod lets packages stack; every
/// package in it still takes one unpack and one free spot.
/// </summary>
public readonly record struct Package(long ItemId, int ConfigId, int Count);
