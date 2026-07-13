using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using SpaceTimeWitch.Commands;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Powers;

[RegisterPower]
public class STWSingularityPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/SpaceTimeWitch/Powers/{GetType().Name}.png",
        BigIconPath: $"res://images/SpaceTimeWitch/Powers/{GetType().Name}.png"
    );

    protected override bool IsVisibleInternal => false;

    /// <summary>
    /// 玩家回合开始后：若 Singularity 卡牌在手牌中，触发一次奇点效果并递减次数。
    /// 次数归零时消耗卡牌并移除自身。
    /// 使用 AfterPlayerTurnStartLate：确保 GainEnergy 和卡牌选择 UI 正常工作，且升级卡牌时的异步 Apply 已完成。
    /// </summary>
    public override async Task AfterPlayerTurnStartLate(
        PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;

        // 奇点卡牌必须在手牌中
        var hand = PileType.Hand.GetPile(player);
        var token = hand.Cards.OfType<Cards.Token.STWSingularity>().FirstOrDefault();
        if (token == null) return;

        // 卡牌次数为唯一真相来源，能力层数从卡牌同步
        if (Amount != token.RemainingUses)
        {
            SetAmount(token.RemainingUses);
        }

        if (Amount <= 0) return;

        await TriggerOnce(choiceContext, player);

        await PowerCmd.Decrement(this);

        // 同步卡牌显示次数
        token.RemainingUses = Amount;

        // 次数归零
        if (Amount <= 0)
        {
            await CardCmd.Exhaust(choiceContext, token);
            await PowerCmd.Remove(this);
        }
    }

    /// <summary>
    /// 执行一次奇点效果：力敏集中辉星时痕 + 能量抽牌 + 复现 + 发现。
    /// ChronoCollapse 升级后调用此方法立即触发一次。
    /// </summary>
    public static async Task TriggerOnce(PlayerChoiceContext choiceContext, Player player)
    {
        var creature = player.Creature;
        if (creature == null) return;

        await PowerCmd.Apply<StrengthPower>(choiceContext, creature, 1m, creature, null);
        await PowerCmd.Apply<DexterityPower>(choiceContext, creature, 1m, creature, null);
        await PowerCmd.Apply<FocusPower>(choiceContext, creature, 1m, creature, null);
        await PlayerCmd.GainStars(1, player);
        await ChronoMark.Gain(creature, 1);

        await PlayerCmd.GainEnergy(1, player);
        await CardPileCmd.Draw(choiceContext, 1, player);

        // 从链接时空复现 1 张
        var reproduced = await DiscoverCmd.Discover(
            choiceContext, player,
            cardType: null,
            offerCount: 5, minCount: 0, maxCount: 1,
            prompt: new LocString("cards", "STW_SHARED_CHOOSE_CARD"));
        var rCard = reproduced.FirstOrDefault();
        if (rCard != null)
            await CardPileCmd.AddGeneratedCardToCombat(rCard, PileType.Hand, creator: rCard.Owner);

        // 从构筑卡组中发现 1 张（展示卡组实例，保留升级等数值；选中后创建复制品入手牌）
        var deckCards = PileType.Deck.GetPile(player).Cards
            .DistinctBy(c => c.Id)
            .OrderBy(c => c.Id)
            .OrderBy(_ => player.RunState.Rng.CombatCardGeneration.NextInt())
            .Take(3)
            .ToList();

        if (deckCards.Count > 0)
        {
            var chosen = (await CardSelectCmd.FromSimpleGrid(
                choiceContext, deckCards, player,
                new CardSelectorPrefs(
                    new LocString("cards", "STW_DECK_DISCOVER_PROMPT"), 0, 1)))
                .FirstOrDefault();

            if (chosen != null)
            {
                // 通过 canonical 模板重建，并复制升级等级
                var canonical = ModelDb.AllCardPools
                    .SelectMany(p => p.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint))
                    .FirstOrDefault(c => c.Id == chosen.Id);

                if (canonical != null)
                {
                    var copy = player.Creature!.CombatState.CreateCard(canonical, player);
                    for (int i = 0; i < chosen.CurrentUpgradeLevel; i++)
                        CardCmd.Upgrade(copy);
                    await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, creator: copy.Owner);
                }
            }
        }
    }
}
