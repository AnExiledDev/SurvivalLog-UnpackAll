using System;

namespace UnpackAll.Core;

/// <summary>The toast a Pick up press ends with, in English or in Chinese.</summary>
public static class PickUpMessage
{
    public static string Describe(PickUpOutcome outcome, bool isChinese = false) => outcome switch
    {
        PickUpOutcome.Gone => Texts.PickUpGone.Pick(isChinese),
        PickUpOutcome.Refused refused => isChinese ? $"无法拿起这个包裹：{refused.Reason.Zh}。" : $"Can't pick up this package: {refused.Reason.En}.",
        PickUpOutcome.NoRoom => Texts.PickUpNoRoom.Pick(isChinese),
        PickUpOutcome.Failed { TookBack: true } => Texts.PickUpFailed.Pick(isChinese),
        PickUpOutcome.Failed => Texts.PickUpFailedKept.Pick(isChinese),
        PickUpOutcome.PickedUp picked => Picked(picked, isChinese),
        _ => throw new InvalidOperationException("unknown pick up outcome " + outcome),
    };

    private static string Picked(PickUpOutcome.PickedUp picked, bool isChinese)
    {
        var line = (picked.Place == PickUpPlace.Bag ? Texts.PickedIntoBag : Texts.PickedIntoShoppingBag).Pick(isChinese);

        return picked.WasBought ? line + (isChinese ? "" : " ") + Texts.NoLongerRefundable.Pick(isChinese) : line;
    }
}
