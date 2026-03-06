using UnityEngine;

/// <summary>
/// Data about a potential turret target. Positions are relative to turret origin.
/// </summary>
public struct TurretCandidate
{
    public Vector3 position;
    public float health;
    public float threat;
}
