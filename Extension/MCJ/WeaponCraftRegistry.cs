using SpaceTimeWitch.Extension.MCJ.Card1;

namespace SpaceTimeWitch.Extension.MCJ;

/// <summary>
/// 武器卡牌的制作数据：升级目标（下一级）、替换目标（同级的剑↔斧）。
/// </summary>
/// <param name="NextTier">下一级武器类型，null 表示已是最高级。</param>
/// <param name="Alternate">同级的替代武器（剑→斧 或 斧→剑），null 表示无替代。</param>
/// <param name="UpgradeCost">升级花费的材料数量。</param>
/// <param name="ReplaceCost">替换花费的材料数量。</param>
public record WeaponCraftData(
    Type? NextTier,
    Type? Alternate,
    int UpgradeCost = 1,
    int ReplaceCost = 1
);

/// <summary>
/// 武器制作注册表 —— 集中定义每张武器卡的升级链和替换关系。
/// 格式: [typeof(木剑)] = new(NextTier: typeof(石剑), Alternate: typeof(木斧), MaterialCost: 2)
/// </summary>
public static class WeaponCraftRegistry
{
    public static readonly IReadOnlyDictionary<Type, WeaponCraftData> Entries =
        new Dictionary<Type, WeaponCraftData>
        {
            // 剑（升级费用逐渐递增，替换费用固定 1）
            [typeof(STWWoodenSword)]    = new(typeof(STWStoneSword),     typeof(STWWoodenAxe),      UpgradeCost: 3, ReplaceCost: 0),
            [typeof(STWStoneSword)]     = new(typeof(STWIronSword),      typeof(STWStoneAxe),       UpgradeCost: 5, ReplaceCost: 0),
            [typeof(STWIronSword)]      = new(typeof(STWDiamondSword),   typeof(STWIronAxe),        UpgradeCost: 7, ReplaceCost: 0),
            [typeof(STWDiamondSword)]   = new(typeof(STWNetheriteSword), typeof(STWDiamondAxe),     UpgradeCost: 9, ReplaceCost: 0),
            [typeof(STWNetheriteSword)] = new(null,                      typeof(STWNetheriteAxe),   UpgradeCost: 0, ReplaceCost: 0),

            // 斧
            [typeof(STWWoodenAxe)]     = new(typeof(STWStoneAxe),      typeof(STWWoodenSword),    UpgradeCost: 3, ReplaceCost: 0),
            [typeof(STWStoneAxe)]      = new(typeof(STWIronAxe),       typeof(STWStoneSword),     UpgradeCost: 5, ReplaceCost: 0),
            [typeof(STWIronAxe)]       = new(typeof(STWDiamondAxe),    typeof(STWIronSword),      UpgradeCost: 7, ReplaceCost: 0),
            [typeof(STWDiamondAxe)]    = new(typeof(STWNetheriteAxe),  typeof(STWDiamondSword),   UpgradeCost: 9, ReplaceCost: 0),
            [typeof(STWNetheriteAxe)]  = new(null,                     typeof(STWNetheriteSword), UpgradeCost: 0, ReplaceCost: 0),
        };

    public static WeaponCraftData? GetData(Type cardType) =>
        Entries.TryGetValue(cardType, out var data) ? data : null;
}
