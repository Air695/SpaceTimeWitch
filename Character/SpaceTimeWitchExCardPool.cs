using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using SpaceTimeWitch.Cards;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Character;

public class SpaceTimeWitchExCardPool : TypeListCardPoolModel
{
    private static Theme? _colorTheme;
    private static Theme ColorTheme =>
        _colorTheme ??= GD.Load<Theme>("res://themes/Color.tres");
    public override string Title => "SpaceTimeWitchExCardPool";
    public override string EnergyColorName => "Chronite";
    public override string? TextEnergyIconPath => "res://images/SpaceTimeWitch/UI/Chronite24.png";
    public override string? BigEnergyIconPath => "res://images/SpaceTimeWitch/UI/Chronite.png";
    public override Color DeckEntryCardColor => new(0.106f, 0.039f, 0.243f);
    public override Color EnergyOutlineColor => new(0.106f, 0.039f, 0.243f);
    public override bool IsColorless => false;

    public override IEnumerable<CardModel> AllCards =>
        base.AllCards.OrderBy(c => GetSpacetimeGroup(c)).ThenBy(c => c.Rarity);

    private static int GetSpacetimeGroup(CardModel card)
    {
        var tags = card.Tags;
        if (tags.Any(t => (int)t >= 210504 && (int)t <= 210506)) return 0; // DCB
        if (tags.Any(t => (int)t >= 210501 && (int)t <= 210503)) return 1; // ERZ
        return 2; // MCJ
    }
}