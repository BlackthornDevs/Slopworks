/// <summary>
/// Plain data struct pushed from NetworkBuildController to IBuildStateReceiver.
/// Kevin adds fields when new state exists. Joe reads them in VisorBuildAdapter.
/// No logic, no dependencies on build controller internals.
/// </summary>
public struct BuildStateSnapshot
{
    public bool BuildMode;
    public bool DeleteMode;
    public bool ZoopMode;
    public bool ZoopStartSet;
    public string ToolName;
    public int RotationDegrees;
    public string BeltRoutingMode;
    public string SnapFilter;
    public string ValidationError;
    public string[] KeycapLabels;
}
