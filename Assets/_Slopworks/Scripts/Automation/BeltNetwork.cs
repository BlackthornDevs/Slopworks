using System.Collections.Generic;

/// <summary>
/// Manages connections between IItemSource and IItemDestination endpoints,
/// transferring items from sources to destinations. Plain C# class per D-004.
///
/// Each connection can hold one item in transit. If the destination
/// rejects an insert (full), the item is held and retried on the next tick.
/// </summary>
public class BeltNetwork
{
    private struct BeltConnection
    {
        public IItemSource Source;
        public IItemDestination Destination;
        public string HeldItemId;
    }

    private readonly List<BeltConnection> _connections = new List<BeltConnection>();

    public int ConnectionCount => _connections.Count;

    /// <summary>
    /// Register a connection from any IItemSource to any IItemDestination.
    /// Duplicate connections (same source and destination pair) are ignored.
    /// </summary>
    public void Connect(IItemSource source, IItemDestination destination)
    {
        if (source == null || destination == null)
            return;

        // Check for duplicate
        for (int i = 0; i < _connections.Count; i++)
        {
            if (_connections[i].Source == source && _connections[i].Destination == destination)
                return;
        }

        _connections.Add(new BeltConnection
        {
            Source = source,
            Destination = destination,
            HeldItemId = null
        });
    }

    /// <summary>
    /// Register a connection from the output of one belt to the input of another.
    /// Wraps in BeltOutputAdapter/BeltInputAdapter for backward compatibility.
    /// Duplicate connections (same from and to pair) are ignored.
    /// </summary>
    public void Connect(BeltSegment from, BeltSegment to)
    {
        if (from == null || to == null)
            return;

        if (IsConnected(from, to))
            return;

        Connect(new BeltOutputAdapter(from), new BeltInputAdapter(to));
    }

    /// <summary>
    /// Remove a connection between two belt segments.
    /// </summary>
    public void Disconnect(BeltSegment from, BeltSegment to)
    {
        for (int i = _connections.Count - 1; i >= 0; i--)
        {
            var conn = _connections[i];
            if (conn.Source is BeltOutputAdapter outputAdapter &&
                conn.Destination is BeltInputAdapter inputAdapter &&
                outputAdapter.Belt == from && inputAdapter.Belt == to)
            {
                _connections.RemoveAt(i);
                return;
            }
        }
    }

    /// <summary>
    /// Remove a connection by source/destination reference.
    /// </summary>
    public void Disconnect(IItemSource source, IItemDestination destination)
    {
        for (int i = _connections.Count - 1; i >= 0; i--)
        {
            if (_connections[i].Source == source && _connections[i].Destination == destination)
            {
                _connections.RemoveAt(i);
                return;
            }
        }
    }

    /// <summary>
    /// Check if a connection exists between two belt segments.
    /// </summary>
    public bool IsConnected(BeltSegment from, BeltSegment to)
    {
        for (int i = 0; i < _connections.Count; i++)
        {
            var conn = _connections[i];
            if (conn.Source is BeltOutputAdapter outputAdapter &&
                conn.Destination is BeltInputAdapter inputAdapter &&
                outputAdapter.Belt == from && inputAdapter.Belt == to)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Process all connections. For each connection:
    /// - If holding an item from a previous failed insert, retry inserting it.
    /// - Otherwise, if the source has an item available, extract it
    ///   and try to insert into the destination.
    /// - If the destination rejects the insert, hold the item until next tick.
    /// </summary>
    public void Tick()
    {
        for (int i = 0; i < _connections.Count; i++)
        {
            var conn = _connections[i];

            if (conn.HeldItemId != null)
            {
                // Retry inserting the held item
                if (conn.Destination.TryInsert(conn.HeldItemId))
                {
                    conn.HeldItemId = null;
                    _connections[i] = conn;
                }
                continue;
            }

            if (!conn.Source.HasItemAvailable)
                continue;

            if (!conn.Source.TryExtract(out string itemId))
                continue;

            if (!conn.Destination.TryInsert(itemId))
            {
                // Destination rejected, hold item until next tick
                conn.HeldItemId = itemId;
            }

            _connections[i] = conn;
        }
    }
}
