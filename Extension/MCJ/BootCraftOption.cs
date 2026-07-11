using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Runs;

namespace SpaceTimeWitch.Extension.MCJ;

/// <summary>
/// 靴的制作子菜单 —— 升级 / 替换为胸甲(免费) / 返回。
/// 胸甲不可升级，仅可替换回靴。
/// </summary>
public static class BootCraftSubMenu
{
    public static void Show(Player player, int slot, Type currentType)
    {
        var options = (List<RestSiteOption>)RunManager.Instance.RestSiteSynchronizer
            .GetOptionsForPlayer(player);
        var data = BootCraftRegistry.GetData(currentType);

        options.Clear();

        if (data?.NextTier != null)
            options.Add(new BootUpgradeOption(player, slot, data.NextTier, data.UpgradeCost));

        if (data?.Chestplate != null)
            options.Add(new BootReplaceOption(player, slot, data.Chestplate));

        options.Add(new WeaponReturnOption(player, CraftCraftOption.GetCraftGroup(slot)));
    }
}

public class BootUpgradeOption : RestSiteOption, ICustomRestSiteIcon
{
    private readonly int _slot;
    private readonly Type _toType;
    private readonly int _cost;
    private readonly bool _enabled;
    private readonly string _cardName;

    public override string OptionId => "BOOT_UPGRADE";
    public override LocString Description
    {
        get { var d = new LocString("cards", "STW_REST_BOOT_UPGRADE_DESC"); d.Add("Cost", _cost); d.Add("Card", new LocString("cards", $"{_cardName}.title")); d.Add("CardDesc", CardDescriptionHelper.GetCardDescription(_toType)); return d; }
    }
    public LocString CustomTitle => new("cards", "STW_REST_BOOT_UPGRADE_TITLE");
    public bool CustomIsEnabled => _enabled;

    public BootUpgradeOption(Player owner, int slot, Type toType, int cost) : base(owner)
    {
        _slot = slot; _toType = toType; _cost = cost;
        _cardName = MegaCrit.Sts2.Core.Models.ModelDb.GetId(toType).ToString().Replace("CARD.", "");
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

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default) { BootCraftSubMenu.Show(Owner, _slot, _toType); return Task.CompletedTask; }

    private static void UpdateBinding(IPickaxeRelic p, int slot, Type t)
    {
        var b = PickaxeCardBindings.Deserialize(p.GetCardBindingsData());
        b.SetSlot(slot, t);
        p.SetCardBindingsData(b.Serialize());
    }
}

public class BootReplaceOption : RestSiteOption, ICustomRestSiteIcon
{
    private readonly int _slot;
    private readonly Type _toType;
    private readonly string _cardName;

    public override string OptionId => "BOOT_REPLACE";
    public override LocString Description
    {
        get { var d = new LocString("cards", "STW_REST_BOOT_REPLACE_DESC"); d.Add("Card", new LocString("cards", $"{_cardName}.title")); d.Add("CardDesc", CardDescriptionHelper.GetCardDescription(_toType)); return d; }
    }
    public LocString CustomTitle => new("cards", "STW_REST_BOOT_REPLACE_TITLE");
    public bool CustomIsEnabled => true;

    public BootReplaceOption(Player owner, int slot, Type toType) : base(owner)
    {
        _slot = slot; _toType = toType;
        _cardName = MegaCrit.Sts2.Core.Models.ModelDb.GetId(toType).ToString().Replace("CARD.", "");
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

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default) { BootCraftSubMenu.Show(Owner, _slot, _toType); return Task.CompletedTask; }
}
