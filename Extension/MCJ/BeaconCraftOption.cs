using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Runs;

namespace SpaceTimeWitch.Extension.MCJ;

public static class BeaconCraftSubMenu
{
    public static void Show(Player player, int slot, Type currentType)
    {
        var options = (List<RestSiteOption>)RunManager.Instance.RestSiteSynchronizer
            .GetOptionsForPlayer(player);
        var data = BeaconCraftRegistry.GetData(currentType);

        options.Clear();
        if (data?.NextTier != null)
            options.Add(new BeaconUpgradeOption(player, slot, data.NextTier, data.UpgradeCost));
        options.Add(new WeaponReturnOption(player, CraftCraftOption.GetCraftGroup(slot)));
    }
}

public class BeaconUpgradeOption : RestSiteOption, ICustomRestSiteIcon
{
    private readonly int _slot; private readonly Type _toType; private readonly int _cost; private readonly bool _enabled; private readonly string _cn;

    public override string OptionId => "BEACON_UPGRADE";
    public override LocString Description { get { var d = new LocString("cards", "STW_REST_BEACON_UPGRADE_DESC"); d.Add("Cost", _cost); d.Add("Card", new LocString("cards", $"{_cn}.title")); d.Add("CardDesc", CardDescriptionHelper.GetCardDescription(_toType)); return d; } }
    public LocString CustomTitle => new("cards", "STW_REST_BEACON_UPGRADE_TITLE");
    public bool CustomIsEnabled => _enabled;

    public BeaconUpgradeOption(Player owner, int slot, Type toType, int cost) : base(owner)
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
        var b = PickaxeCardBindings.Deserialize(p.GetCardBindingsData());
        b.SetSlot(_slot, _toType);
        p.SetCardBindingsData(b.Serialize());
        return true;
    }

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default) { BeaconCraftSubMenu.Show(Owner, _slot, _toType); return Task.CompletedTask; }
}
