using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Rare;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class STWBombardment : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10m, ValueProp.Move)
    ];

    public STWBombardment()
        : base(
            baseCost:3,
            type: CardType.Attack,
            rarity: CardRarity.Rare,
            target: TargetType.AllEnemies
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        // X = 当前时痕数
        var x = ChronoMark.GetAmount(owner.Creature);
        if (x <= 0) return;

        // 随机选取个人空间中最多 2X 张卡牌
        var rng = owner.RunState.Rng.CombatCardGeneration;
        var psCards = PersonalSpaceCmd.GetCards(owner)
            .OrderBy(_ => rng.NextInt())
            .Take(2 * x)
            .ToList();

        if (psCards.Count == 0) return;

        // 实际消耗的时痕 = ⌈卡牌数 / 2⌉，返还多余
        var chronoSpent = (psCards.Count + 1) / 2;
        var refund = x - chronoSpent;

        await ChronoMark.Consume(owner.Creature, x);
        if (refund > 0)
            await ChronoMark.Gain(owner.Creature, refund);

        var damagePerCard = DynamicVars.Damage.BaseValue;

        foreach (var card in psCards)
        {
            await CardCmd.Exhaust(choiceContext, card);
        }

        for (int i = 0; i < psCards.Count; i++)
        {
            if (CurrentUpgradeLevel > 0)
            {
                // 升级后：对所有敌人造成伤害
                await DamageCmd.Attack(damagePerCard)
                    .FromCard(this)
                    .TargetingAllOpponents(CombatState)
                    .Execute(choiceContext);
            }
            else
            {
                // 普通：对一个随机敌人造成伤害
                var aliveEnemies = CombatState.HittableEnemies
                    .Where(e => !e.IsDead).ToList();
                if (aliveEnemies.Count == 0) break;

                var target = aliveEnemies[rng.NextInt() % aliveEnemies.Count];
                await DamageCmd.Attack(damagePerCard)
                    .FromCard(this)
                    .Targeting(target)
                    .Execute(choiceContext);
            }
        }
    }

    protected override void OnUpgrade()
    {
    }
}