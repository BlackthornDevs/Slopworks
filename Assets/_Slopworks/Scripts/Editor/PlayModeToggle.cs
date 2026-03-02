using UnityEditor;

public static class PlayModeToggle
{
    [MenuItem("Slopworks/Enter Play Mode")]
    public static void EnterPlayMode()
    {
        if (!EditorApplication.isPlaying)
            EditorApplication.isPlaying = true;
    }

    [MenuItem("Slopworks/Exit Play Mode")]
    public static void ExitPlayMode()
    {
        if (EditorApplication.isPlaying)
            EditorApplication.isPlaying = false;
    }

    [MenuItem("Slopworks/Run Validation")]
    public static void RunValidation()
    {
        EditorPrefs.SetBool("PlaytestValidator_RunOnPlay", true);
        EditorApplication.isPlaying = true;
    }
}
