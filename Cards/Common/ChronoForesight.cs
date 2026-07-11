using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Common;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class ChronoForesight : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.MarkA
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<STWForesight>(3m),
        new PowerVar<NChronoMark>(2m),
        new PowerVar<STWNextTurnDraw>(2m)
    ];

    public ChronoForesight()
        : base(
            baseCost: 0,
            type: CardType.Skill,
            rarity: CardRarity.Common,
            target: TargetType.Self
        )
    {
        SetChronoMarkCost(1);
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;
        
        var fAmount = DynamicVars["STWForesight"].IntValue;
        await PowerCmd.Apply<STWForesight>(choiceContext, owner.Creature, fAmount,owner.Creature,this);
        
        var dcntAmount = DynamicVars["STWNextTurnDraw"].IntValue;
        await PowerCmd.Apply<STWNextTurnDraw>(choiceContext, owner.Creature, dcntAmount,owner.Creature,this);

        var nmarkAmount = DynamicVars["NChronoMark"].IntValue;
        if (CurrentUpgradeLevel > 0)
            await PowerCmd.Apply<NChronoMark>(choiceContext, owner.Creature, nmarkAmount,owner.Creature,this);
        
    }

    protected override void OnUpgrade()
    {
    }
}