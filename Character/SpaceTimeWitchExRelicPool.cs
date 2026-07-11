using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Character;

public class SpaceTimeWitchExRelicPool : TypeListRelicPoolModel
{
    // 描述中使用的能量图标。大小为24x24。
    public override string? TextEnergyIconPath => "res://images/SpaceTimeWitch/UI/Chronite24.png";
    // tooltip和卡牌左上角的能量图标。大小为74x74。
    public override string? BigEnergyIconPath => "res://images/SpaceTimeWitch/UI/Chronite.png";

    public override string EnergyColorName => "ChroniteColor";
    
    
}