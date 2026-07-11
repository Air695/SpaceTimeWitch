using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Runs;
using SpaceTimeWitch.Extension.MCJ.Card5;

namespace SpaceTimeWitch.Extension.MCJ;

/// <summary>
/// 矛的制作子菜单 —— 升级 / 解锁三叉戟 / 解锁重锤 / 返回。
/// 解锁消耗材料（各 15），解锁后替换免费。等级记忆跨战斗/SL 保留。
/// SpearData 格式: "{preSpearType}:{unlockMask}" (1=Trident, 2=Mace, 3=Both)
/// </summary>
public static class SpearCraftSubMenu
{
    public static void Show(Player player, int slot, Type currentType)
    {
        var options = (List<RestSiteOption>)RunManager.Instance.RestSiteSynchronizer
            .GetOptionsForPlayer(player);
        var pickaxe = player.Relics.OfType<IPickaxeRelic>().FirstOrDefault();
        var sd = SpearDataHelper.Get(pickaxe);

        options.Clear();

        bool isSpecial = currentType == typeof(STWTrident) || currentType == typeof(STWMace);

        if (!isSpecial)
        {
            var data = SpearCraftRegistry.GetData(currentType);
            if (data?.NextTier != null)
                options.Add(new SpearUpgradeOption(player, slot, data.NextTier, data.UpgradeCost));

            // 三叉戟
            bool tridentUnlocked = (sd.unlockMask & SpearCraftRegistry.UnlockFlagTrident) != 0;
            int tCost = tridentUnlocked ? 0 : SpearCraftRegistry.TridentUnlockCost;
            options.Add(new SpearSpecialOption(player, slot, typeof(STWTrident), tCost, !tridentUnlocked, "TRIDENT"));

            // 重锤
            bool maceUnlocked = (sd.unlockMask & SpearCraftRegistry.UnlockFlagMace) != 0;
            int mCost = maceUnlocked ? 0 : SpearCraftRegistry.MaceUnlockCost;
            options.Add(new SpearSpecialOption(player, slot, typeof(STWMace), mCost, !maceUnlocked, "MACE"));
        }
        else
        {
            // 特殊武器 → 替换回矛（免费，恢复等级）
            int tier = sd.tier;
            var spearType = SpearCraftRegistry.GetSpearAtTier(tier);
            if (spearType != null)
                options.Add(new SpearReplaceOption(player, slot, spearType));
        }

        options.Add(new WeaponReturnOption(player, CraftCraftOption.GetCraftGroup(slot)));
    }
}

internal static class SpearDataHelper
{
    public static (int tier, int unlockMask) Get(IPickaxeRelic? pickaxe)
    {
        if (pickaxe == null) return (0, 0);
        var raw = GetRaw(pickaxe);
        if (string.IsNullOrEmpty(raw)) return (0, 0);
        var parts = raw.Split(':');
        int tier = parts.Length > 0 && int.TryParse(parts[0], out var t) ? t : 0;
        int mask = parts.Length > 1 && int.TryParse(parts[1], out var m) ? m : 0;
        return (tier, mask);
    }

    private static string GetRaw(IPickaxeRelic p) => p switch
    {
        STWWoodenPickaxe w => STWWoodenPickaxe.SpearData[w],
        STWStonePickaxe s => STWStonePickaxe.SpearData[s],
        STWIronPickaxe i => STWIronPickaxe.SpearData[i],
        STWDiamondPickaxe d => STWDiamondPickaxe.SpearData[d],
        STWNetheritePickaxe n => STWNetheritePickaxe.SpearData[n],
        _ => "",
    };

    private static void SetRaw(IPickaxeRelic p, string value)
    {
        switch (p)
        {
            case STWWoodenPickaxe w: STWWoodenPickaxe.SpearData[w] = value; break;
            case STWStonePickaxe s: STWStonePickaxe.SpearData[s] = value; break;
            case STWIronPickaxe i: STWIronPickaxe.SpearData[i] = value; break;
            case STWDiamondPickaxe d: STWDiamondPickaxe.SpearData[d] = value; break;
            case STWNetheritePickaxe n: STWNetheritePickaxe.SpearData[n] = value; break;
        }
    }

    public static void MarkUnlocked(IPickaxeRelic pickaxe, int tier, int flag)
    {
        var (_, mask) = Get(pickaxe);
        mask |= flag;
        SetRaw(pickaxe, $"{tier}:{mask}");
    }

    public static void SetTier(IPickaxeRelic pickaxe, int tier)
    {
        var (_, mask) = Get(pickaxe);
        SetRaw(pickaxe, $"{tier}:{mask}");
    }

}

// ── 升级 ──

public class SpearUpgradeOption : RestSiteOption, ICustomRestSiteIcon
{
    private readonly int _slot; private readonly Type _toType; private readonly int _cost; private readonly bool _enabled; private readonly string _cn;

    public override string OptionId => "SPEAR_UPGRADE";
    public override LocString Description { get { var d = new LocString("cards", "STW_REST_SPEAR_UPGRADE_DESC"); d.Add("Cost", _cost); d.Add("Card", new LocString("cards", $"{_cn}.title")); d.Add("CardDesc", CardDescriptionHelper.GetCardDescription(_toType)); return d; } }
    public LocString CustomTitle => new("cards", "STW_REST_SPEAR_UPGRADE_TITLE");
    public bool CustomIsEnabled => _enabled;

    public SpearUpgradeOption(Player owner, int slot, Type toType, int cost) : base(owner)
    {
        _slot = slot; _toType = toType; _cost = cost; _cn = MegaCrit.Sts2.Core.Models.ModelDb.GetId(toType).ToString().Replace("CARD.", "");
        var p = owner.Relics.OfType<IPickaxeRelic>().FirstOrDefault();
        _enabled = p != null && p.MaterialCount >= cost;
    }

    public override async Task<bool> OnSelect()
    {
        var p = Owner.Relics.OfType<IPickaxeRelic>().FirstOrDefault();
        if (p == null || p.MaterialCount < _cost) return false;
        p.ConsumeMaterial(_cost);
        UpdateBinding(p, _slot, _toType);
        return true;
    }

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default) { SpearCraftSubMenu.Show(Owner, _slot, _toType); return Task.CompletedTask; }

    private static void UpdateBinding(IPickaxeRelic p, int slot, Type t)
    {
        var b = PickaxeCardBindings.Deserialize(p.GetCardBindingsData());
        b.SetSlot(slot, t);
        p.SetCardBindingsData(b.Serialize());
    }
}

// ── 解锁/替换特殊武器 ──

public class SpearSpecialOption : RestSiteOption, ICustomRestSiteIcon
{
    private readonly int _slot; private readonly Type _toType; private readonly int _cost;
    private readonly bool _isFirstUnlock; private readonly string _label; private readonly bool _enabled; private readonly string _cn;

    public override string OptionId => $"SPEAR_SPECIAL_{_label}";
    public override LocString Description { get { var d = new LocString("cards", "STW_REST_SPEAR_SPECIAL_DESC"); d.Add("Cost", _cost); d.Add("Card", new LocString("cards", $"{_cn}.title")); d.Add("CardDesc", CardDescriptionHelper.GetCardDescription(_toType)); return d; } }
    public LocString CustomTitle => new("cards", "STW_REST_SPEAR_SPECIAL_TITLE");
    public bool CustomIsEnabled => _enabled;

    public SpearSpecialOption(Player owner, int slot, Type toType, int cost, bool isFirstUnlock, string label) : base(owner)
    {
        _slot = slot; _toType = toType; _cost = cost; _isFirstUnlock = isFirstUnlock; _label = label; _cn = MegaCrit.Sts2.Core.Models.ModelDb.GetId(toType).ToString().Replace("CARD.", "");
        var p = owner.Relics.OfType<IPickaxeRelic>().FirstOrDefault();
        _enabled = p != null && p.MaterialCount >= cost;
    }

    public override async Task<bool> OnSelect()
    {
        var p = Owner.Relics.OfType<IPickaxeRelic>().FirstOrDefault();
        if (p == null || p.MaterialCount < _cost) return false;
        if (_cost > 0) p.ConsumeMaterial(_cost);

        int flag = _toType == typeof(STWTrident) ? SpearCraftRegistry.UnlockFlagTrident : SpearCraftRegistry.UnlockFlagMace;
        if (_isFirstUnlock)
        {
            var b = PickaxeCardBindings.Deserialize(p.GetCardBindingsData());
            var currentType = b.GetSlot(_slot);
            int tier = currentType != null ? SpearCraftRegistry.GetTier(currentType) : 0;
            SpearDataHelper.MarkUnlocked(p, tier, flag);
        }

        var b2 = PickaxeCardBindings.Deserialize(p.GetCardBindingsData());
        b2.SetSlot(_slot, _toType);
        p.SetCardBindingsData(b2.Serialize());

        return true;
    }

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default) { SpearCraftSubMenu.Show(Owner, _slot, _toType); return Task.CompletedTask; }
}

// ── 从特殊武器替换回矛（免费）──

public class SpearReplaceOption : RestSiteOption, ICustomRestSiteIcon
{
    private readonly int _slot; private readonly Type _toType; private readonly string _cn;

    public override string OptionId => "SPEAR_REPLACE";
    public override LocString Description { get { var d = new LocString("cards", "STW_REST_SPEAR_REPLACE_DESC"); d.Add("Card", new LocString("cards", $"{_cn}.title")); d.Add("CardDesc", CardDescriptionHelper.GetCardDescription(_toType)); return d; } }
    public LocString CustomTitle => new("cards", "STW_REST_SPEAR_REPLACE_TITLE");
    public bool CustomIsEnabled => true;

    public SpearReplaceOption(Player owner, int slot, Type toType) : base(owner)
    {
        _slot = slot; _toType = toType; _cn = MegaCrit.Sts2.Core.Models.ModelDb.GetId(toType).ToString().Replace("CARD.", "");
    }

    public override Task<bool> OnSelect()
    {
        var p = Owner.Relics.OfType<IPickaxeRelic>().FirstOrDefault();
        if (p == null) return Task.FromResult(false);
        var b = PickaxeCardBindings.Deserialize(p.GetCardBindingsData());
        b.SetSlot(_slot, _toType);
        p.SetCardBindingsData(b.Serialize());
        return Task.FromResult(true);
    }

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default) { SpearCraftSubMenu.Show(Owner, _slot, _toType); return Task.CompletedTask; }
}
