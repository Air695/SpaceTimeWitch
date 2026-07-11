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
public class STWStonePickaxe : TagRelic, IPickaxeRelic
{
    /// <summary>材料量 —— 整局游戏生效，跨越多场战斗保存。</summary>
    public static readonly SavedAttachedState<STWStonePickaxe, int> Materials =
        new("Stone_Materials", _ => 0);

    /// <summary>升级进度 —— 整局游戏生效，跨越多场战斗保存。初始为10。</summary>
    public static readonly SavedAttachedState<STWStonePickaxe, int> UpgradeProgress =
        new("Stone_UpgradeProgress", _ => 10);

    /// <summary>卡牌绑定（Card1~Card7）—— 序列化字符串，由 PickaxeCardBindings 管理。</summary>
    public static readonly SavedAttachedState<STWStonePickaxe, string> CardBindings =
        new("Stone_CardBindings", _ => PickaxeCardBindings.DefaultsSerialized);

    /// <summary>门槽位数据 —— 记录矿车解锁前的门类型（空字符串=未解锁），跨战斗/SL保存。</summary>
    public static readonly SavedAttachedState<STWStonePickaxe, string> DoorData =
        new("Stone_DoorData", _ => "");

    /// <summary>矛槽位数据 —— 记录解锁状态和等级，跨战斗/SL保存。</summary>
    public static readonly SavedAttachedState<STWStonePickaxe, string> SpearData =
        new("Stone_SpearData", _ => "");

    /// <summary>盾槽位数据 —— 记录解锁状态和永久禁用标志，跨战斗/SL保存。</summary>
    public static readonly SavedAttachedState<STWStonePickaxe, string> ShieldData =
        new("Stone_ShieldData", _ => "");

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
        var nextId = ModelDb.GetId(typeof(STWIronPickaxe));
        var next = (IPickaxeRelic)ModelDb.GetById<RelicModel>(nextId).ToMutable();
        next.MaterialCount = ((IPickaxeRelic)this).MaterialCount;
        next.SetCardBindingsData(((IPickaxeRelic)this).GetCardBindingsData());
        if (next is STWIronPickaxe iron)
        {
            STWIronPickaxe.DoorData[iron] = DoorData[this];
            STWIronPickaxe.SpearData[iron] = SpearData[this];
            STWIronPickaxe.ShieldData[iron] = ShieldData[this];
        }
        await RelicCmd.Replace(this, (RelicModel)next);
        return true;
    }

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        Materials[this] += 3;
        UpgradeProgress[this]--;
        DynamicVars["Materials"].BaseValue = Materials[this];
        DynamicVars["UpgradeProgress"].BaseValue = UpgradeProgress[this];
        if (UpgradeProgress[this] <= 0)
            await ((IPickaxeRelic)this).UpgradeToNextTier();
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
