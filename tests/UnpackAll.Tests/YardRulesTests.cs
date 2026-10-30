using UnpackAll.Core;
using Xunit;

namespace UnpackAll.Tests;

public class YardRulesTests
{
    [Theory]
    [InlineData("HomePackage1", true)]
    [InlineData("HomePackage12", true)]
    [InlineData("HomePackage", false)]
    [InlineData("HomePackageCar", false)]
    [InlineData("DoorBox0", false)]
    [InlineData("HomePackage1a", false)]
    [InlineData(null, false)]
    public void Only_the_yards_numbered_package_spots_count(string? node, bool isYardSpot)
    {
        Assert.Equal(isYardSpot, YardRules.IsHomePackageSpot(node));
    }

    [Fact]
    public void A_dyed_package_turns_back_into_the_item_with_its_dye()
    {
        var dyes = new Dictionary<int, int> { [100] = 0, [101] = 2, [102] = 3 };

        Assert.Equal(102, YardRules.ItemFor(new[] { 100, 101, 102 }, id => dyes[id], dyeSlot: 3));
        Assert.Equal(100, YardRules.ItemFor(new[] { 100, 101, 102 }, id => dyes[id], dyeSlot: 0));
    }

    [Fact]
    public void A_package_with_no_matching_item_turns_into_nothing()
    {
        Assert.Equal(0, YardRules.ItemFor(new[] { 100 }, _ => 0, dyeSlot: 5));
        Assert.Equal(0, YardRules.ItemFor(Array.Empty<int>(), _ => 0, dyeSlot: 0));
    }
}
