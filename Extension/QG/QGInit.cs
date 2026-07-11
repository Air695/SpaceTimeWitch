namespace SpaceTimeWitch.Extension.QG;

/// <summary>
/// QG 情感系统初始化。
/// </summary>
public static class QGInit
{
    private static bool s_registered;

    public static void Register(string modId)
    {
        if (s_registered) return;
        s_registered = true;
        QGResources.Register(modId);
    }
}
