using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Direction a belt port faces for item flow.
/// </summary>
public enum BeltPortDirection
{
    Input,
    Output
}

/// <summary>
/// A connection point on a prefab where a belt can attach.
/// Placed as a child GameObject on machine, storage, and belt prefabs.
/// Position and forward direction come from the child transform.
/// Tracks connected belts to prevent multiple connections to the same port.
/// </summary>
public class BeltPort : MonoBehaviour
{
    [SerializeField] private BeltPortDirection _direction = BeltPortDirection.Input;
    [SerializeField] private int _slotIndex;
    [SerializeField] private string _slotLabel;
    [SerializeField] private int _maxConnections = 1;

    private readonly List<GameObject> _connectedBelts = new();

    public BeltPortDirection Direction
    {
        get => _direction;
        set => _direction = value;
    }

    public int SlotIndex
    {
        get => _slotIndex;
        set => _slotIndex = value;
    }

    public string SlotLabel
    {
        get => _slotLabel;
        set => _slotLabel = value;
    }

    /// <summary>
    /// World-space position of this port.
    /// </summary>
    public Vector3 WorldPosition => transform.position;

    /// <summary>
    /// World-space direction this port faces (outward from the building).
    /// For Output ports, this is the direction items leave.
    /// For Input ports, this is the direction items arrive from.
    /// </summary>
    public Vector3 WorldDirection => transform.forward;

    /// <summary>
    /// Number of belts currently connected to this port.
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
    /// True when this port has reached its maximum number of connections.
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
    /// Register a belt as connected to this port.
    /// </summary>
    public void Connect(GameObject belt)
    {
        _connectedBelts.RemoveAll(b => b == null);
        if (belt != null && !_connectedBelts.Contains(belt))
            _connectedBelts.Add(belt);
    }

    /// <summary>
    /// Unregister a belt from this port.
    /// </summary>
    public void Disconnect(GameObject belt)
    {
        _connectedBelts.Remove(belt);
        _connectedBelts.RemoveAll(b => b == null);
    }
}
