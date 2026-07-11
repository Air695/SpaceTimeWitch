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
public class STWWoodenPickaxe : TagRelic, IPickaxeRelic
{
    /// <summary>材料量 —— 整局游戏生效，跨越多场战斗保存。</summary>
    public static readonly SavedAttachedState<STWWoodenPickaxe, int> Materials =
        new("Wooden_Materials", _ => 0);

    /// <summary>升级进度 —— 整局游戏生效，跨越多场战斗保存。初始为4，归零后升级为石镐。</summary>
    public static readonly SavedAttachedState<STWWoodenPickaxe, int> UpgradeProgress =
        new("Wooden_UpgradeProgress", _ => 4);

    /// <summary>卡牌绑定（Card1~Card7）—— 序列化字符串，由 PickaxeCardBindings 管理。</summary>
    public static readonly SavedAttachedState<STWWoodenPickaxe, string> CardBindings =
        new("Wooden_CardBindings", _ => PickaxeCardBindings.DefaultsSerialized);

    /// <summary>门槽位数据 —— 记录矿车解锁前的门类型（空字符串=未解锁），跨战斗/SL保存。</summary>
    public static readonly SavedAttachedState<STWWoodenPickaxe, string> DoorData =
        new("Wooden_DoorData", _ => "");

    /// <summary>矛槽位数据 —— 记录解锁状态和等级，跨战斗/SL保存。</summary>
    public static readonly SavedAttachedState<STWWoodenPickaxe, string> SpearData =
        new("Wooden_SpearData", _ => "");

    /// <summary>盾槽位数据 —— 记录解锁状态和永久禁用标志，跨战斗/SL保存。</summary>
    public static readonly SavedAttachedState<STWWoodenPickaxe, string> ShieldData =
        new("Wooden_ShieldData", _ => "");

    public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
    {
        if (player != Owner) return false;
        options.Add(new CraftRestSiteOption(player));
        return true;
    }

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"res://images/Extension/Relics/{GetType().Name}.png",
        IconOutlinePath: $"res://images/Extension/Relics/{GetType().Name}.png",
        BigIconPath: $"res://images/Extension/Relics/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new ComputedDynamicVar("Materials", Materials[this], _ => Materials[this]),
        new ComputedDynamicVar("UpgradeProgress", UpgradeProgress[this], _ => UpgradeProgress[this])
    ];

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        // 每场战斗胜利后：+2 材料，-1 升级进度
        Materials[this] += 2;
        UpgradeProgress[this]--;

        DynamicVars["Materials"].BaseValue = Materials[this];
        DynamicVars["UpgradeProgress"].BaseValue = UpgradeProgress[this];

        // 进度归零 → 升级
        if (UpgradeProgress[this] <= 0)
            await ((IPickaxeRelic)this).UpgradeToNextTier();
    }

    #region IPickaxeRelic

    int IPickaxeRelic.MaterialCount
    {
        get => Materials[this];
        set { Materials[this] = value; DynamicVars["Materials"].BaseValue = value; }
    }

    int IPickaxeRelic.UpgradeProgressCount
    {
        get => UpgradeProgress[this];
        set { UpgradeProgress[this] = value; DynamicVars["UpgradeProgress"].BaseValue = value; }
    }

    bool IPickaxeRelic.CanConsumeMaterial(int amount) => Materials[this] >= amount;

    void IPickaxeRelic.ConsumeMaterial(int amount)
    {
        Materials[this] -= amount;
        DynamicVars["Materials"].BaseValue = Materials[this];
    }

    bool IPickaxeRelic.HasNextTier => true;

    async Task<bool> IPickaxeRelic.UpgradeToNextTier()
    {
        var nextId = ModelDb.GetId(typeof(STWStonePickaxe));
        var next = (IPickaxeRelic)ModelDb.GetById<RelicModel>(nextId).ToMutable();
        next.MaterialCount = ((IPickaxeRelic)this).MaterialCount;
        next.SetCardBindingsData(((IPickaxeRelic)this).GetCardBindingsData());
        STWStonePickaxe.DoorData[(STWStonePickaxe)next] = DoorData[this];
        STWStonePickaxe.SpearData[(STWStonePickaxe)next] = SpearData[this];
        STWStonePickaxe.ShieldData[(STWStonePickaxe)next] = ShieldData[this];
        await RelicCmd.Replace(this, (RelicModel)next);
        return true;
    }

    string IPickaxeRelic.GetCardBindingsData() => CardBindings[this];

    void IPickaxeRelic.SetCardBindingsData(string data) => CardBindings[this] = data;

    void IPickaxeRelic.RefreshDynamicVars()
    {
        DynamicVars["Materials"].BaseValue = Materials[this];
        DynamicVars["UpgradeProgress"].BaseValue = UpgradeProgress[this];
    }

    #endregion
}
