using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Uncommon;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class ChronoDissipation : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(12m, ValueProp.Move),
        new PowerVar<STWDissipation>(3m)
    ];


    public ChronoDissipation()
        : base(
            baseCost: 2,
            type: CardType.Skill,
            rarity: CardRarity.Uncommon,
            target: TargetType.Self
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    { 
        var owner = Owner;
        if (owner?.Creature == null) return;
        
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        
        var dAmount = DynamicVars["STWDissipation"].IntValue;
        await PowerCmd.Apply<STWDissipation>(choiceContext, owner.Creature, dAmount,owner.Creature,this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4m);
        DynamicVars["STWDissipation"].UpgradeValueBy(1);
    }
}