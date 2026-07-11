using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace SpaceTimeWitch.Extension.MCJ;

public class CraftCraftOption : RestSiteOption, ICustomRestSiteIcon
{
    private readonly int _craftGroup;

    public override string OptionId => $"CRAFT_CRAFT_{_craftGroup}";
    public override LocString Description => new("cards", $"STW_REST_CRAFT_CRAFT{_craftGroup}_DESC");
    public LocString CustomTitle => new("cards", $"STW_REST_CRAFT_CRAFT{_craftGroup}_TITLE");
    public bool CustomIsEnabled => true;

    public CraftCraftOption(Player owner, int craftGroup) : base(owner)
    {
        _craftGroup = craftGroup;
    }

    public override Task<bool> OnSelect() => Task.FromResult(true);

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default)
    {
        ShowGroupMenu(Owner, _craftGroup);
        return Task.CompletedTask;
    }

    /// <summary>根据槽位编号返回所属的制作组（1 或 2）。</summary>
    public static int GetCraftGroup(int slot) => slot <= 2 ? 1 : 2;

    /// <summary>显示指定制作组的槽位列表 + 返回合成选项。</summary>
    public static void ShowGroupMenu(Player player, int craftGroup)
    {
        var options = (List<RestSiteOption>)RunManager.Instance.RestSiteSynchronizer.GetOptionsForPlayer(player);
        var pickaxe = player.Relics.OfType<IPickaxeRelic>().FirstOrDefault();
        options.Clear();
        if (pickaxe == null) { CraftRestSiteOption.ShowCraftSubMenu(player); return; }

        var bindings = PickaxeCardBindings.Deserialize(pickaxe.GetCardBindingsData());
        bool isMulti = player.RunState.Players.Count > 1;

        int start = craftGroup == 1 ? 0 : 3;
        int end = craftGroup == 1 ? 2 : PickaxeCardBindings.SlotCount - 1;

        for (int i = start; i <= end; i++)
        {
            if (i == 6 && !isMulti) continue;
            if (i == 5 && ShieldCraftSubMenu.IsSlotDisabled(pickaxe)) continue;
            var cardType = bindings[i];
            if (cardType != null)
                options.Add(new CraftSlotOption(player, i, cardType));
        }
        options.Add(new CraftGroupReturnOption(player));

        // 同步标签遗物卡池显示（升级/替换后绑定的卡牌可能已变更）
        Nodes.TagRelicPoolManager.RefreshPile(player);
    }
}

public class CraftSlotOption : RestSiteOption, ICustomRestSiteIcon
{
    private readonly int _slot;
    private readonly Type _cardType;
    private readonly string _cardName;

    public override string OptionId => $"CRAFT_SLOT_{_slot + 1}";
    public override LocString Description => CardDescriptionHelper.GetDescriptionLocString(_cardType);
    public LocString CustomTitle => new("cards", $"{_cardName}.title");
    public bool CustomIsEnabled => true;
    public string CustomIconPath => $"res://images/Extension/Cards/{_cardType.Name}.png";

    public CraftSlotOption(Player owner, int slot, Type cardType) : base(owner)
    {
        _slot = slot; _cardType = cardType;
        _cardName = ModelDb.GetId(cardType).ToString().Replace("CARD.", "");
    }

    public override Task<bool> OnSelect() => Task.FromResult(true);

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default)
    {
        if (_slot == 0) ShowWeaponSubMenu(Owner, _slot, _cardType);
        else if (_slot == 2) DoorCraftSubMenu.Show(Owner, _slot, _cardType);
        else if (_slot == 3) BootCraftSubMenu.Show(Owner, _slot, _cardType);
        else if (_slot == 4) SpearCraftSubMenu.Show(Owner, _slot, _cardType);
        else if (_slot == 5) ShieldCraftSubMenu.Show(Owner, _slot, _cardType);
        else if (_slot == 6) BeaconCraftSubMenu.Show(Owner, _slot, _cardType);
        else ShowSlotReplaceSubMenu(Owner, _slot, _cardType);
        return Task.CompletedTask;
    }

    internal static void ShowWeaponSubMenu(Player player, int slot, Type cardType)
    {
        var options = (List<RestSiteOption>)RunManager.Instance.RestSiteSynchronizer.GetOptionsForPlayer(player);
        var data = WeaponCraftRegistry.GetData(cardType);
        int group = CraftCraftOption.GetCraftGroup(slot);
        options.Clear();
        if (data?.NextTier != null)
            options.Add(new WeaponUpgradeOption(player, slot, cardType, data.NextTier, data.UpgradeCost));
        if (data?.Alternate != null)
            options.Add(new WeaponReplaceOption(player, slot, cardType, data.Alternate, data.ReplaceCost));
        options.Add(new WeaponReturnOption(player, group));
    }

    internal static void ShowSlotReplaceSubMenu(Player player, int slot, Type currentType)
    {
        var options = (List<RestSiteOption>)RunManager.Instance.RestSiteSynchronizer.GetOptionsForPlayer(player);
        int group = CraftCraftOption.GetCraftGroup(slot);
        options.Clear();
        if (SlotCraftRegistry.ReplaceOptions.TryGetValue(slot, out var candidates))
        {
            foreach (var alt in candidates)
            {
                if (alt == currentType) continue;
                options.Add(new SlotReplaceOption(player, slot, currentType, alt, SlotCraftRegistry.ReplaceCost));
            }
        }
        options.Add(new WeaponReturnOption(player, group));
    }
}

public class WeaponUpgradeOption : RestSiteOption, ICustomRestSiteIcon
{
    private readonly int _slot; private readonly Type _toType; private readonly int _cost;
    private readonly bool _enabled; private readonly string _cn;

    public override string OptionId => "WEAPON_UPGRADE";
    public override LocString Description { get { var d = new LocString("cards", "STW_REST_WEAPON_UPGRADE_DESC"); d.Add("Cost", _cost); d.Add("Card", new LocString("cards", $"{_cn}.title")); d.Add("CardDesc", CardDescriptionHelper.GetCardDescription(_toType)); return d; } }
    public LocString CustomTitle => new("cards", "STW_REST_WEAPON_UPGRADE_TITLE");
    public bool CustomIsEnabled => _enabled;

    public WeaponUpgradeOption(Player owner, int slot, Type fromType, Type toType, int cost) : base(owner)
    {
        _slot = slot; _toType = toType; _cost = cost; _cn = ModelDb.GetId(toType).ToString().Replace("CARD.", "");
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

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default)
    {
        CraftSlotOption.ShowWeaponSubMenu(Owner, _slot, _toType);
        return Task.CompletedTask;
    }
}

public class WeaponReplaceOption : RestSiteOption, ICustomRestSiteIcon
{
    private readonly int _slot; private readonly Type _toType; private readonly int _cost;
    private readonly bool _enabled; private readonly string _cn;

    public override string OptionId => "WEAPON_REPLACE";
    public override LocString Description { get { var d = new LocString("cards", "STW_REST_WEAPON_REPLACE_DESC"); d.Add("Cost", _cost); d.Add("Card", new LocString("cards", $"{_cn}.title")); d.Add("CardDesc", CardDescriptionHelper.GetCardDescription(_toType)); return d; } }
    public LocString CustomTitle => new("cards", "STW_REST_WEAPON_REPLACE_TITLE");
    public bool CustomIsEnabled => _enabled;

    public WeaponReplaceOption(Player owner, int slot, Type fromType, Type toType, int cost) : base(owner)
    {
        _slot = slot; _toType = toType; _cost = cost; _cn = ModelDb.GetId(toType).ToString().Replace("CARD.", "");
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

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default)
    {
        CraftSlotOption.ShowWeaponSubMenu(Owner, _slot, _toType);
        return Task.CompletedTask;
    }
}

public class SlotReplaceOption : RestSiteOption, ICustomRestSiteIcon
{
    private readonly int _slot; private readonly Type _toType; private readonly int _cost;
    private readonly bool _enabled; private readonly string _cn;

    public override string OptionId => $"SLOT_REPLACE_{_cn}";
    public override LocString Description { get { var d = new LocString("cards", "STW_REST_SLOT_REPLACE_DESC"); d.Add("Cost", _cost); d.Add("Card", new LocString("cards", $"{_cn}.title")); d.Add("CardDesc", CardDescriptionHelper.GetCardDescription(_toType)); return d; } }
    public LocString CustomTitle => new("cards", "STW_REST_SLOT_REPLACE_TITLE");
    public bool CustomIsEnabled => _enabled;
    public string CustomIconPath => $"res://images/Extension/Cards/{_toType.Name}.png";

    public SlotReplaceOption(Player owner, int slot, Type fromType, Type toType, int cost) : base(owner)
    {
        _slot = slot; _toType = toType; _cost = cost; _cn = ModelDb.GetId(toType).ToString().Replace("CARD.", "");
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

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default)
    {
        if (_slot == 0) CraftSlotOption.ShowWeaponSubMenu(Owner, _slot, _toType);
        else CraftSlotOption.ShowSlotReplaceSubMenu(Owner, _slot, _toType);
        return Task.CompletedTask;
    }
}

public class WeaponReturnOption : RestSiteOption, ICustomRestSiteIcon
{
    private readonly int _craftGroup;

    public override string OptionId => "WEAPON_RETURN";
    public override LocString Description => new("cards", "STW_REST_WEAPON_RETURN_DESC");
    public LocString CustomTitle => new("cards", "STW_REST_WEAPON_RETURN_TITLE");
    public bool CustomIsEnabled => true;

    /// <param name="craftGroup">制作组编号（1 或 2），传 0 使用旧的全体槽位行为。</param>
    public WeaponReturnOption(Player owner, int craftGroup = 0) : base(owner)
    {
        _craftGroup = craftGroup;
    }

    public override Task<bool> OnSelect() => Task.FromResult(true);

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default)
    {
        if (_craftGroup > 0)
        {
            CraftCraftOption.ShowGroupMenu(Owner, _craftGroup);
        }
        else
        {
            // 兼容旧行为：显示全体槽位（无分组时不应再触发，但保留以防万一）
            var options = (List<RestSiteOption>)RunManager.Instance.RestSiteSynchronizer.GetOptionsForPlayer(Owner);
            options.Clear();
            var pickaxe = Owner.Relics.OfType<IPickaxeRelic>().FirstOrDefault();
            if (pickaxe == null) { CraftRestSiteOption.ShowCraftSubMenu(Owner); return Task.CompletedTask; }
            var bindings = PickaxeCardBindings.Deserialize(pickaxe.GetCardBindingsData());
            bool isMulti = Owner.RunState.Players.Count > 1;
            for (int i = 0; i < PickaxeCardBindings.SlotCount; i++)
            {
                if (i == 6 && !isMulti) continue;
                if (i == 5 && ShieldCraftSubMenu.IsSlotDisabled(pickaxe)) continue;
                var ct2 = bindings[i];
                if (ct2 != null) options.Add(new CraftSlotOption(Owner, i, ct2));
            }
            options.Add(new WeaponReturnOption(Owner));
        }
        return Task.CompletedTask;
    }
}

/// <summary>从制作组菜单返回根目录合成选项。</summary>
public class CraftGroupReturnOption : RestSiteOption, ICustomRestSiteIcon
{
    public override string OptionId => "CRAFT_GROUP_RETURN";
    public override LocString Description => new("cards", "STW_REST_CRAFT_GROUP_RETURN_DESC");
    public LocString CustomTitle => new("cards", "STW_REST_CRAFT_GROUP_RETURN_TITLE");
    public bool CustomIsEnabled => true;

    public CraftGroupReturnOption(Player owner) : base(owner) { }

    public override Task<bool> OnSelect() => Task.FromResult(true);

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default)
    {
        CraftRestSiteOption.ShowCraftSubMenu(Owner);
        return Task.CompletedTask;
    }
}

/// <summary>为篝火 UI 提供卡牌描述文本（通过每张卡独立的本地化 key 支持多语言）。</summary>
public static class CardDescriptionHelper
{
    /// <summary>获取卡牌描述 LocString（查 cards 表中 STW_REST_CARD_DESC_{CardName} 键）。</summary>
    public static LocString GetCardDescription(Type cardType)
    {
        var cardName = ModelDb.GetId(cardType).ToString().Replace("CARD.", "");
        return new LocString("cards", $"STW_REST_CARD_DESC_{cardName}");
    }

    /// <summary>获取卡牌描述 LocString（用于 CraftSlotOption.Description）。</summary>
    public static LocString GetDescriptionLocString(Type cardType) => GetCardDescription(cardType);
}
