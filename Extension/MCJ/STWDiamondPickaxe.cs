using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Relics;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace SpaceTimeWitch.Extension.MCJ;

[RegisterRelic(typeof(SpaceTimeWitchExRelicPool))]
public class STWDiamondPickaxe : TagRelic, IPickaxeRelic
{
    public static readonly SavedAttachedState<STWDiamondPickaxe, int> Materials = new("Diamond_Materials", _ => 0);
    public static readonly SavedAttachedState<STWDiamondPickaxe, int> UpgradeProgress = new("Diamond_UpgradeProgress", _ => 10);
    public static readonly SavedAttachedState<STWDiamondPickaxe, string> CardBindings = new("Diamond_CardBindings", _ => PickaxeCardBindings.DefaultsSerialized);
    public static readonly SavedAttachedState<STWDiamondPickaxe, string> DoorData = new("Diamond_DoorData", _ => "");
    public static readonly SavedAttachedState<STWDiamondPickaxe, string> SpearData = new("Diamond_SpearData", _ => "");
    public static readonly SavedAttachedState<STWDiamondPickaxe, string> ShieldData = new("Diamond_ShieldData", _ => "");

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
        new ComputedDynamicVar("Materials", Materials[this], _ => Materials[this]),
        new ComputedDynamicVar("UpgradeProgress", UpgradeProgress[this], _ => UpgradeProgress[this])
    ];

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        Materials[this] += 8;
        UpgradeProgress[this]--;
        DynamicVars["Materials"].BaseValue = Materials[this];
        DynamicVars["UpgradeProgress"].BaseValue = UpgradeProgress[this];
        if (UpgradeProgress[this] <= 0)
            await ((IPickaxeRelic)this).UpgradeToNextTier();
    }

    #region IPickaxeRelic
    int IPickaxeRelic.MaterialCount { get => Materials[this]; set { Materials[this] = value; DynamicVars["Materials"].BaseValue = value; } }
    int IPickaxeRelic.UpgradeProgressCount { get => UpgradeProgress[this]; set { UpgradeProgress[this] = value; DynamicVars["UpgradeProgress"].BaseValue = value; } }
    bool IPickaxeRelic.CanConsumeMaterial(int amount) => Materials[this] >= amount;
    void IPickaxeRelic.ConsumeMaterial(int amount) { Materials[this] -= amount; DynamicVars["Materials"].BaseValue = Materials[this]; }
    bool IPickaxeRelic.HasNextTier => true;

    async Task<bool> IPickaxeRelic.UpgradeToNextTier()
    {
        var nextId = ModelDb.GetId(typeof(STWNetheritePickaxe));
        var next = (IPickaxeRelic)ModelDb.GetById<RelicModel>(nextId).ToMutable();
        next.MaterialCount = ((IPickaxeRelic)this).MaterialCount;
        next.SetCardBindingsData(((IPickaxeRelic)this).GetCardBindingsData());
        if (next is STWNetheritePickaxe n)
        {
            STWNetheritePickaxe.DoorData[n] = DoorData[this];
            STWNetheritePickaxe.SpearData[n] = SpearData[this];
            STWNetheritePickaxe.ShieldData[n] = ShieldData[this];
        }
        await RelicCmd.Replace(this, (RelicModel)next);
        return true;
    }

    string IPickaxeRelic.GetCardBindingsData() => CardBindings[this];
    void IPickaxeRelic.SetCardBindingsData(string data) => CardBindings[this] = data;
    void IPickaxeRelic.RefreshDynamicVars() { DynamicVars["Materials"].BaseValue = Materials[this]; DynamicVars["UpgradeProgress"].BaseValue = UpgradeProgress[this]; }
    #endregion
}
