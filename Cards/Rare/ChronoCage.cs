using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SpaceTimeWitch.Character;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Rare;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class ChronoCage : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<WeakPower>(1m),
        new PowerVar<ShrinkPower>(1m),
        new DamageVar(10m, ValueProp.Move),
        new CardsVar(2)
    ];

    public ChronoCage()
        : base(
            baseCost: 2,
            type: CardType.Attack,
            rarity: CardRarity.Rare,
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
        
        var wAmount = DynamicVars["WeakPower"].IntValue;
        await PowerCmd.Apply<WeakPower>(choiceContext, play.Target, wAmount,owner.Creature,this);
        var sAmount = DynamicVars["ShrinkPower"].IntValue;
        await PowerCmd.Apply<ShrinkPower>(choiceContext, play.Target, sAmount,owner.Creature,this);
        
        var otherEnemies = CombatState
            .GetTeammatesOf(play.Target)
            .Except(new[] { play.Target })
            .Where(e => e.IsHittable)
            .ToList();
        
        var damageTargets = otherEnemies.Count != 0
            ? (IReadOnlyList<Creature>)otherEnemies
            : new[] { play.Target };
        
        var hitCount = DynamicVars.Cards.IntValue;
        for (var i = 0; i < hitCount; i++)
        {
            await CreatureCmd.Damage(choiceContext, damageTargets,
                DynamicVars.Damage.BaseValue,
                ValueProp.Unpowered | ValueProp.Move,
                Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
    }
}