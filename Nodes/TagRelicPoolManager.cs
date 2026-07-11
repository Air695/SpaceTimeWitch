using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using SpaceTimeWitch.Extension.MCJ;
using SpaceTimeWitch.Relics;
using SpaceTimeWitch.Scripts;

namespace SpaceTimeWitch.Nodes;

/// <summary>
/// 标签遗物卡池查看牌堆管理器。
/// 在每次战斗开始时刷新牌堆内容，展示与玩家标签遗物关联的所有卡牌。
/// </summary>
public static class TagRelicPoolManager
{
    /// <summary>
    /// 清除并重新填充标签遗物卡池查看牌堆。
    /// </summary>
    public static void RefreshPile(Player player)
    {
        var pile = Entry.TagRelicPoolPile.GetPile(player);

        // 获取玩家当前所有标签遗物的激活标签
        var activeTags = player.Relics
            .OfType<ITagRelic>()
            .Select(r => r.AssociatedTag)
            .ToHashSet();

        // 清除旧内容
        pile.Clear();

        if (activeTags.Count == 0)
        {
            pile.InvokeContentsChanged();
            return;
        }

        // 镐类遗物的卡牌绑定类型名（跳过已永久禁用的槽位）
        var pickaxeTypeNames = player.Relics
            .OfType<IPickaxeRelic>()
            .SelectMany(p =>
            {
                var bindings = PickaxeCardBindings.Deserialize(p.GetCardBindingsData());
                bool shieldDisabled = ShieldCraftSubMenu.IsSlotDisabled(p);
                return bindings.AllSlots
                    .Where((t, i) => t != null && !(i == 5 && shieldDisabled))
                    .Select(t => t!.Name);
            })
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // 从所有卡池筛选匹配卡牌（遵循解锁/多人游戏过滤，去重）
        var constraint = player.RunState.CardMultiplayerConstraint;
        var candidates = ModelDb.AllCardPools
            .SelectMany(p => p.GetUnlockedCards(player.UnlockState, constraint))
            .Where(c => c.Tags.Any(t => activeTags.Contains(t))
                        || pickaxeTypeNames.Contains(c.GetType().Name))
            .GroupBy(c => c.Id)
            .Select(g => g.First())
            .ToList();

        // 创建可变副本并加入牌堆
        foreach (var prototype in candidates)
        {
            var mutable = prototype.ToMutable();
            mutable.Owner = player;
            pile.AddInternal(mutable, silent: true);
        }

        // 通知 UI 刷新计数
        pile.InvokeContentsChanged();
    }
}
