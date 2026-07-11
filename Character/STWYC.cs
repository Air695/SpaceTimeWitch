using Godot;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Character;
[RegisterSharedCardPool]
public class STWYC : TypeListCardPoolModel
{
    public override string Title => "STWYC";
    public override string EnergyColorName => "STWYC";
    public override string? TextEnergyIconPath => null;
    public override string? BigEnergyIconPath => null;
    public override Color DeckEntryCardColor => new(0f, 0f, 0f);
    public override Color EnergyOutlineColor => new(0f, 0f, 0f);
    public override bool IsColorless => false;
}