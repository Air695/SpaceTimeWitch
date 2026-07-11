using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Runs;

namespace SpaceTimeWitch.Extension.MCJ;

public class CraftRestSiteOption : RestSiteOption, ICustomRestSiteIcon
{
    public override string OptionId => "CRAFT";
    public override LocString Description => new("cards", "STW_REST_CRAFT_DESC");
    public LocString CustomTitle => new("cards", "STW_REST_CRAFT_TITLE");
    public bool CustomIsEnabled => true;

    public CraftRestSiteOption(Player owner) : base(owner) { }

    public override Task<bool> OnSelect() => Task.FromResult(true);

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default)
    {
        ShowCraftSubMenu(Owner);
        return Task.CompletedTask;
    }

    internal static void ShowCraftSubMenu(Player player)
    {
        var options = (List<RestSiteOption>)RunManager.Instance.RestSiteSynchronizer.GetOptionsForPlayer(player);
        var pickaxe = player.Relics.OfType<IPickaxeRelic>().FirstOrDefault();
        options.Clear();
        if (pickaxe != null && pickaxe.HasNextTier)
            options.Add(new UpgradePickaxeOption(player));
        options.Add(new CraftCraftOption(player, 1));
        options.Add(new CraftCraftOption(player, 2));
        options.Add(new CraftReturnOption(player));
    }
}

public class CraftReturnOption : RestSiteOption, ICustomRestSiteIcon
{
    public override string OptionId => "CRAFT_RETURN";
    public override LocString Description => new("cards", "STW_REST_CRAFT_RETURN_DESC");
    public LocString CustomTitle => new("cards", "STW_REST_CRAFT_RETURN_TITLE");
    public bool CustomIsEnabled => true;

    public CraftReturnOption(Player owner) : base(owner) { }

    public override Task<bool> OnSelect() => Task.FromResult(true);

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default)
    {
        var options = (List<RestSiteOption>)RunManager.Instance.RestSiteSynchronizer
            .GetOptionsForPlayer(Owner);
        options.Clear();
        options.AddRange(Generate(Owner));
        return Task.CompletedTask;
    }
}
