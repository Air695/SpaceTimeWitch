using SpaceTimeWitch.Extension.MCJ.Card3;

namespace SpaceTimeWitch.Extension.MCJ;

/// <param name="NextTier">下一级（同种门），null = 最高级。</param>
/// <param name="TrapdoorVariant">同级的活板门变体。</param>
/// <param name="DoorVariant">同级的门变体（活板门用）。</param>
/// <param name="Tier">等级 0/1/2。</param>
/// <param name="UpgradeCost">升级花费。</param>
public record DoorCraftData(
    Type? NextTier,
    Type? TrapdoorVariant,
    Type? DoorVariant,
    int Tier,
    int UpgradeCost
);

public static class DoorCraftRegistry
{
    public const int MinecartUnlockCost = 12;

    public static readonly IReadOnlyDictionary<Type, DoorCraftData> Entries =
        new Dictionary<Type, DoorCraftData>
        {
            // 门
            [typeof(STWDoor)]         = new(typeof(STWIronDoor),      typeof(STWTrapdoor),         null,                 0, 8),
            [typeof(STWIronDoor)]     = new(typeof(STWNetheriteDoor), typeof(STWIronTrapdoor),     null,                 1, 12),
            [typeof(STWNetheriteDoor)] = new(null,                     typeof(STWNetheriteTrapdoor), null,                2, 0),

            // 活板门
            [typeof(STWTrapdoor)]         = new(typeof(STWIronTrapdoor),      null, typeof(STWDoor),         0, 8),
            [typeof(STWIronTrapdoor)]     = new(typeof(STWNetheriteTrapdoor), null, typeof(STWIronDoor),     1, 12),
            [typeof(STWNetheriteTrapdoor)] = new(null,                         null, typeof(STWNetheriteDoor), 2, 0),
        };

    public static DoorCraftData? GetData(Type cardType) =>
        Entries.TryGetValue(cardType, out var d) ? d : null;

    /// <summary>根据 tier 获取对应等级的门类型。</summary>
    public static Type? GetDoorAtTier(int tier)
    {
        foreach (var (type, data) in Entries)
        {
            if (data.DoorVariant == null && data.Tier == tier) // 门本身的条目
                return type;
        }
        return null;
    }

    /// <summary>根据 tier 获取对应等级的活板门类型。</summary>
    public static Type? GetTrapdoorAtTier(int tier)
    {
        foreach (var (type, data) in Entries)
        {
            if (data.TrapdoorVariant == null && data.Tier == tier) // 活板门本身的条目
                return type;
        }
        return null;
    }
}
