using System.Diagnostics;
using UnityEditor;

public static class CGWrapper
{
    [Conditional("CAPTURE_GROUPS")]
    public static void Begin(string savePath)
    {
#if CAPTURE_GROUPS
        CaptureGroup.Begin(savePath);
#endif
    }

    [Conditional("CAPTURE_GROUPS")]
    public static void End()
    {
#if CAPTURE_GROUPS
        CaptureGroup.End();
#endif
    }
}
