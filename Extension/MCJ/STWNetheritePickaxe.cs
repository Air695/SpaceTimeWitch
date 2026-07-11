using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Relics;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace SpaceTimeWitch.Extension.MCJ;

[RegisterRelic(typeof(SpaceTimeWitchExRelicPool))]
public class STWNetheritePickaxe : TagRelic, IPickaxeRelic
{
    public static readonly SavedAttachedState<STWNetheritePickaxe, int> Materials = new("Netherite_Materials", _ => 0);
    public static readonly SavedAttachedState<STWNetheritePickaxe, string> CardBindings = new("Netherite_CardBindings", _ => PickaxeCardBindings.DefaultsSerialized);
    public static readonly SavedAttachedState<STWNetheritePickaxe, string> DoorData = new("Netherite_DoorData", _ => "");
    public static readonly SavedAttachedState<STWNetheritePickaxe, string> SpearData = new("Netherite_SpearData", _ => "");
    public static readonly SavedAttachedState<STWNetheritePickaxe, string> ShieldData = new("Netherite_ShieldData", _ => "");

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"res://images/Extension/Relics/{GetType().Name}.png",
        IconOutlinePath: $"res://images/Extension/Relics/{GetType().Name}.png",
        BigIconPath: $"res://images/Extension/Relics/{GetType().Name}.png"
    );

    public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
    {
        if (player != Owner) return false;
        options.Add(new CraftRestSiteOption(player));
        return true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new ComputedDynamicVar("Materials", Materials[this], _ => Materials[this])
    ];

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        Materials[this] += 12;
        DynamicVars["Materials"].BaseValue = Materials[this];
    }

    #region IPickaxeRelic
    int IPickaxeRelic.MaterialCount { get => Materials[this]; set { Materials[this] = value; DynamicVars["Materials"].BaseValue = value; } }
    int IPickaxeRelic.UpgradeProgressCount { get => 0; set { } }
    bool IPickaxeRelic.CanConsumeMaterial(int amount) => Materials[this] >= amount;
    void IPickaxeRelic.ConsumeMaterial(int amount) { Materials[this] -= amount; DynamicVars["Materials"].BaseValue = Materials[this]; }
    bool IPickaxeRelic.HasNextTier => false;
    Task<bool> IPickaxeRelic.UpgradeToNextTier() => Task.FromResult(false);
    string IPickaxeRelic.GetCardBindingsData() => CardBindings[this];
    void IPickaxeRelic.SetCardBindingsData(string data) => CardBindings[this] = data;
    void IPickaxeRelic.RefreshDynamicVars() { DynamicVars["Materials"].BaseValue = Materials[this]; }
    #endregion
}
