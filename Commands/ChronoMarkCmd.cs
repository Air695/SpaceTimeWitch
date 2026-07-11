using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using SpaceTimeWitch.Scripts;
using STS2RitsuLib.Combat.SecondaryResources;

namespace SpaceTimeWitch.Commands;

/// <summary>
/// ChronoMark（时痕）操作 API — 兼容层。
/// 底层使用 RitsuLib 的 SecondaryResourceCmd，保持旧 API 签名不变。
/// </summary>
public static class ChronoMark
{
    public static int GetAmount(Creature creature)
    {
        ArgumentNullException.ThrowIfNull(creature);
        return SecondaryResourceCmd.Get(creature.Player, ModChronoResources.Id);
    }

    public static bool HasEnough(Creature creature, int amount)
    {
        return GetAmount(creature) >= amount;
    }

    /// <summary>
    /// 获得时痕。amount 为 decimal 以兼容旧调用方（如 STWUnleash.Amount）。
    /// </summary>
    public static async Task Gain(Creature creature, decimal amount, CardModel? source = null)
    {
        ArgumentNullException.ThrowIfNull(creature);
        if (amount <= 0m) return;
        await SecondaryResourceCmd.Gain(creature.Player, ModChronoResources.Id, (int)amount);
    }

    /// <summary>
    /// 消耗时痕（非卡牌打出场景）。
    /// </summary>
    public static async Task<bool> Consume(Creature creature, decimal amount, CardModel? source = null)
    {
        ArgumentNullException.ThrowIfNull(creature);
        if (amount <= 0m) return true;
        return await SecondaryResourceCmd.Spend(creature.Player, ModChronoResources.Id, (int)amount);
    }

    /// <summary>
    /// 卡牌打出时扣除 ChronoMark 消耗。
    /// </summary>
    public static async Task SpendStarCost(Creature creature, int starCost, CardModel? source = null)
    {
        if (starCost <= 0) return;
        await SecondaryResourceCmd.Spend(creature.Player, ModChronoResources.Id, starCost);
    }
}
