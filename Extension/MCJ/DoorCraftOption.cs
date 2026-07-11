using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Runs;
using SpaceTimeWitch.Extension.MCJ.Card3;

namespace SpaceTimeWitch.Extension.MCJ;

public static class DoorCraftSubMenu
{
    internal static void Show(Player player, int slot, Type currentType)
    {
        var options = (List<RestSiteOption>)RunManager.Instance.RestSiteSynchronizer.GetOptionsForPlayer(player);
        var pickaxe = player.Relics.OfType<IPickaxeRelic>().FirstOrDefault();
        options.Clear();
        bool isMinecart = currentType == typeof(STWMinecart);

        if (!isMinecart)
        {
            var data = DoorCraftRegistry.GetData(currentType);
            if (data?.NextTier != null)
                options.Add(new DoorUpgradeOption(player, slot, data.NextTier, data.UpgradeCost));
            if (data?.DoorVariant != null)
                options.Add(new DoorReplaceOption(player, slot, data.DoorVariant, "DOOR"));
            if (data?.TrapdoorVariant != null)
                options.Add(new DoorReplaceOption(player, slot, data.TrapdoorVariant, "TRAPDOOR"));
            bool unlocked = GetMinecartUnlocked(pickaxe);
            int cost = unlocked ? 0 : DoorCraftRegistry.MinecartUnlockCost;
            options.Add(new DoorMinecartOption(player, slot, pickaxe != null && pickaxe.MaterialCount >= cost, unlocked));
        }
        else
        {
            int tier = GetSavedTier(pickaxe);
            var doorType = DoorCraftRegistry.GetDoorAtTier(tier);
            var trapdoorType = DoorCraftRegistry.GetTrapdoorAtTier(tier);
            if (doorType != null) options.Add(new DoorReplaceOption(player, slot, doorType, "DOOR"));
            if (trapdoorType != null) options.Add(new DoorReplaceOption(player, slot, trapdoorType, "TRAPDOOR"));
        }
        options.Add(new WeaponReturnOption(player, CraftCraftOption.GetCraftGroup(slot)));
    }

    internal static bool GetMinecartUnlocked(IPickaxeRelic? p) { if (p == null) return false; return !string.IsNullOrEmpty(DoorDataFor(p)); }
    internal static void MarkMinecartUnlocked(IPickaxeRelic p, Type pre) => SetDoorData(p, pre.FullName!);
    internal static int GetSavedTier(IPickaxeRelic? p)
    {
        if (p == null) return 0;
        var name = DoorDataFor(p);
        if (!string.IsNullOrEmpty(name)) { var t = Type.GetType(name); if (t != null) { var d = DoorCraftRegistry.GetData(t); if (d != null) return d.Tier; } }
        return 0;
    }
    private static string DoorDataFor(IPickaxeRelic p) => p switch { STWWoodenPickaxe w => STWWoodenPickaxe.DoorData[w], STWStonePickaxe s => STWStonePickaxe.DoorData[s], STWIronPickaxe i => STWIronPickaxe.DoorData[i], STWDiamondPickaxe d => STWDiamondPickaxe.DoorData[d], STWNetheritePickaxe n => STWNetheritePickaxe.DoorData[n], _ => "" };
    private static void SetDoorData(IPickaxeRelic p, string v) { switch (p) { case STWWoodenPickaxe w: STWWoodenPickaxe.DoorData[w] = v; break; case STWStonePickaxe s: STWStonePickaxe.DoorData[s] = v; break; case STWIronPickaxe i: STWIronPickaxe.DoorData[i] = v; break; case STWDiamondPickaxe d: STWDiamondPickaxe.DoorData[d] = v; break; case STWNetheritePickaxe n: STWNetheritePickaxe.DoorData[n] = v; break; } }
}

public class DoorUpgradeOption : RestSiteOption, ICustomRestSiteIcon
{
    private readonly int _slot; private readonly Type _toType; private readonly int _cost; private readonly bool _enabled; private readonly string _cn;
    public override string OptionId => "DOOR_UPGRADE";
    public override LocString Description { get { var d = new LocString("cards", "STW_REST_DOOR_UPGRADE_DESC"); d.Add("Cost", _cost); d.Add("Card", new LocString("cards", $"{_cn}.title")); d.Add("CardDesc", CardDescriptionHelper.GetCardDescription(_toType)); return d; } }
    public LocString CustomTitle => new("cards", "STW_REST_DOOR_UPGRADE_TITLE");
    public bool CustomIsEnabled => _enabled;

    public DoorUpgradeOption(Player owner, int slot, Type toType, int cost) : base(owner)
    { _slot = slot; _toType = toType; _cost = cost; _cn = MegaCrit.Sts2.Core.Models.ModelDb.GetId(toType).ToString().Replace("CARD.", ""); var p = owner.Relics.OfType<IPickaxeRelic>().FirstOrDefault(); _enabled = p != null && p.MaterialCount >= cost; }

    public override async Task<bool> OnSelect()
    {
        var p = Owner.Relics.OfType<IPickaxeRelic>().FirstOrDefault();
        if (p == null || p.MaterialCount < _cost) return false;
        p.ConsumeMaterial(_cost);
        var b = PickaxeCardBindings.Deserialize(p.GetCardBindingsData()); b.SetSlot(_slot, _toType); p.SetCardBindingsData(b.Serialize());
        return true;
    }

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default) { DoorCraftSubMenu.Show(Owner, _slot, _toType); return Task.CompletedTask; }
}

public class DoorReplaceOption : RestSiteOption, ICustomRestSiteIcon
{
    private readonly int _slot; private readonly Type _toType; private readonly string _label; private readonly string _cn;
    public override string OptionId => $"DOOR_REPLACE_{_label}";
    public override LocString Description { get { var d = new LocString("cards", "STW_REST_DOOR_REPLACE_DESC"); d.Add("Card", new LocString("cards", $"{_cn}.title")); d.Add("CardDesc", CardDescriptionHelper.GetCardDescription(_toType)); return d; } }
    public LocString CustomTitle => new("cards", "STW_REST_DOOR_REPLACE_TITLE");
    public bool CustomIsEnabled => true;

    public DoorReplaceOption(Player owner, int slot, Type toType, string label) : base(owner) { _slot = slot; _toType = toType; _label = label; _cn = MegaCrit.Sts2.Core.Models.ModelDb.GetId(toType).ToString().Replace("CARD.", ""); }

    public override Task<bool> OnSelect()
    {
        var p = Owner.Relics.OfType<IPickaxeRelic>().FirstOrDefault();
        if (p == null) return Task.FromResult(false);
        var b = PickaxeCardBindings.Deserialize(p.GetCardBindingsData()); b.SetSlot(_slot, _toType); p.SetCardBindingsData(b.Serialize());
        return Task.FromResult(true);
    }

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default) { DoorCraftSubMenu.Show(Owner, _slot, _toType); return Task.CompletedTask; }
}

public class DoorMinecartOption : RestSiteOption, ICustomRestSiteIcon
{
    private readonly int _slot; private readonly bool _enabled; private readonly bool _alreadyUnlocked; private readonly int _cost;
    public override string OptionId => "DOOR_MINECART";
    public override LocString Description { get { var d = new LocString("cards", "STW_REST_DOOR_MINECART_DESC"); d.Add("Cost", _cost); return d; } }
    public LocString CustomTitle => new("cards", "STW_REST_DOOR_MINECART_TITLE");
    public bool CustomIsEnabled => _enabled;

    public DoorMinecartOption(Player owner, int slot, bool canAfford, bool alreadyUnlocked) : base(owner)
    { _slot = slot; _enabled = canAfford; _alreadyUnlocked = alreadyUnlocked; _cost = alreadyUnlocked ? 0 : DoorCraftRegistry.MinecartUnlockCost; }

    public override async Task<bool> OnSelect()
    {
        var p = Owner.Relics.OfType<IPickaxeRelic>().FirstOrDefault();
        if (p == null || p.MaterialCount < _cost) return false;
        if (_cost > 0) p.ConsumeMaterial(_cost);
        var b = PickaxeCardBindings.Deserialize(p.GetCardBindingsData());
        var ct = b.GetSlot(_slot);
        if (!_alreadyUnlocked && ct != null) DoorCraftSubMenu.MarkMinecartUnlocked(p, ct);
        b.SetSlot(_slot, typeof(STWMinecart)); p.SetCardBindingsData(b.Serialize());
        return true;
    }

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default) { DoorCraftSubMenu.Show(Owner, _slot, typeof(STWMinecart)); return Task.CompletedTask; }
}
