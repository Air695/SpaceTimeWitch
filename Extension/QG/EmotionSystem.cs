using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib.Combat.SecondaryResources;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace SpaceTimeWitch.Extension.QG;

/// <summary>
/// 情感系统 — 基于次级资源的薄封装。
/// </summary>
public static class EmotionSystem
{
    public static readonly int[] LevelCaps = [5, 5, 8, 11, 11, 0];
    public const int MaxLevel = 5;

    // ─── 读写 ───

    public static int GetPositive(Player player) =>
        SecondaryResourceCmd.Get(player, QGResources.PositiveId);

    public static int GetNegative(Player player) =>
        SecondaryResourceCmd.Get(player, QGResources.NegativeId);

    public static int GetTotal(Player player) =>
        GetPositive(player) + GetNegative(player);

    public static int GetCap(int level)
    {
        var index = level < LevelCaps.Length ? level : LevelCaps.Length - 1;
        return LevelCaps[index];
    }

    public static int GetCurrentCap(Player player)
    {
        return GetCap(GetLevel(player));
    }

    public static bool IsCapReached(Player player) =>
        GetTotal(player) >= GetCurrentCap(player);

    // ─── 增减 ───

    public static async Task AddPositive(Player player, int amount)
    {
        if (!HasQGRelic(player)) return;
        if (IsCapReached(player)) return;
        var maxCanAdd = GetCurrentCap(player) - GetTotal(player);
        if (amount > maxCanAdd) amount = maxCanAdd;
        if (amount <= 0) return;
        QGResources.ShowCounters();
        await SecondaryResourceCmd.Gain(player, QGResources.PositiveId, amount);
    }

    public static async Task AddNegative(Player player, int amount)
    {
        if (!HasQGRelic(player)) return;
        if (IsCapReached(player)) return;
        var maxCanAdd = GetCurrentCap(player) - GetTotal(player);
        if (amount > maxCanAdd) amount = maxCanAdd;
        if (amount <= 0) return;
        QGResources.ShowCounters();
        await SecondaryResourceCmd.Gain(player, QGResources.NegativeId, amount);
    }

    // ─── 等级 ───

    private static readonly Dictionary<Player, int> s_levels = [];

    public static int GetLevel(Player player)
    {
        s_levels.TryGetValue(player, out var lvl);
        return lvl;
    }

    public static void SetLevel(Player player, int level)
    {
        s_levels[player] = level;
    }

    public static bool CanLevelUp(Player player) =>
        IsCapReached(player) && GetLevel(player) < MaxLevel;

    public static async Task<bool> TryLevelUp(Player player)
    {
        if (!CanLevelUp(player)) return false;

        s_levels[player] = GetLevel(player) + 1;
        await SecondaryResourceCmd.Set(player, QGResources.PositiveId, 0);
        await SecondaryResourceCmd.Set(player, QGResources.NegativeId, 0);
        return true;
    }

    // ─── QG遗物检查 ───

    public static bool HasQGRelic(Player player) =>
        player.Relics.OfType<IQGRelic>().Any();

    /// <summary>获取玩家持有的情感遗物中最大的 MaxRelicLevel，无遗物时返回 0</summary>
    public static int GetMaxRelicLevel(Player player)
    {
        return player.Relics.OfType<QGRelicBase>()
            .Select(r => r.MaxRelicLevel)
            .DefaultIfEmpty(0)
            .Max();
    }

    // ─── 升级（由遗物右键调用）──

    /// <summary>
    /// 尝试升级情感等级。找到玩家持有的情感遗物中 MaxRelicLevel 最高的，
    /// 当前等级 &lt; 上限且情感达标时升级并抽取。
    /// </summary>
    public static async Task TryUpgrade(Player player, PlayerChoiceContext ctx)
    {
        if (!CanLevelUp(player)) return;

        var maxLevel = GetMaxRelicLevel(player);
        if (GetLevel(player) >= maxLevel) return;

        var oldLevel = GetLevel(player);
        var prePos = GetPositive(player);
        var preNeg = GetNegative(player);

        if (!await TryLevelUp(player)) return;

        await EmotionDrawSystem.PerformDraw(ctx, player, oldLevel, prePos, preNeg);
    }

    // ─── 战斗重置 ───

    public static async Task ResetForCombat(Player player)
    {
        s_levels.Remove(player);
        await SecondaryResourceCmd.Set(player, QGResources.PositiveId, 0);
        await SecondaryResourceCmd.Set(player, QGResources.NegativeId, 0);
        QGResources.HideCounters();
        EmotionDrawSystem.ResetChosenCards();
    }
}
