using SpaceTimeWitch.Extension.MCJ.Card4;

namespace SpaceTimeWitch.Extension.MCJ;

/// <param name="NextTier">下一级靴子，null = 最高级。</param>
/// <param name="Chestplate">同级的胸甲（替换目标）。</param>
/// <param name="UpgradeCost">升级花费。</param>
public record BootCraftData(Type? NextTier, Type? Chestplate, int UpgradeCost);

public static class BootCraftRegistry
{
    public static readonly IReadOnlyDictionary<Type, BootCraftData> Entries =
        new Dictionary<Type, BootCraftData>
        {
            // 靴
            [typeof(STWLeatherBoots)]   = new(typeof(STWIronBoots),      typeof(STWLeatherTunic),       5),
            [typeof(STWIronBoots)]      = new(typeof(STWDiamondBoots),   typeof(STWIronChestplate),  8),
            [typeof(STWDiamondBoots)]   = new(typeof(STWNetheriteBoots), typeof(STWDiamondChestplate), 11),
            [typeof(STWNetheriteBoots)] = new(null,                      typeof(STWNetheriteChestplate), 0),

            // 胸甲（可升级，也可替换回靴）
            [typeof(STWLeatherTunic)]          = new(typeof(STWIronChestplate),      typeof(STWLeatherBoots),   5),
            [typeof(STWIronChestplate)]     = new(typeof(STWDiamondChestplate),   typeof(STWIronBoots),      8),
            [typeof(STWDiamondChestplate)]  = new(typeof(STWNetheriteChestplate), typeof(STWDiamondBoots),   11),
            [typeof(STWNetheriteChestplate)] = new(null,                           typeof(STWNetheriteBoots), 0),
        };

    public static BootCraftData? GetData(Type t) =>
        Entries.TryGetValue(t, out var d) ? d : null;
}
