/// <summary>
/// Interface between NetworkBuildController and UI.
/// Kevin's controller pushes state. Joe's VisorBuildAdapter receives it.
/// </summary>
public interface IBuildStateReceiver
{
    void OnBuildStateChanged(BuildStateSnapshot state);
    void OnBuildModeEntered();
    void OnBuildModeExited();
}
