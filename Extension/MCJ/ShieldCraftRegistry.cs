using SpaceTimeWitch.Extension.MCJ.Card6;

namespace SpaceTimeWitch.Extension.MCJ;

public record ShieldOption(Type Type, string Label, int UnlockCost);

public static class ShieldCraftRegistry
{
    public const int TotemCost = 20;

    // 解锁位掩码
    public const int FlagClock    = 1 << 0;
    public const int FlagSpyglass = 1 << 1;
    public const int FlagBucket   = 1 << 2;
    public const int FlagElytra   = 1 << 3;

    /// <summary>所有可选类型（盾始终第一，Totem 单独处理）。</summary>
    public static readonly IReadOnlyList<ShieldOption> Options = new[]
    {
        new ShieldOption(typeof(STWShield),   "SHIELD",    0),
        new ShieldOption(typeof(STWClock),    "CLOCK",    5),
        new ShieldOption(typeof(STWSpyglass), "SPYGLASS", 5),
        new ShieldOption(typeof(STWBucket),   "BUCKET",   3),
        new ShieldOption(typeof(STWElytra),   "ELYTRA",   20),
    };

    public static int GetFlag(string label) => label switch
    {
        "CLOCK"    => FlagClock,
        "SPYGLASS" => FlagSpyglass,
        "BUCKET"   => FlagBucket,
        "ELYTRA"   => FlagElytra,
        _          => 0,
    };
}
