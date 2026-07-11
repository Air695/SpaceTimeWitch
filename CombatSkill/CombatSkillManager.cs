using MegaCrit.Sts2.Core.Entities.Players;

namespace SpaceTimeWitch.CombatSkill;

public class CombatSkillState
{
    /// <summary>上一张打出的 CombatSkill 卡的完整行动（核心+额外效果）</summary>
    public CombatSkillActionData? LastAction;

    public bool IsSwapActive;
}

public static class CombatSkillManager
{
    private static readonly Dictionary<Player, CombatSkillState> s_states = new();

    public static CombatSkillState GetState(Player player)
    {
        if (!s_states.TryGetValue(player, out var state))
        {
            state = new CombatSkillState();
            s_states[player] = state;
        }
        return state;
    }

    public static void ResetForPlayer(Player player) => s_states.Remove(player);
    internal static void ResetAll() => s_states.Clear();
}
