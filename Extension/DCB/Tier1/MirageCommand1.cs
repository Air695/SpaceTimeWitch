using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Extension.DCB.Tier1;

[RegisterCard(typeof(SpaceTimeWitchExCardPool))]
public class MirageCommand1 : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.DCB1
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1)
    ];
    
    protected override IEnumerable<IHoverTip> CardSpecificHoverTips =>
    [
        HoverTipFactory.FromCard<STWMirageBlades>(),
    ];

    public MirageCommand1()
        : base(
            baseCost:0,
            type: CardType.Skill,
            rarity: CardRarity.Common,
            target: TargetType.RandomEnemy
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;
        
        for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
        {
            var mb = CombatState.CreateCard<STWMirageBlades>(owner);
            await CardPileCmd.AddGeneratedCardToCombat(mb, PileType.Hand,Owner);
            await Cmd.Wait(0.1f);
        }

        // 收集手牌、抽牌堆、弃牌堆、消耗堆中所有的 STWMirageBlades
        var handBlades = PileType.Hand.GetPile(owner).Cards.OfType<STWMirageBlades>().ToList();
        var drawBlades = PileType.Draw.GetPile(owner).Cards.OfType<STWMirageBlades>().ToList();
        var discardBlades = PileType.Discard.GetPile(owner).Cards.OfType<STWMirageBlades>().ToList();
        var exhaustBlades = PileType.Exhaust.GetPile(owner).Cards.OfType<STWMirageBlades>().ToList();
        var allBlades = handBlades.Concat(drawBlades).Concat(discardBlades).Concat(exhaustBlades).ToList();

        if (allBlades.Count == 0) return;

        // 升级后：先升级所有 STWMirageBlades
        if (IsUpgraded)
        {
            foreach (var blade in allBlades)
                CardCmd.Upgrade(blade);
        }

        // 获取存活敌人用于随机目标选择
        var aliveEnemies = CombatState.HittableEnemies
            .Where(e => !e.IsDead)
            .ToList();

        if (aliveEnemies.Count == 0) return;

        // 对每把剑：随机选取敌人 → 触发 → 置入个人空间
        foreach (var blade in allBlades)
        {
            var randomTarget = aliveEnemies.Count == 1
                ? aliveEnemies[0]
                : owner.RunState.Rng.CombatCardGeneration.NextItem(aliveEnemies);

            if (randomTarget != null && !randomTarget.IsDead)
                await CardCmd.AutoPlay(choiceContext, blade, randomTarget);

            // AutoPlay 后剑可能因 Exhaust 关键词进入消耗堆，但仍可存入个人空间
            await PersonalSpaceCmd.Store(owner, blade);
        }
    }

    protected override void OnUpgrade()
    {
    }

    protected override string PortraitPath => "res://images/Extension/Cards/MirageCommand.png";
}
