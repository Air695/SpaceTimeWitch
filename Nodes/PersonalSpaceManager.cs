namespace SpaceTimeWitch.Nodes;

/// <summary>
/// 个人空间管理器。每场战斗开始时重置状态。
/// </summary>
public static class PersonalSpaceManager
{
    public static int Version { get; private set; }

    internal static void BumpVersion() { Version++; }

    public static void ClearAll()
    {
        Version = 0;
    }
}
