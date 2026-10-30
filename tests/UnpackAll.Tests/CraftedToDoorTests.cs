using System.Collections.Generic;
using UnpackAll.Core;
using Xunit;

namespace UnpackAll.Tests;

/// <summary>
/// Crafted furniture goes to the entryway on its own, but never when the player turned it off, never a package an open
/// commission wants handed in, and never more than the entryway has room for (the game complains for each extra one).
/// </summary>
public class CraftedToDoorTests
{
    private const int Table = 14014;
    private const int Chair = 14066;

    private static readonly CraftedPackage[] TwoTables = { new(101, Table), new(102, Table) };

    private static List<long> Pick(bool isOn, IReadOnlyList<CraftedPackage> crafted, int room, params int[] wanted)
    {
        var send = new List<long> { 999 };

        CraftedToDoor.Pick(isOn, crafted, config => System.Array.IndexOf(wanted, config) >= 0, room, send);

        return send;
    }

    [Fact]
    public void Everything_crafted_goes_when_there_is_room()
    {
        Assert.Equal(new long[] { 101, 102 }, Pick(isOn: true, TwoTables, room: 5));
    }

    [Fact]
    public void Switched_off_sends_nothing()
    {
        Assert.Empty(Pick(isOn: false, TwoTables, room: 5));
    }

    [Fact]
    public void A_package_a_commission_wants_stays_a_package()
    {
        var crafted = new CraftedPackage[] { new(101, Table), new(102, Chair), new(103, Table) };

        Assert.Equal(new long[] { 102 }, Pick(isOn: true, crafted, room: 5, wanted: Table));
    }

    [Fact]
    public void A_full_entryway_leaves_the_package()
    {
        Assert.Empty(Pick(isOn: true, TwoTables, room: 0));
    }

    [Fact]
    public void Only_as_many_go_as_the_entryway_has_room_for()
    {
        Assert.Equal(new long[] { 101 }, Pick(isOn: true, TwoTables, room: 1));
    }

    [Fact]
    public void A_wanted_package_does_not_use_up_the_room()
    {
        var crafted = new CraftedPackage[] { new(101, Table), new(102, Chair) };

        Assert.Equal(new long[] { 102 }, Pick(isOn: true, crafted, room: 1, wanted: Table));
    }

    [Fact]
    public void The_toast_counts_what_was_sent()
    {
        Assert.Null(CraftedToDoor.Message(0, isChinese: false));
        Assert.Equal(Texts.SentOneToDoor.En, CraftedToDoor.Message(1, isChinese: false));
        Assert.StartsWith("3 crafted furniture packages", CraftedToDoor.Message(3, isChinese: false));
        Assert.StartsWith("3 件", CraftedToDoor.Message(3, isChinese: true));
    }

    [Fact]
    public void The_setting_description_is_english_then_chinese()
    {
        var parts = Settings.CraftedToDoor.Split('\n');

        Assert.Equal(2, parts.Length);
        Assert.Contains("entryway", parts[0]);
        Assert.Contains("玄关", parts[1]);
    }
}
