using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace SpaceTimeWitch.Extension.MCJ;

/// <summary>
/// ShieldData 格式: "{isDisabled}|{unlockMask}"  (isDisabled: 0/1)
/// </summary>
public static class ShieldCraftSubMenu
{
    public static void Show(Player player, int slot, Type currentType)
    {
        var options = (List<RestSiteOption>)RunManager.Instance.RestSiteSynchronizer
            .GetOptionsForPlayer(player);
        var pickaxe = player.Relics.OfType<IPickaxeRelic>().FirstOrDefault();
        var (isDisabled, unlockMask) = ShieldDataHelper.Get(pickaxe);

        options.Clear();

        foreach (var opt in ShieldCraftRegistry.Options)
        {
            if (opt.Type == currentType) continue; // 跳过当前持有
            int flag = ShieldCraftRegistry.GetFlag(opt.Label);
            bool unlocked = flag == 0 || (unlockMask & flag) != 0;
            int cost = unlocked ? 0 : opt.UnlockCost;
            options.Add(new ShieldReplaceOption(player, slot, opt.Type, cost, !unlocked, opt.Label));
        }

        // Totem of Undying
        var totemType = typeof(Card6.STWTotemUndying);
        if (currentType != totemType)
        {
            var p = pickaxe;
            bool canAfford = p != null && p.MaterialCount >= ShieldCraftRegistry.TotemCost;
            options.Add(new ShieldTotemOption(player, slot, canAfford));
        }

        options.Add(new WeaponReturnOption(player, CraftCraftOption.GetCraftGroup(slot)));
    }

    public static bool IsSlotDisabled(IPickaxeRelic? pickaxe)
    {
        var (isDisabled, _) = ShieldDataHelper.Get(pickaxe);
        return isDisabled;
    }
}

internal static class ShieldDataHelper
{
    public static (bool isDisabled, int unlockMask) Get(IPickaxeRelic? p)
    {
        if (p == null) return (false, 0);
        var raw = GetRaw(p);
        if (string.IsNullOrEmpty(raw)) return (false, 0);
        var parts = raw.Split('|');
        bool disabled = parts.Length > 0 && parts[0] == "1";
        int mask = parts.Length > 1 && int.TryParse(parts[1], out var m) ? m : 0;
        return (disabled, mask);
    }

    private static string GetRaw(IPickaxeRelic p) => p switch
    {
        STWWoodenPickaxe w => STWWoodenPickaxe.ShieldData[w],
        STWStonePickaxe s => STWStonePickaxe.ShieldData[s],
        STWIronPickaxe i => STWIronPickaxe.ShieldData[i],
        STWDiamondPickaxe d => STWDiamondPickaxe.ShieldData[d],
        STWNetheritePickaxe n => STWNetheritePickaxe.ShieldData[n],
        _ => "",
    };

    private static void SetRaw(IPickaxeRelic p, string value)
    {
        switch (p)
        {
            case STWWoodenPickaxe w: STWWoodenPickaxe.ShieldData[w] = value; break;
            case STWStonePickaxe s: STWStonePickaxe.ShieldData[s] = value; break;
            case STWIronPickaxe i: STWIronPickaxe.ShieldData[i] = value; break;
            case STWDiamondPickaxe d: STWDiamondPickaxe.ShieldData[d] = value; break;
            case STWNetheritePickaxe n: STWNetheritePickaxe.ShieldData[n] = value; break;
        }
    }

    public static void SetDisabled(IPickaxeRelic p)
    {
        var (_, mask) = Get(p);
        SetRaw(p, $"1|{mask}");
    }

    public static void MarkUnlocked(IPickaxeRelic p, int flag)
    {
        var (disabled, mask) = Get(p);
        SetRaw(p, $"{(disabled ? 1 : 0)}|{mask | flag}");
    }

}

// ── 替换（解锁后免费）──

public class ShieldReplaceOption : RestSiteOption, ICustomRestSiteIcon
{
    private readonly int _slot; private readonly Type _toType; private readonly int _cost;
    private readonly bool _isFirstUnlock; private readonly string _label; private readonly bool _enabled; private readonly string _cn;

    public override string OptionId => $"SHIELD_{_label}";
    public override LocString Description { get { var d = new LocString("cards", "STW_REST_SHIELD_REPLACE_DESC"); d.Add("Cost", _cost); d.Add("Card", new LocString("cards", $"{_cn}.title")); d.Add("CardDesc", CardDescriptionHelper.GetCardDescription(_toType)); return d; } }
    public LocString CustomTitle => new("cards", $"STW_REST_SHIELD_{_label}_TITLE");
    public bool CustomIsEnabled => _enabled;

    public ShieldReplaceOption(Player owner, int slot, Type toType, int cost, bool isFirstUnlock, string label) : base(owner)
    {
        _slot = slot; _toType = toType; _cost = cost; _isFirstUnlock = isFirstUnlock; _label = label; _cn = ModelDb.GetId(toType).ToString().Replace("CARD.", "");
        var p = owner.Relics.OfType<IPickaxeRelic>().FirstOrDefault();
        _enabled = p != null && p.MaterialCount >= cost;
    }

    public override async Task<bool> OnSelect()
    {
        var p = Owner.Relics.OfType<IPickaxeRelic>().FirstOrDefault();
        if (p == null || p.MaterialCount < _cost) return false;
        if (_cost > 0) p.ConsumeMaterial(_cost);
        if (_isFirstUnlock)
            ShieldDataHelper.MarkUnlocked(p, ShieldCraftRegistry.GetFlag(_label));

        var b = PickaxeCardBindings.Deserialize(p.GetCardBindingsData());
        b.SetSlot(_slot, _toType);
        p.SetCardBindingsData(b.Serialize());
        return true;
    }

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default) { ShieldCraftSubMenu.Show(Owner, _slot, _toType); return Task.CompletedTask; }
}

// ── Totem of Undying ──

public class ShieldTotemOption : RestSiteOption, ICustomRestSiteIcon
{
    private readonly int _slot; private readonly bool _enabled;

    public override string OptionId => "SHIELD_TOTEM";
    public override LocString Description => new("cards", "STW_REST_SHIELD_TOTEM_DESC");
    public LocString CustomTitle => new("cards", "STW_REST_SHIELD_TOTEM_TITLE");
    public bool CustomIsEnabled => _enabled;

    public ShieldTotemOption(Player owner, int slot, bool canAfford) : base(owner)
    {
        _slot = slot;
        _enabled = canAfford;
    }

    public override async Task<bool> OnSelect()
    {
        var p = Owner.Relics.OfType<IPickaxeRelic>().FirstOrDefault();
        if (p == null || p.MaterialCount < ShieldCraftRegistry.TotemCost) return false;
        p.ConsumeMaterial(ShieldCraftRegistry.TotemCost);

        // 解除槽位绑定
        var b = PickaxeCardBindings.Deserialize(p.GetCardBindingsData());
        b.ClearSlot(_slot);
        p.SetCardBindingsData(b.Serialize());

        // 获得遗物
        var totemId = ModelDb.GetId(typeof(Card6.STWTotemUndying));
        var totem = ModelDb.GetById<RelicModel>(totemId).ToMutable();
        await RelicCmd.Obtain(totem, Owner);

        // 永久禁用此槽位
        ShieldDataHelper.SetDisabled(p);
        return true;
    }

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default)
    {
        // 盾槽属于制作2组，返回对应组菜单
        CraftCraftOption.ShowGroupMenu(Owner, 2);
        return Task.CompletedTask;
    }
}
