using SpaceTimeWitch.Extension.MCJ.Card7;

namespace SpaceTimeWitch.Extension.MCJ;

public record BeaconCraftData(Type? NextTier, int UpgradeCost);

public static class BeaconCraftRegistry
{
    public static readonly IReadOnlyDictionary<Type, BeaconCraftData> Entries =
        new Dictionary<Type, BeaconCraftData>
        {
            [typeof(STWBeacon1)] = new(typeof(STWBeacon2), 8),
            [typeof(STWBeacon2)] = new(typeof(STWBeacon3), 8),
            [typeof(STWBeacon3)] = new(typeof(STWBeacon4), 8),
            [typeof(STWBeacon4)] = new(typeof(STWBeacon5), 8),
            [typeof(STWBeacon5)] = new(null,               0),
        };

    public static BeaconCraftData? GetData(Type t) =>
        Entries.TryGetValue(t, out var d) ? d : null;
}
