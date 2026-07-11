using System.Collections.Generic;
using System.Linq;
using Godot;
using SpaceTimeWitch.Relics;
using SpaceTimeWitch.Scripts;
using STS2RitsuLib.Settings;

namespace SpaceTimeWitch.Nodes;

/// <summary>
/// 链接时空卡池配置自定义设置控件。
/// 从 <see cref="TagRelicRegistry.Entries"/> 动态生成各组权重滑条、去重开关和重置按钮。
/// 新增卡池/遗物时只需在 TagRelicRegistry 添加条目，UI 自动生成对应控件。
/// 文本通过 <see cref="TagRelicPoolText"/> 传入，支持本地化。
/// </summary>
public sealed partial class NTagRelicPoolSettingsControl : VBoxContainer
{
    private readonly IModSettingsUiActionHost _host;
    private readonly ModSettingsValueBinding<SpaceTimeWitchSettingsData,
        SpaceTimeWitchSettingsData> _rootBinding;
    private readonly TagRelicPoolText _text;

    private Dictionary<string, int> _groupWeights = [];
    private Dictionary<string, bool> _groupDedup = [];
    private Dictionary<string, int> _relicWeights = [];
    private Dictionary<string, int> _branchWeights = [];
    private Dictionary<string, bool> _classDedup = [];

    private const double SliderMin = 0;
    private const double SliderMax = 5;
    private const double SliderStep = 1;
    private const int DefaultWeight = 1;

    public NTagRelicPoolSettingsControl(
        IModSettingsUiActionHost host,
        ModSettingsValueBinding<SpaceTimeWitchSettingsData,
            SpaceTimeWitchSettingsData> rootBinding,
        TagRelicPoolText text)
    {
        _host = host;
        _rootBinding = rootBinding;
        _text = text;

        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        SizeFlagsVertical = SizeFlags.ExpandFill;

        LoadState();
        BuildUi();
    }

    // ==================== 状态 ====================

    private void LoadState()
    {
        var data = _rootBinding.Read();
        _groupWeights = data.TagRelicGroupWeights.Count > 0
            ? new Dictionary<string, int>(data.TagRelicGroupWeights) : [];
        _groupDedup = data.TagRelicGroupDedup.Count > 0
            ? new Dictionary<string, bool>(data.TagRelicGroupDedup) : [];
        _relicWeights = data.TagRelicWeights.Count > 0
            ? new Dictionary<string, int>(data.TagRelicWeights) : [];
        _branchWeights = data.TagRelicBranchWeights.Count > 0
            ? new Dictionary<string, int>(data.TagRelicBranchWeights) : [];
        _classDedup = data.TagRelicClassDedup.Count > 0
            ? new Dictionary<string, bool>(data.TagRelicClassDedup) : [];

        foreach (var group in TagRelicRegistry.Entries.Values.Select(d => d.Group).Distinct())
        {
            if (!_groupWeights.ContainsKey(group))
                _groupWeights[group] = DefaultWeight;
            if (!_groupDedup.ContainsKey(group))
                _groupDedup[group] = false;
        }

        foreach (var (type, relicData) in TagRelicRegistry.Entries)
        {
            if (!_relicWeights.ContainsKey(type.Name))
                _relicWeights[type.Name] = (int)relicData.Weight;
        }

        foreach (var (parentType, relicData) in TagRelicRegistry.Entries)
        {
            if (relicData.NextTierWeights == null) continue;
            foreach (var (childType, defaultW) in relicData.NextTierWeights)
            {
                var key = BranchKey(parentType.Name, childType.Name);
                if (!_branchWeights.ContainsKey(key))
                    _branchWeights[key] = (int)defaultW;
            }
        }

        var allClasses = TagRelicRegistry.Entries.Values
            .Select(d => d.Class).Where(c => !string.IsNullOrEmpty(c)).Distinct();
        foreach (var cls in allClasses)
        {
            if (!_classDedup.ContainsKey(cls))
                _classDedup[cls] = TagRelicRegistry.LimitedClasses.Contains(cls);
        }
    }

    private void Persist()
    {
        var data = _rootBinding.Read();
        data.TagRelicGroupWeights = new Dictionary<string, int>(_groupWeights);
        data.TagRelicGroupDedup = new Dictionary<string, bool>(_groupDedup);
        data.TagRelicWeights = new Dictionary<string, int>(_relicWeights);
        data.TagRelicBranchWeights = new Dictionary<string, int>(_branchWeights);
        data.TagRelicClassDedup = new Dictionary<string, bool>(_classDedup);
        _rootBinding.Save();
        _host.MarkDirty(_rootBinding);
        _host.RequestRefresh();

        SpaceTimeWitchSettings.SyncTagRelicFrom(
            _groupWeights, _groupDedup, _relicWeights, _branchWeights, _classDedup);
    }

    private static string BranchKey(string parentTypeName, string childTypeName) =>
        $"{parentTypeName}:{childTypeName}";

    // ==================== 名称解析 ====================

    /// <summary>获取遗物的本地化显示名称。</summary>
    private string GetRelicDisplayName(Type relicType)
    {
        if (_text.RelicNameResolver != null)
            return _text.RelicNameResolver(relicType);
        return relicType.Name;
    }

    // ==================== UI 构建 ====================

    private void BuildUi()
    {
        var groups = TagRelicRegistry.Entries
            .GroupBy(kv => kv.Value.Group).ToList();

        foreach (var group in groups)
        {
            var entries = group
                .Select(kv => (Type: kv.Key, Data: kv.Value))
                .OrderBy(e => e.Data.Tier).ToList();
            BuildGroupSection(this, group.Key, entries);
        }

        AddChild(new HSeparator { CustomMinimumSize = new Vector2(0, 8) });
        BuildClassDedupSection(this);
    }

    private void BuildGroupSection(VBoxContainer parent, string groupName,
        List<(Type Type, TagRelicData Data)> entries)
    {
        // 组标题
        var header = new Label
        {
            Text = groupName,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        header.AddThemeFontSizeOverride("font_size", 16);
        parent.AddChild(header);
        parent.AddChild(new Control { CustomMinimumSize = new Vector2(0, 4) });

        // 组权重
        AddSliderRow(parent, _text.GroupWeightLabel,
            _groupWeights.GetValueOrDefault(groupName, DefaultWeight),
            v => { _groupWeights[groupName] = (int)v; Persist(); });

        // 组去重 —— on/off 切换开关
        AddToggleRow(parent, _text.GroupDedupLabel,
            _groupDedup.GetValueOrDefault(groupName, false),
            p => { _groupDedup[groupName] = p; Persist(); });

        // 全部重置
        AddButtonRow(parent, _text.ResetAllLabel, () =>
        {
            _groupWeights[groupName] = DefaultWeight;
            _groupDedup[groupName] = false;
            foreach (var (type, data) in entries)
            {
                var tn = type.Name;
                _relicWeights[tn] = (int)data.Weight;
                if (data.NextTierWeights != null)
                    foreach (var (child, w) in data.NextTierWeights)
                        _branchWeights[BranchKey(tn, child.Name)] = (int)w;
            }
            Persist();
            RebuildUi();
        });

        parent.AddChild(new Control { CustomMinimumSize = new Vector2(0, 6) });

        // 每个遗物
        foreach (var (type, data) in entries)
        {
            // 仅 Tier1 或有升级分支的遗物显示自身权重滑条
            bool showWeight = data.Tier == 1 || (data.NextTierWeights is { Count: > 0 });
            if (showWeight)
                BuildRelicWeightRow(parent, type, data);
        }

        parent.AddChild(new HSeparator { CustomMinimumSize = new Vector2(0, 6) });
        parent.AddChild(new Control { CustomMinimumSize = new Vector2(0, 4) });
    }

    private void BuildRelicWeightRow(VBoxContainer parent, Type relicType, TagRelicData data)
    {
        var tn = relicType.Name;
        var displayName = GetRelicDisplayName(relicType);
        var tierLabel = $"Tier{data.Tier}";
        if (!string.IsNullOrEmpty(data.Class))
            tierLabel += $" {data.Class}";

        AddSliderRow(parent, displayName,
            _relicWeights.GetValueOrDefault(tn, (int)data.Weight),
            v => { _relicWeights[tn] = (int)v; Persist(); },
            subLabel: tierLabel);

        // 升级分支权重
        if (data.NextTierWeights is { Count: > 0 })
        {
            foreach (var (childType, defaultW) in data.NextTierWeights)
            {
                var bk = BranchKey(tn, childType.Name);

                AddSliderRow(parent,
                    _text.BranchWeightFormat,
                    _branchWeights.GetValueOrDefault(bk, (int)defaultW),
                    v => { _branchWeights[bk] = (int)v; Persist(); },
                    indent: true);
            }
        }

        parent.AddChild(new Control { CustomMinimumSize = new Vector2(0, 4) });
    }

    private void BuildClassDedupSection(VBoxContainer parent)
    {
        var header = new Label
        {
            Text = _text.ClassDedupTitle,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        header.AddThemeFontSizeOverride("font_size", 16);
        parent.AddChild(header);

        var sub = new Label
        {
            Text = _text.ClassDedupDesc,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        sub.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        parent.AddChild(sub);
        parent.AddChild(new Control { CustomMinimumSize = new Vector2(0, 4) });

        var allClasses = TagRelicRegistry.Entries.Values
            .Select(d => d.Class).Where(c => !string.IsNullOrEmpty(c))
            .Distinct().OrderBy(c => c).ToList();

        foreach (var cls in allClasses)
        {
            AddCheckBoxRow(parent, cls,
                _classDedup.GetValueOrDefault(cls, true),
                p => { _classDedup[cls] = p; Persist(); });
        }

        parent.AddChild(new Control { CustomMinimumSize = new Vector2(0, 4) });

        AddButtonRow(parent, _text.ResetDedupLabel, () =>
        {
            foreach (var cls in allClasses)
                _classDedup[cls] = TagRelicRegistry.LimitedClasses.Contains(cls);
            Persist();
            RebuildUi();
        });
    }

    // ==================== UI 辅助 ====================

    private void AddSliderRow(VBoxContainer parent, string label, double value,
        Action<double> onChanged, string? subLabel = null, bool indent = false)
    {
        var row = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        if (indent)
            row.AddChild(new Control { CustomMinimumSize = new Vector2(16, 0) });

        var labelCtl = new Label
        {
            Text = label,
            HorizontalAlignment = HorizontalAlignment.Left,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsStretchRatio = 4,
        };
        row.AddChild(labelCtl);

        if (!string.IsNullOrEmpty(subLabel))
        {
            var sl = new Label
            {
                Text = subLabel,
                HorizontalAlignment = HorizontalAlignment.Left,
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsStretchRatio = 2,
            };
            sl.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
            row.AddChild(sl);
        }

        var slider = new HSlider
        {
            MinValue = SliderMin, MaxValue = SliderMax, Step = SliderStep,
            Value = value,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsStretchRatio = 6,
        };
        var valueLabel = new Label
        {
            Text = ((int)value).ToString(),
            HorizontalAlignment = HorizontalAlignment.Right,
            CustomMinimumSize = new Vector2(32, 0),
        };
        slider.ValueChanged += v =>
        {
            valueLabel.Text = ((int)v).ToString();
            onChanged(v);
        };
        row.AddChild(slider);
        row.AddChild(valueLabel);
        parent.AddChild(row);
    }

    /// <summary>On/Off 切换开关（ToggleMode Button），用于组去重。</summary>
    private void AddToggleRow(VBoxContainer parent, string label, bool pressed,
        Action<bool> onToggled)
    {
        var row = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };

        var labelCtl = new Label
        {
            Text = label,
            HorizontalAlignment = HorizontalAlignment.Left,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsStretchRatio = 6,
        };
        row.AddChild(labelCtl);

        var colorOn = new Color(0.2f, 0.8f, 0.2f);
        var colorOff = new Color(0.9f, 0.2f, 0.2f);

        var btn = new Button
        {
            Text = pressed ? _text.ToggleOnLabel : _text.ToggleOffLabel,
            ToggleMode = true,
            ButtonPressed = pressed,
            CustomMinimumSize = new Vector2(52, 0),
            SizeFlagsHorizontal = SizeFlags.ShrinkEnd,
        };
        // 所有状态的颜色都要覆盖，否则 ToggleMode 按下态会走默认主题色
        btn.AddThemeColorOverride("font_color", pressed ? colorOn : colorOff);
        btn.AddThemeColorOverride("font_hover_color", pressed ? colorOn : colorOff);
        btn.AddThemeColorOverride("font_pressed_color", pressed ? colorOn : colorOff);
        btn.AddThemeColorOverride("font_focus_color", pressed ? colorOn : colorOff);
        btn.Toggled += toggled =>
        {
            btn.Text = toggled ? _text.ToggleOnLabel : _text.ToggleOffLabel;
            btn.AddThemeColorOverride("font_color", toggled ? colorOn : colorOff);
            btn.AddThemeColorOverride("font_hover_color", toggled ? colorOn : colorOff);
            btn.AddThemeColorOverride("font_pressed_color", toggled ? colorOn : colorOff);
            btn.AddThemeColorOverride("font_focus_color", toggled ? colorOn : colorOff);
            onToggled(toggled);
        };
        row.AddChild(btn);
        parent.AddChild(row);
    }

    private static void AddCheckBoxRow(VBoxContainer parent, string label, bool pressed,
        Action<bool> onToggled)
    {
        var cb = new CheckBox
        {
            Text = label,
            ButtonPressed = pressed,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        cb.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f));
        cb.AddThemeColorOverride("font_hover_color", new Color(1f, 1f, 1f));
        cb.Toggled += p => onToggled(p);
        parent.AddChild(cb);
    }

    private static void AddButtonRow(VBoxContainer parent, string text, Action onPressed,
        bool indent = false)
    {
        var row = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        if (indent) row.AddChild(new Control { CustomMinimumSize = new Vector2(16, 0) });

        var btn = new Button { Text = text, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        btn.Pressed += () => onPressed();
        row.AddChild(btn);
        parent.AddChild(row);
    }

    private void RebuildUi()
    {
        foreach (var child in GetChildren().ToList())
            child.QueueFree();
        BuildUi();
    }
}
