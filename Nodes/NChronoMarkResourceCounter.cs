using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib.Combat.SecondaryResources;

namespace SpaceTimeWitch.Nodes;

/// <summary>
/// ChronoMark（时痕）战斗 UI 计数器。
/// 图标上居中显示当前数量，为 0 时自动隐藏。
/// </summary>
public partial class NChronoMarkResourceCounter : Control
{
    private TextureRect _icon = null!;
    private MegaRichTextLabel _label = null!;
    private Player? _player;
    private string _resourceId = string.Empty;
    private int _displayedCount = -1;

    public NChronoMarkResourceCounter(string iconPath)
    {
        MouseFilter = MouseFilterEnum.Ignore;

        _icon = new TextureRect
        {
            Texture = ResourceLoader.Load<Texture2D>(iconPath),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            Size = new Vector2(64, 64),
            MouseFilter = MouseFilterEnum.Ignore,
        };
        AddChild(_icon);

        _label = new MegaRichTextLabel
        {
            BbcodeEnabled = true,
            AnchorLeft = 0.5f, AnchorTop = 0.5f, AnchorRight = 0.5f, AnchorBottom = 0.5f,
            OffsetLeft = -24, OffsetTop = -16, OffsetRight = 24, OffsetBottom = 16,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        _label.AddThemeColorOverride("default_color", new Color(1f, 0.965f, 0.886f));
        _label.AddThemeFontSizeOverride("normal_font_size", 20);
        _icon.AddChild(_label);

        Visible = false;
    }

    public void Bind(Player player, string resourceId)
    {
        _player = player;
        _resourceId = resourceId;
        SetProcess(true);
    }

    public override void _Process(double delta)
    {
        if (_player == null) return;

        int amount = SecondaryResourceCmd.Get(_player, _resourceId);
        if (amount != _displayedCount)
        {
            _displayedCount = amount;
            _label.Text = $"[center]{amount}[/center]";
        }
        Visible = amount > 0;
    }
}
