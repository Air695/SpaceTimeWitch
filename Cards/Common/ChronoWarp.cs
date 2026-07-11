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
public class ChronoWarp : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<STWStrengthless>(5m),
        new PowerVar<VulnerablePower>(2m)
    ];


    public ChronoWarp()
        : base(
            baseCost: 0,
            type: CardType.Skill,
            rarity: CardRarity.Common,
            target: TargetType.AnyEnemy
        )
    {
        SetChronoMarkCost(1);
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;
        
        var vA = DynamicVars["VulnerablePower"].IntValue;
        await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target, vA, owner.Creature, this);

        var sA = DynamicVars["STWStrengthless"].IntValue;
        await PowerCmd.Apply<STWStrengthless>(choiceContext, play.Target, sA, owner.Creature, this);
        
    }

    protected override void OnUpgrade()
    {
        DynamicVars["STWStrengthless"].UpgradeValueBy(3m);
    }
}