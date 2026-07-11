using Godot;
using SpaceTimeWitch.Scripts;
using STS2RitsuLib.Settings;

namespace SpaceTimeWitch.Nodes;

/// <summary>
/// 复现可选卡牌数量设置控件：滑条 1-10，步长 1。
/// </summary>
public sealed partial class NDiscoverOfferCountControl : VBoxContainer
{
    private readonly string _title;
    private readonly string _resetButtonLabel;

    private readonly IModSettingsUiActionHost _host;
    private readonly ModSettingsValueBinding<SpaceTimeWitchSettingsData,
        SpaceTimeWitchSettingsData> _rootBinding;
    private readonly ModSettingsValueBinding<SpaceTimeWitchSettingsData, int> _countBinding;

    private int _offerCount;
    private bool _updating;

    private Label _titleLabel = null!;
    private HSlider _slider = null!;
    private Label _valueLabel = null!;
    private Button _resetBtn = null!;

    private const int MinOfferCount = 1;
    private const int MaxOfferCount = 10;

    public NDiscoverOfferCountControl(
        IModSettingsUiActionHost host,
        ModSettingsValueBinding<SpaceTimeWitchSettingsData,
            SpaceTimeWitchSettingsData> rootBinding,
        ModSettingsValueBinding<SpaceTimeWitchSettingsData, int> countBinding,
        string title,
        string resetButtonLabel)
    {
        _host = host;
        _rootBinding = rootBinding;
        _countBinding = countBinding;
        _title = title;
        _resetButtonLabel = resetButtonLabel;

        _offerCount = _countBinding.Read();

        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        BuildUi();
        RefreshUi();
    }

    private void BuildUi()
    {
        // 标题
        _titleLabel = new Label
        {
            Text = _title,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        AddChild(_titleLabel);

        AddChild(new Control { CustomMinimumSize = new Vector2(0, 8) });

        // 滑条行
        var row = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };

        _slider = new HSlider
        {
            MinValue = MinOfferCount,
            MaxValue = MaxOfferCount,
            Step = 1,
            Value = _offerCount,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsStretchRatio = 8
        };
        _slider.ValueChanged += OnSliderChanged;

        _valueLabel = new Label
        {
            Text = _offerCount.ToString(),
            HorizontalAlignment = HorizontalAlignment.Right,
            CustomMinimumSize = new Vector2(48, 0)
        };

        row.AddChild(_slider);
        row.AddChild(_valueLabel);
        AddChild(row);

        AddChild(new Control { CustomMinimumSize = new Vector2(0, 4) });

        // 重置按钮
        _resetBtn = new Button { Text = _resetButtonLabel };
        _resetBtn.Pressed += OnReset;
        AddChild(_resetBtn);
    }

    private void OnSliderChanged(double value)
    {
        if (_updating) return;

        int newValue = (int)Math.Round(value);
        if (newValue < MinOfferCount) newValue = MinOfferCount;
        if (newValue > MaxOfferCount) newValue = MaxOfferCount;

        if (newValue == _offerCount) return;

        _offerCount = newValue;
        _updating = true;
        RefreshUi();
        _updating = false;
        Persist();
    }

    private void OnReset()
    {
        _offerCount = SpaceTimeWitchSettings.DefaultDiscoverOfferCount;
        _updating = true;
        RefreshUi();
        _updating = false;
        Persist();
    }

    private void RefreshUi()
    {
        _slider.SetValueNoSignal(_offerCount);
        _valueLabel.Text = _offerCount.ToString();
    }

    private void Persist()
    {
        _countBinding.Write(_offerCount);
        _rootBinding.Save();
        _host.MarkDirty(_rootBinding);
        _host.RequestRefresh();

        SpaceTimeWitchSettings.SyncDiscoverOfferCount(_offerCount);
    }
}
