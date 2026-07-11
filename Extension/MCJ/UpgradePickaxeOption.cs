using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Runs;

namespace SpaceTimeWitch.Extension.MCJ;

public class UpgradePickaxeOption : RestSiteOption, ICustomRestSiteIcon
{
    public override string OptionId => "CRAFT_UPGRADE";

    public override LocString Description => new("cards", "STW_REST_CRAFT_UPGRADE_DESC");
    public LocString CustomTitle => new("cards", "STW_REST_CRAFT_UPGRADE_TITLE");
    public bool CustomIsEnabled => true;

    public UpgradePickaxeOption(Player owner) : base(owner) { }

    public override Task<bool> OnSelect() => Task.FromResult(true);

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default)
    {
        UpgradeSubMenu(Owner);
        return Task.CompletedTask;
    }

    internal static void UpgradeSubMenu(Player player)
    {
        var options = (List<RestSiteOption>)RunManager.Instance.RestSiteSynchronizer
            .GetOptionsForPlayer(player);
        options.Clear();
        options.Add(new IncreaseProgressOption(player));
        options.Add(new DirectUpgradeOption(player));
        options.Add(new ReturnToCraftOption(player));
    }
}

internal static class SubMenuHelper
{
    public static void ShowUpgrade(Player player) => UpgradePickaxeOption.UpgradeSubMenu(player);
}

public class IncreaseProgressOption : RestSiteOption, ICustomRestSiteIcon
{
    public override string OptionId => "CRAFT_UPGRADE_PROGRESS";

    public override LocString Description => new("cards", "STW_REST_UPGRADE_PROGRESS_DESC");
    public LocString CustomTitle => new("cards", "STW_REST_UPGRADE_PROGRESS_TITLE");

    private readonly bool _enabled;

    public bool CustomIsEnabled => _enabled;

    public IncreaseProgressOption(Player owner) : base(owner)
    {
        var pickaxe = owner.Relics.OfType<IPickaxeRelic>().FirstOrDefault();
        _enabled = pickaxe != null
            && pickaxe.HasNextTier
            && pickaxe.MaterialCount >= 2;
    }

    public override async Task<bool> OnSelect()
    {
        var pickaxe = Owner.Relics.OfType<IPickaxeRelic>().FirstOrDefault();
        if (pickaxe == null || !pickaxe.HasNextTier || pickaxe.MaterialCount < 2)
            return false;

        pickaxe.ConsumeMaterial(2);
        pickaxe.UpgradeProgressCount--;

        if (pickaxe.UpgradeProgressCount <= 0)
            await pickaxe.UpgradeToNextTier();

        return true;
    }

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default)
    {
        UpgradePickaxeOption.UpgradeSubMenu(Owner);
        return Task.CompletedTask;
    }
}

public class DirectUpgradeOption : RestSiteOption, ICustomRestSiteIcon
{
    public override string OptionId => "CRAFT_UPGRADE_DIRECT";

    public override LocString Description
    {
        get { var d = new LocString("cards", "STW_REST_UPGRADE_DIRECT_DESC"); d.Add("Cost", _cost); return d; }
    }
    public LocString CustomTitle => new("cards", "STW_REST_UPGRADE_DIRECT_TITLE");

    private readonly bool _enabled;
    private readonly int _cost;

    public bool CustomIsEnabled => _enabled;

    public DirectUpgradeOption(Player owner) : base(owner)
    {
        var pickaxe = owner.Relics.OfType<IPickaxeRelic>().FirstOrDefault();
        if (pickaxe != null && pickaxe.HasNextTier)
        {
            _cost = pickaxe.UpgradeProgressCount * 2;
            _enabled = pickaxe.MaterialCount >= _cost;
        }
    }

    public override async Task<bool> OnSelect()
    {
        var pickaxe = Owner.Relics.OfType<IPickaxeRelic>().FirstOrDefault();
        if (pickaxe == null || !pickaxe.HasNextTier)
            return false;

        int cost = pickaxe.UpgradeProgressCount * 2;
        if (pickaxe.MaterialCount < cost)
            return false;

        pickaxe.ConsumeMaterial(cost);
        await pickaxe.UpgradeToNextTier();

        return true;
    }

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default)
    {
        UpgradePickaxeOption.UpgradeSubMenu(Owner);
        return Task.CompletedTask;
    }
}

public class ReturnToCraftOption : RestSiteOption, ICustomRestSiteIcon
{
    public override string OptionId => "CRAFT_UPGRADE_RETURN";

    public override LocString Description => new("cards", "STW_REST_UPGRADE_RETURN_DESC");
    public LocString CustomTitle => new("cards", "STW_REST_UPGRADE_RETURN_TITLE");
    public bool CustomIsEnabled => true;

    public ReturnToCraftOption(Player owner) : base(owner) { }

    public override Task<bool> OnSelect() => Task.FromResult(true);

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default)
    {
        CraftRestSiteOption.ShowCraftSubMenu(Owner);
        return Task.CompletedTask;
    }
}
