using Godot;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Extension.QG;
[RegisterSharedCardPool]
public class STWLRA : TypeListCardPoolModel
{
    public override string Title => "STWLRA";
    public override string EnergyColorName => "STWLRA";
    public override string? TextEnergyIconPath => "res://images/Extension/UI/124.png";
    public override string? BigEnergyIconPath => "res://images/Extension/UI/1.png";
    public override Color DeckEntryCardColor => new(0f,0f,0f);
    public override Color EnergyOutlineColor => new(1f, 0.843f, 0f);
    public override bool IsColorless => false;
}