using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace SpaceTimeWitch.Patches;

/// <summary>
/// 拦截 AfterCardChangedPiles：当 STWSingularity 以任意方式进入手牌时，自动施加 STWSingularityPower。
/// 这样即使通过非 ChronoCollapse 途径获取 Singularity 也能正常运作。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardChangedPiles))]
public static class SingularityPowerPatch
{
    [HarmonyPostfix]
    static void Postfix(CardModel card, PileType oldPile)
    {
        if (card is not Cards.Token.STWSingularity token) return;
        if (oldPile == PileType.Hand) return;
        if (card.Pile?.Type != PileType.Hand) return;

        var owner = card.Owner;
        if (owner?.Creature == null) return;

        var existingPower = owner.Creature.GetPower<Powers.STWSingularityPower>();
        if (existingPower != null) return;

        if (token.RemainingUses <= 0)
        {
            token.RemainingUses = 1;
        }

        PowerCmd.Apply<Powers.STWSingularityPower>(
            new ThrowingPlayerChoiceContext(),
            owner.Creature,
            token.RemainingUses,
            owner.Creature,
            null).GetAwaiter().GetResult();
    }
}
