using Godot;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib;

namespace SpaceTimeWitch.Field;

public static class FieldCmd
{
    private static NCombatBackground? _savedBackground;
    private static Node? _fieldBackgroundNode;

    private static readonly MegaCrit.Sts2.Core.Logging.Logger Logger =
        RitsuLibFramework.CreateLogger("SpaceTimeWitch");

    internal static bool IsReplacing { get; private set; }

    public static void BeginReplace() => IsReplacing = true;
    public static void EndReplace() => IsReplacing = false;

    public static void ApplyBackground(FieldBackgroundType type, string path)
{
    var room = NCombatRoom.Instance;
    if (room?.Background == null) return;

    // ★ 防御：每次应用新场地前，先恢复旧场地，防止节点泄漏
    RestoreBackground();

    // 保存当前背景引用，并隐藏它
    _savedBackground = room.Background;
    if (GodotObject.IsInstanceValid(_savedBackground))
    {
        _savedBackground.Visible = false;
    }
    else
    {
        // 如果背景无效，就不保存了
        _savedBackground = null;
    }

    switch (type)
    {
        case FieldBackgroundType.Replace:
        {
            var texture = LoadTexture(path);
            if (texture == null)
            {
                // 加载失败，恢复之前隐藏的背景
                if (_savedBackground != null && GodotObject.IsInstanceValid(_savedBackground))
                    _savedBackground.Visible = true;
                _savedBackground = null;
                return;
            }

            var rect = new TextureRect
            {
                Texture = texture,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
                Name = "_FieldBackground",
                ZIndex = -100,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            room.AddChild(rect);
            rect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            _fieldBackgroundNode = rect;
            break;   // ★ 确认 break 存在
        }

        case FieldBackgroundType.Overlay:
        {
            var overlayTexture = LoadTexture(path);
            if (overlayTexture == null) return;

            var overlay = new TextureRect
            {
                Texture = overlayTexture,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
                Name = "_FieldOverlay",
                ZIndex = -50,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            room.AddChild(overlay);
            overlay.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            _fieldBackgroundNode = overlay;
            break;   // ★ 确认 break 存在
        }

        case FieldBackgroundType.Dynamic:
            break;
    }
}

public static void RestoreBackground()
{
    // 恢复保存的背景
    if (_savedBackground != null)
    {
        if (GodotObject.IsInstanceValid(_savedBackground))
        {
            _savedBackground.Visible = true;
        }
        _savedBackground = null;
    }

    // 释放自定义场地节点
    if (_fieldBackgroundNode != null)
    {
        if (GodotObject.IsInstanceValid(_fieldBackgroundNode))
        {
            _fieldBackgroundNode.QueueFree();
        }
        _fieldBackgroundNode = null;
    }
}

    private static Texture2D? LoadTexture(string path)
    {
        if (!ResourceLoader.Exists(path))
        {
            Logger.Warn($"Field background not in PCK: {path}");
            return null;
        }
        return ResourceLoader.Load<Texture2D>(path);
    }
}
