using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using SpaceTimeWitch.Character;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Uncommon;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class ChronoBurst : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(20m, ValueProp.Move),
    ];

    public ChronoBurst()
        : base(
            baseCost: 2,
            type: CardType.Attack,
            rarity: CardRarity.Common,
            target: TargetType.AnyEnemy
        )
    {
        SetChronoMarkCost(1);
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await using AttackContext context =
            await AttackCommand.CreateContextAsync(CombatState, choiceContext, this);

        var primaryTarget = cardPlay.Target;
        var otherEnemies = CombatState
            .GetTeammatesOf(primaryTarget)
            .Except(new[] { primaryTarget })
            .Where(e => e.IsHittable)
            .ToList();

        decimal halfDamage = 0m;

        if (otherEnemies.Count > 0)
        {
            decimal baseDamage = DynamicVars.Damage.BaseValue;
            decimal predictedDamage = baseDamage;

            var combatState = owner.Creature.CombatState;
            var allPowers = combatState.Allies
                .Concat(combatState.Enemies)
                .SelectMany(c => c.Powers)
                .ToList();

            foreach (var power in allPowers)
                predictedDamage += power.ModifyDamageAdditive(
                    primaryTarget, predictedDamage, ValueProp.Move, owner.Creature, this);

            foreach (var power in allPowers)
                predictedDamage = Math.Floor(predictedDamage * power.ModifyDamageMultiplicative(
                    primaryTarget, predictedDamage, ValueProp.Move, owner.Creature, this));

            halfDamage = Math.Floor(predictedDamage / 2m);
        }

        var results = (await CreatureCmd.Damage(
            choiceContext,
            cardPlay.Target,
            DynamicVars.Damage.BaseValue,
            ValueProp.Move,
            this
        )).ToList();
        context.AddHit(results);

        if (otherEnemies.Count > 0 && halfDamage > 0)
        {
            context.AddHit(await CreatureCmd.Damage(
                choiceContext,
                otherEnemies,
                halfDamage,
                ValueProp.Unpowered | ValueProp.Move,
                owner.Creature,
                this
            ));
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6m);
    }
}