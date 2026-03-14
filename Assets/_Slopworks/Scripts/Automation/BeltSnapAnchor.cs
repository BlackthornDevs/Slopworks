using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Placement guide on a support prefab. Provides a snap position and direction
/// for belt placement. Tracks connected belts to enforce max connection limit.
/// </summary>
public class BeltSnapAnchor : MonoBehaviour
{
    [SerializeField] private int _maxConnections = 2;

    private readonly List<GameObject> _connectedBelts = new();

    /// <summary>
    /// World-space position for belt endpoint placement.
    /// </summary>
    public Vector3 WorldPosition => transform.position;

    /// <summary>
    /// World-space direction for belt tangent at this anchor.
    /// </summary>
    public Vector3 WorldDirection => transform.forward;

    /// <summary>
    /// Number of belts currently connected to this anchor.
    /// </summary>
    public int ConnectionCount
    {
        get
        {
            _connectedBelts.RemoveAll(b => b == null);
            return _connectedBelts.Count;
        }
    }

    /// <summary>
    /// True when the anchor has reached its maximum number of belt connections.
    /// </summary>
    public bool IsFull
    {
        get
        {
            _connectedBelts.RemoveAll(b => b == null);
            return _connectedBelts.Count >= _maxConnections;
        }
    }

    /// <summary>
    /// Register a belt as connected to this anchor.
    /// </summary>
    public void Connect(GameObject belt)
    {
        _connectedBelts.RemoveAll(b => b == null);
        if (belt != null && !_connectedBelts.Contains(belt))
            _connectedBelts.Add(belt);
    }

    /// <summary>
    /// Unregister a belt from this anchor.
    /// </summary>
    public void Disconnect(GameObject belt)
    {
        _connectedBelts.Remove(belt);
        _connectedBelts.RemoveAll(b => b == null);
    }
}
