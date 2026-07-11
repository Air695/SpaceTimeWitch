using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Scripts;

public abstract class SpaceTimeWitchRelics(RelicRarity relicRarity) : ModRelicTemplate
{
    public override RelicRarity Rarity => relicRarity;

    internal IEnumerable<DynamicVar> ExposedCanonicalVars => CanonicalVars;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"res://images/Relics/{GetType().Name}.png",
        IconOutlinePath:null,
        BigIconPath:null
    );
}


