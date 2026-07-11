using SpaceTimeWitch.Extension.MCJ.Card5;

namespace SpaceTimeWitch.Extension.MCJ;

public record SpearCraftData(Type? NextTier, int UpgradeCost);

public static class SpearCraftRegistry
{
    public const int TridentUnlockCost = 12;
    public const int MaceUnlockCost = 10;

    public const int UnlockFlagTrident = 1;
    public const int UnlockFlagMace = 2;

    public static readonly IReadOnlyDictionary<Type, SpearCraftData> Entries =
        new Dictionary<Type, SpearCraftData>
        {
            [typeof(STWWoodenSpear)]    = new(typeof(STWStoneSpear),    3),
            [typeof(STWStoneSpear)]     = new(typeof(STWIronSpear),     5),
            [typeof(STWIronSpear)]      = new(typeof(STWDiamondSpear),  7),
            [typeof(STWDiamondSpear)]   = new(typeof(STWNetheriteSpear),9),
            [typeof(STWNetheriteSpear)] = new(null,                     0),
        };

    public static SpearCraftData? GetData(Type t) =>
        Entries.TryGetValue(t, out var d) ? d : null;

    /// <summary>根据 tier 获取对应等级的矛类型（用于从特殊武器恢复）。</summary>
    /// <param name="tier">0=Wood, 1=Stone, 2=Iron, 3=Diamond, 4=Netherite</param>
    public static Type? GetSpearAtTier(int tier) => tier switch
    {
        0 => typeof(STWWoodenSpear),
        1 => typeof(STWStoneSpear),
        2 => typeof(STWIronSpear),
        3 => typeof(STWDiamondSpear),
        4 => typeof(STWNetheriteSpear),
        _ => null,
    };

    public static int GetTier(Type spearType)
    {
        var data = GetData(spearType);
        if (data == null) return 0;
        // Count upgrades from this type to determine tier
        int tier = 0;
        var t = spearType;
        while (true)
        {
            bool found = false;
            foreach (var (entryType, entryData) in Entries)
            {
                if (entryData.NextTier == t) { t = entryType; tier++; found = true; break; }
            }
            if (!found) break;
        }
        return tier;
    }
}
