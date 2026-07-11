using Godot;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Characters;

namespace SpaceTimeWitch.Character;

public class SpaceTimeWitchCardPool : TypeListCardPoolModel, IModColorfulPhilosophersCardPool
{
    private static Theme? _colorTheme;
    private static Theme ColorTheme => 
        _colorTheme ??= GD.Load<Theme>("res://themes/Color.tres");
    // 卡池的ID。必须唯一防撞车。
    public override string Title => "SpaceTimeWitchCardPool";
    public override string EnergyColorName => "Chronite";

    // 描述中使用的能量图标。大小为24x24。
    public override string? TextEnergyIconPath => "res://images/SpaceTimeWitch/UI/Chronite24.png";
    // tooltip和卡牌左上角的能量图标。大小为74x74。
    public override string? BigEnergyIconPath => "res://images/SpaceTimeWitch/UI/Chronite.png";

    // 卡池的主题色。
    public override Color DeckEntryCardColor => new(0.106f, 0.039f, 0.243f);
    // 能量表盘文字轮廓颜色
    public override Color EnergyOutlineColor => new(0.106f, 0.039f, 0.243f);

    // 卡池是否是无色。例如事件、状态等卡池就是无色的。
    public override bool IsColorless => false;
}