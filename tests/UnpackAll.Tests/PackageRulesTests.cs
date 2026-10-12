using UnpackAll.Core;
using Xunit;

namespace UnpackAll.Tests;

public class PackageRulesTests
{
    [Theory]
    [InlineData(14, 5001, "Action/Item_Generate_BagFurniture", true)]
    [InlineData(13, 5001, "Action/Item_Generate_BagFurniture", false)]
    [InlineData(14, 0, "Action/Item_Generate_BagFurniture", false)]
    [InlineData(14, 5001, "Action/Item_Default", false)]
    [InlineData(14, 5001, null, false)]
    public void A_package_is_a_package_category_item_that_names_its_furniture_and_runs_the_unpack_effect(int category, int targetFurnitureId, string? effect, bool isPackage)
    {
        Assert.Equal(isPackage, PackageRules.IsPackage(category, targetFurnitureId, effect));
    }

    [Fact]
    public void Below_the_purchase_cap_every_free_package_point_is_a_spot()
    {
        Assert.Equal(3, PackageRules.FreeSpots(purchaseCount: 2, maxCapacity: 5, freePackagePoints: 3));
    }

    [Fact]
    public void Once_purchases_reach_the_home_capacity_no_package_can_be_unpacked()
    {
        Assert.Equal(0, PackageRules.FreeSpots(purchaseCount: 5, maxCapacity: 5, freePackagePoints: 3));
        Assert.Equal(0, PackageRules.FreeSpots(purchaseCount: 6, maxCapacity: 5, freePackagePoints: 3));
    }

    [Fact]
    public void Free_spots_are_never_negative()
    {
        Assert.Equal(0, PackageRules.FreeSpots(purchaseCount: 0, maxCapacity: 5, freePackagePoints: -1));
    }
}
