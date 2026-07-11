using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using SpaceTimeWitch.Commands;
using SpaceTimeWitch.Patches;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Powers;

[RegisterPower]
public class STWTidePower : ModPowerTemplate, IPersonalSpaceSelfStore
{

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/SpaceTimeWitch/Powers/{GetType().Name}.png",
        BigIconPath: $"res://images/SpaceTimeWitch/Powers/{GetType().Name}.png"
    );
    
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return;
        if (Owner?.Player == null) return;

        var player = Owner.Player;

        var allCards = PileType.Hand.GetPile(player).Cards
            .Concat(PileType.Draw.GetPile(player).Cards)
            .Concat(PileType.Discard.GetPile(player).Cards)
            .ToList();

        foreach (var card in allCards)
        {
            await PersonalSpaceCmd.Store(player, card);
        }
    }

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        if (Owner?.Player == null) return;

        var player = Owner.Player;
        var psCards = PersonalSpaceCmd.GetCards(player).ToList();
        if (psCards.Count == 0) return;

        var maxRetrieve = Amount;

        var chosen = await CardSelectCmd.FromSimpleGrid(
            choiceContext, psCards, player,
            new CardSelectorPrefs(
                new LocString("cards", "STW_WITHDRAW"),
                minCount: 0, maxCount: maxRetrieve));

        foreach (var card in chosen)
        {
            await PersonalSpaceCmd.Retrieve(player, card);
        }
    }

}