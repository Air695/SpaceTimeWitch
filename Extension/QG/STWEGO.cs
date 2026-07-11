using Godot;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;

namespace SpaceTimeWitch.Extension.QG;
[RegisterSharedCardPool]
public class STWEGO : TypeListCardPoolModel, IModColorfulPhilosophersCardPool
{
    public override string Title => "STWEGO";
    public override string EnergyColorName => "STWEGO";
    public override string? TextEnergyIconPath => "res://images/Extension/UI/124.png";
    public override string? BigEnergyIconPath => "res://images/Extension/UI/1.png";
    public override Color DeckEntryCardColor => new(0.8f, 0.0f, 0.0f);
    public override Color EnergyOutlineColor => new(1f, 0.843f, 0f);
    public override bool IsColorless => false;
}