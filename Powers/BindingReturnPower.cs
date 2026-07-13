using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using SpaceTimeWitch.Cards.KeyWords;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Powers;

[RegisterPower]
public class BindingReturnPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.None;
    public override PowerStackType StackType => PowerStackType.Single;
    protected override bool IsVisibleInternal => false;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext,
        CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner?.Side) return;

        var player = Owner?.Player;
        if (player?.PlayerCombatState == null) return;
        var hand = player.PlayerCombatState.Hand;
        const int maxHand = 10;

        foreach (var card in BindingHandler.ConsumePlayedCards())
        {
            var pileType = card.Pile?.Type;
            if (pileType != PileType.Discard && pileType != PileType.Draw)
                continue;

            if (hand.Cards.Count < maxHand)
                await CardPileCmd.Add(card, PileType.Hand);
        }
    }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: null,
        BigIconPath: null
    );
}
