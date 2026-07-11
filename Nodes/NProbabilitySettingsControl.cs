using Godot;
using SpaceTimeWitch.Scripts;
using STS2RitsuLib.Settings;

namespace SpaceTimeWitch.Nodes;

/// <summary>
/// 概率控制自定义设置控件：三滑块联动（sum=100）+ 互斥锁定 + 重置按钮。
/// </summary>
public sealed partial class NProbabilitySettingsControl : VBoxContainer
{
    private readonly string _title;
    private readonly string _lockCommonLabel;
    private readonly string _lockUncommonLabel;
    private readonly string _lockRareLabel;
    private readonly string _resetButtonLabel;

    private readonly IModSettingsUiActionHost _host;
    private readonly ModSettingsValueBinding<SpaceTimeWitchSettingsData,
        SpaceTimeWitchSettingsData> _rootBinding;
    private readonly ModSettingsValueBinding<SpaceTimeWitchSettingsData, int> _commonBinding;
    private readonly ModSettingsValueBinding<SpaceTimeWitchSettingsData, int> _uncommonBinding;
    private readonly ModSettingsValueBinding<SpaceTimeWitchSettingsData, int> _rareBinding;
    private readonly ModSettingsValueBinding<SpaceTimeWitchSettingsData, LockTarget> _lockBinding;

    private int _commonWeight;
    private int _uncommonWeight;
    private int _rareWeight;
    private LockTarget _lockedRarity;
    private bool _updating;

    private Label _titleLabel = null!;
    private CheckBox _lockCommon = null!;
    private CheckBox _lockUncommon = null!;
    private CheckBox _lockRare = null!;
    private HSlider _sliderCommon = null!;
    private HSlider _sliderUncommon = null!;
    private HSlider _sliderRare = null!;
    private Label _valueCommon = null!;
    private Label _valueUncommon = null!;
    private Label _valueRare = null!;
    private Button _resetBtn = null!;

    public NProbabilitySettingsControl(
        IModSettingsUiActionHost host,
        ModSettingsValueBinding<SpaceTimeWitchSettingsData,
            SpaceTimeWitchSettingsData> rootBinding,
        ModSettingsValueBinding<SpaceTimeWitchSettingsData, int> commonBinding,
        ModSettingsValueBinding<SpaceTimeWitchSettingsData, int> uncommonBinding,
        ModSettingsValueBinding<SpaceTimeWitchSettingsData, int> rareBinding,
        ModSettingsValueBinding<SpaceTimeWitchSettingsData, LockTarget> lockBinding,
        string title,
        string lockCommonLabel,
        string lockUncommonLabel,
        string lockRareLabel,
        string resetButtonLabel)
    {
        _host = host;
        _rootBinding = rootBinding;
        _commonBinding = commonBinding;
        _uncommonBinding = uncommonBinding;
        _rareBinding = rareBinding;
        _lockBinding = lockBinding;
        _title = title;
        _lockCommonLabel = lockCommonLabel;
        _lockUncommonLabel = lockUncommonLabel;
        _lockRareLabel = lockRareLabel;
        _resetButtonLabel = resetButtonLabel;

        // 读取当前值
        _commonWeight = _commonBinding.Read();
        _uncommonWeight = _uncommonBinding.Read();
        _rareWeight = _rareBinding.Read();
        _lockedRarity = _lockBinding.Read();

        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        BuildUi();
        RefreshUi();
    }

    // ==================== UI 构建 ====================

    private void BuildUi()
    {
        // 标题
        _titleLabel = new Label
        {
            Text = _title,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        AddChild(_titleLabel);

        // 分隔间距
        AddChild(new Control { CustomMinimumSize = new Vector2(0, 8) });

        // 三行滑块 + 锁定（CreateRow 已自动 AddChild）
        (_lockCommon, _sliderCommon, _valueCommon) = CreateRow(_lockCommonLabel);
        (_lockUncommon, _sliderUncommon, _valueUncommon) = CreateRow(_lockUncommonLabel);
        (_lockRare, _sliderRare, _valueRare) = CreateRow(_lockRareLabel);

        // 连接信号
        _lockCommon.Toggled += pressed => OnLockToggled(LockTarget.Common, pressed);
        _lockUncommon.Toggled += pressed => OnLockToggled(LockTarget.Uncommon, pressed);
        _lockRare.Toggled += pressed => OnLockToggled(LockTarget.Rare, pressed);

        _sliderCommon.ValueChanged += v => OnSliderChanged(LockTarget.Common, v);
        _sliderUncommon.ValueChanged += v => OnSliderChanged(LockTarget.Uncommon, v);
        _sliderRare.ValueChanged += v => OnSliderChanged(LockTarget.Rare, v);

        // 间距
        AddChild(new Control { CustomMinimumSize = new Vector2(0, 4) });

        // 重置按钮
        _resetBtn = new Button { Text = _resetButtonLabel };
        _resetBtn.Pressed += OnReset;
        AddChild(_resetBtn);
    }

    private (CheckBox, HSlider, Label) CreateRow(string lockLabel)
    {
        var row = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };

        var checkBox = new CheckBox
        {
            Text = lockLabel,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsStretchRatio = 4
        };
        // 未勾选：白色轮廓   已勾选：白色轮廓+白勾   文字：亮灰
        checkBox.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f));
        checkBox.AddThemeColorOverride("font_hover_color", new Color(1f, 1f, 1f));
        checkBox.AddThemeColorOverride("icon_normal_color", new Color(1f, 1f, 1f));
        checkBox.AddThemeColorOverride("icon_checked_color", new Color(1f, 1f, 1f));

        var slider = new HSlider
        {
            MinValue = 0,
            MaxValue = 100,
            Step = 5,
            Value = 0,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsStretchRatio = 8
        };

        var label = new Label
        {
            Text = "0%",
            HorizontalAlignment = HorizontalAlignment.Right,
            CustomMinimumSize = new Vector2(48, 0)
        };

        row.AddChild(checkBox);
        row.AddChild(slider);
        row.AddChild(label);
        AddChild(row);
        return (checkBox, slider, label);
    }

    // ==================== 事件处理 ====================

    private void OnLockToggled(LockTarget target, bool pressed)
    {
        if (_updating) return;

        if (pressed)
        {
            _lockedRarity = target;
        }
        else
        {
            if (_lockedRarity == target)
                return;
        }

        _updating = true;
        RefreshUi();
        _updating = false;
        Persist();
    }

    private void OnSliderChanged(LockTarget target, double value)
    {
        if (_updating) return;

        int newValue = (int)Math.Round(value / 5.0) * 5;
        if (newValue < 0) newValue = 0;
        if (newValue > 100) newValue = 100;

        if (target == _lockedRarity)
        {
            _updating = true;
            RefreshUi();
            _updating = false;
            return;
        }

        int oldValue = GetWeight(target);
        int delta = newValue - oldValue;
        if (delta == 0) return;

        // 限制 delta 不超过其他未锁定滑块能吸收的范围
        delta = ClampDelta(target, delta);
        if (delta == 0) return;

        _updating = true;
        SetWeight(target, oldValue + delta);
        DistributeDelta(target, -delta);
        EnforceSum(target);
        RefreshUi();
        _updating = false;
        Persist();
    }

    /// <summary>限制 delta 使其他未锁定滑块不超出 [0, 100]。</summary>
    private int ClampDelta(LockTarget target, int delta)
    {
        var others = new[] { LockTarget.Common, LockTarget.Uncommon, LockTarget.Rare }
            .Where(t => t != target && t != _lockedRarity)
            .ToList();

        if (others.Count == 1)
        {
            int otherVal = GetWeight(others[0]);
            if (delta > 0 && delta > otherVal) delta = otherVal;
            if (delta < 0 && -delta > 100 - otherVal) delta = -(100 - otherVal);
        }
        else if (others.Count == 2)
        {
            foreach (var t in others)
            {
                int v = GetWeight(t);
                if (delta > 0 && v == 0) { delta = 0; break; }
                if (delta < 0 && v == 100) { delta = 0; break; }
            }
        }

        return delta;
    }

    /// <summary>确保三者总和为 100（超出/不足由 target 吸收）。</summary>
    private void EnforceSum(LockTarget target)
    {
        int sum = _commonWeight + _uncommonWeight + _rareWeight;
        if (sum == 100) return;
        int diff = 100 - sum;
        int cur = GetWeight(target);
        SetWeight(target, Math.Clamp(cur + diff, 0, 100));
    }

    private void OnReset()
    {
        _commonWeight = SpaceTimeWitchSettings.DefaultCommonWeight;
        _uncommonWeight = SpaceTimeWitchSettings.DefaultUncommonWeight;
        _rareWeight = SpaceTimeWitchSettings.DefaultRareWeight;
        _lockedRarity = LockTarget.Rare;
        _updating = true;
        RefreshUi();
        _updating = false;
        Persist();
    }

    // ==================== 核心逻辑 ====================

    private void DistributeDelta(LockTarget changed, int delta)
    {
        if (delta == 0) return;

        var unlocked = new[] { LockTarget.Common, LockTarget.Uncommon, LockTarget.Rare }
            .Where(t => t != changed && t != _lockedRarity)
            .ToList();

        if (unlocked.Count == 0) return;

        if (unlocked.Count == 1)
        {
            // 只有一个已解锁滑块，全部给它
            int newVal = Math.Clamp(GetWeight(unlocked[0]) + delta, 0, 100);
            SetWeight(unlocked[0], newVal);
        }
        else
        {
            // 按比例分配给两个已解锁滑块
            int total = unlocked.Sum(t => GetWeight(t));
            if (total == 0)
            {
                int each = delta / unlocked.Count;
                foreach (var t in unlocked)
                    SetWeight(t, Math.Clamp(each, 0, 100));
            }
            else
            {
                int remaining = delta;
                for (int i = 0; i < unlocked.Count - 1; i++)
                {
                    double ratio = (double)GetWeight(unlocked[i]) / total;
                    int share = (int)Math.Round(ratio * delta);
                    int newVal = Math.Clamp(GetWeight(unlocked[i]) + share, 0, 100);
                    int actualDelta = newVal - GetWeight(unlocked[i]);
                    SetWeight(unlocked[i], newVal);
                    remaining -= actualDelta;
                }
                // 最后一个拿余数
                int lastVal = Math.Clamp(GetWeight(unlocked[^1]) + remaining, 0, 100);
                SetWeight(unlocked[^1], lastVal);
            }
        }
    }

    private int GetWeight(LockTarget target) => target switch
    {
        LockTarget.Common => _commonWeight,
        LockTarget.Uncommon => _uncommonWeight,
        LockTarget.Rare => _rareWeight,
        _ => 0
    };

    private void SetWeight(LockTarget target, int val)
    {
        switch (target)
        {
            case LockTarget.Common: _commonWeight = val; break;
            case LockTarget.Uncommon: _uncommonWeight = val; break;
            case LockTarget.Rare: _rareWeight = val; break;
        }
    }

    // ==================== UI 刷新 & 持久化 ====================

    private void RefreshUi()
    {
        _lockCommon.ButtonPressed = _lockedRarity == LockTarget.Common;
        _lockUncommon.ButtonPressed = _lockedRarity == LockTarget.Uncommon;
        _lockRare.ButtonPressed = _lockedRarity == LockTarget.Rare;

        _sliderCommon.SetValueNoSignal(_commonWeight);
        _sliderUncommon.SetValueNoSignal(_uncommonWeight);
        _sliderRare.SetValueNoSignal(_rareWeight);

        ApplyLockStyle(_sliderCommon, _lockedRarity == LockTarget.Common);
        ApplyLockStyle(_sliderUncommon, _lockedRarity == LockTarget.Uncommon);
        ApplyLockStyle(_sliderRare, _lockedRarity == LockTarget.Rare);

        _valueCommon.Text = $"{_commonWeight}%";
        _valueUncommon.Text = $"{_uncommonWeight}%";
        _valueRare.Text = $"{_rareWeight}%";
    }

    private static void ApplyLockStyle(HSlider slider, bool locked)
    {
        // 已锁定的滑块半透明灰色 + 禁用鼠标交互
        if (locked)
        {
            slider.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            slider.MouseFilter = MouseFilterEnum.Ignore;
        }
        else
        {
            slider.Modulate = new Color(1f, 1f, 1f);
            slider.MouseFilter = MouseFilterEnum.Pass;
        }
    }

    private void Persist()
    {
        _commonBinding.Write(_commonWeight);
        _uncommonBinding.Write(_uncommonWeight);
        _rareBinding.Write(_rareWeight);
        _lockBinding.Write(_lockedRarity);
        _rootBinding.Save();
        _host.MarkDirty(_rootBinding);
        _host.RequestRefresh();

        // 同步静态属性，供 WeightedCardSelectCmd 等读取
        SpaceTimeWitchSettings.SyncFrom(_commonWeight, _uncommonWeight,
            _rareWeight, _lockedRarity);
    }
}
