using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Scripts;

public abstract class SpaceTimeWitchRelics(RelicRarity relicRarity) : ModRelicTemplate
{
    public override RelicRarity Rarity => relicRarity;

    internal IEnumerable<DynamicVar> ExposedCanonicalVars => CanonicalVars;

    /// <summary>自定义悬浮提示（非 PowerVar）。子类 override 返回。</summary>
    protected virtual IEnumerable<IHoverTip> CustomHoverTips => [];

    internal IEnumerable<IHoverTip> ExposedCustomHoverTips => CustomHoverTips;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"res://images/Relics/{GetType().Name}.png",
        IconOutlinePath:null,
        BigIconPath:null
    );
}


