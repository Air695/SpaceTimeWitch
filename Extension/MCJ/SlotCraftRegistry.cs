using SpaceTimeWitch.Extension.MCJ.Card2;

namespace SpaceTimeWitch.Extension.MCJ;

/// <summary>
/// 每个卡牌槽位的制作替换选项注册表。
/// 槽位 0 (Card1) 使用 WeaponCraftRegistry 的升级/替换体系，
/// 其余槽位在此定义可替换的卡牌列表。
/// </summary>
public static class SlotCraftRegistry
{
    /// <summary>槽位 → 该槽位可替换的卡牌类型列表。</summary>
    public static readonly IReadOnlyDictionary<int, IReadOnlyList<Type>> ReplaceOptions = new Dictionary<int, IReadOnlyList<Type>>
    {
        // Card2 — 弓/弩/风弹（槽位 1）
        [1] = [typeof(STWBow), typeof(STWCrossbow), typeof(STWWindCharge)],
    };

    /// <summary>替换花费的材料数量（所有替换槽位统一）。</summary>
    public const int ReplaceCost = 0;
}
