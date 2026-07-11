using SpaceTimeWitch.Character;
using SpaceTimeWitch.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Extension.DCB;

[RegisterRelic(typeof(SpaceTimeWitchExRelicPool))]
public class DCB3 : TagRelic
{
    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://images/Extension/Relics/DCB.png",
        IconOutlinePath: "res://images/Extension/Relics/DCB.png",
        BigIconPath: "res://images/Extension/Relics/DCB.png"
    );
}