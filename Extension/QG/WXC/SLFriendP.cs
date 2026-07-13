using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SpaceTimeWitch.Extension.Powers;
using MegaCrit.Sts2.Core.Models;
using SpaceTimeWitch.Powers;

namespace SpaceTimeWitch.Extension.QG.WXC;

[RegisterPower]
public class SLFriendP : ModPowerTemplate
{
    public override PowerType Type => PowerType.None;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/Extension/Powers/SLQZ.png",
        BigIconPath: $"res://images/Extension/Powers/SLQZ.png"
    );

    /// <summary>当前被标记的卡牌，供泛光规则查询。</summary>
    public static CardModel? MarkedCard { get; private set; }

    private CardModel? _markedCard;
    private bool _cardPlayed;

    // ── 回合开始：随机选一张手牌，赋予虚无 ──

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;

        // 清理上回合状态
        if (_markedCard != null && !_cardPlayed)
            _markedCard.RemoveKeyword(CardKeyword.Ethereal);
        MarkedCard = null;
        _markedCard = null;
        _cardPlayed = false;

        var hand = PileType.Hand.GetPile(player);
        var playableCards = hand.Cards.Where(c => c.CanPlay()).OrderBy(c => c.Id).ToList();
        if (playableCards.Count == 0) return;

        var rng = player.RunState.Rng.CombatCardGeneration;
        _markedCard = playableCards[rng.NextInt(playableCards.Count)];
        _markedCard.AddKeyword(CardKeyword.Ethereal);
        MarkedCard = _markedCard;
    }

    // ── 打出标记卡牌：对所有敌人施加效果 ──

    public override async Task AfterCardPlayed(
        PlayerChoiceContext ctx, CardPlay cardPlay)
    {
        if (cardPlay.Card != _markedCard) return;
        if (_cardPlayed) return;

        _cardPlayed = true;
        MarkedCard = null;
        _markedCard?.RemoveKeyword(CardKeyword.Ethereal);

        foreach (var enemy in CombatState.HittableEnemies)
        {
            // 4 流血
            await PowerCmd.Apply<STWBleed>(ctx, enemy, 4, Owner, cardPlay.Card);
            // 失去 6 生命
            await CreatureCmd.Damage(
                ctx, enemy, 6, ValueProp.Unblockable | ValueProp.Unpowered,
                Owner, cardPlay.Card);
            // 失去 3 力量
            await PowerCmd.Apply<TSP>(ctx, enemy, -3, Owner, cardPlay.Card);
        }
    }

    // ── 回合结束：未打出 → 所有玩家获得 2 流血 ──

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return;
        if (_markedCard == null || _cardPlayed) return;

        foreach (var p in CombatState.Players)
        {
            if (p.Creature != null && p.Creature.IsAlive)
                await PowerCmd.Apply<STWBleed>(
                    choiceContext, p.Creature, 2, Owner, null);
        }
    }
}