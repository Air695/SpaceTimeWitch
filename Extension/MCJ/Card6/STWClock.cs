using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Character;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Extension.MCJ.Card6;

[RegisterCard(typeof(SpaceTimeWitchExCardPool))]
public class STWClock : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<STWClockP>(5m)
    ];


    public STWClock()
        : base(
            baseCost:1,
            type: CardType.Power,
            rarity: CardRarity.Rare,
            target: TargetType.Self
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;
        
        var ba = DynamicVars["STWClockP"].IntValue;
        await PowerCmd.Apply<STWClockP>(choiceContext, owner.Creature, ba, owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["STWClockP"].UpgradeValueBy(3m);
    }

    protected override string PortraitPath => $"res://images/Extension/Cards/{GetType().Name}.png";
}