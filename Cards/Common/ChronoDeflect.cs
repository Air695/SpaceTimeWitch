using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Common;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class ChronoDeflect : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.MarkA
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(9m, ValueProp.Move),
        new PowerVar<STWDeflect>(1m)
    ];


    public ChronoDeflect()
        : base(
            baseCost: 1,
            type: CardType.Skill,
            rarity: CardRarity.Common,
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
        
        var amount = DynamicVars["STWDeflect"].IntValue;
        await PowerCmd.Apply<STWDeflect>(choiceContext, owner.Creature, amount,owner.Creature,this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}