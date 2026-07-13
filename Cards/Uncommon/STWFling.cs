using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Uncommon;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class STWFling : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9m,ValueProp.Move)
    ];


    public STWFling()
        : base(
            baseCost:1,
            type: CardType.Attack,
            rarity: CardRarity.Uncommon,
            target: TargetType.AnyEnemy
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(play.Target)
            .Execute(choiceContext);

        var attackCards = PersonalSpaceCmd.GetCards(owner)
            .Where(c => c.Type == CardType.Attack)
            .OrderBy(c => c.Id)
            .ToList();

        if (attackCards.Count == 0) return;

        var chosen = owner.RunState.Rng.CombatCardGeneration.NextItem(attackCards);

        await CardPileCmd.Add(chosen, PileType.Draw);

        // 检查目标是否被 STWFling 自身伤害击杀，若是则随机另选目标
        var target = play.Target;
        if (target.IsDead)
        {
            var aliveEnemies = CombatState.HittableEnemies
                .Where(e => !e.IsDead)
                .ToList();
            target = aliveEnemies.Count > 0
                ? owner.RunState.Rng.CombatCardGeneration.NextItem(aliveEnemies)
                : null;
        }

        if (target != null && !target.IsDead)
            await CardCmd.AutoPlay(choiceContext, chosen, target);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}