using SpaceTimeWitch.Character;
using SpaceTimeWitch.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Extension.ERZ;

[RegisterRelic(typeof(SpaceTimeWitchExRelicPool))]
public class ERZ2 : TagRelic
{
    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://images/Extension/Relics/ERZ.png",
        IconOutlinePath: "res://images/Extension/Relics/ERZ.png",
        BigIconPath: "res://images/Extension/Relics/ERZ.png"
    );

}