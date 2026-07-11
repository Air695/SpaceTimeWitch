using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Powers;

[RegisterPower]
public class STWForesight : ModPowerTemplate
{

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/SpaceTimeWitch/Powers/{GetType().Name}.png",
        BigIconPath: $"res://images/SpaceTimeWitch/Powers/{GetType().Name}.png"
    );

    public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext,Player player)
    {
        // 只对持有者生效
        if (player.Creature != Owner) return;

        // 先移除自身
        await PowerCmd.Remove(this);

        // 取抽牌堆顶 Amount 张牌
        var drawPile = PileType.Draw.GetPile(player);
        var topCards = drawPile.Cards.Take(Amount).ToList();

        if (topCards.Count == 0) return;

        // 展示选择界面：0～层数 张任选，按从左到右（Top 在前）排列
        var chosen = await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            topCards,
            player,
            new CardSelectorPrefs(
                SelectionScreenPrompt,
                minCount: 0,
                maxCount: topCards.Count   // 或直接 Amount
            )
        );

        // 选中的置入弃牌堆
        foreach (var card in chosen)
        {
            await CardPileCmd.Add(card, PileType.Discard);
        }
    }

}