#!/bin/bash
# Slopworks session start hook
# Outputs critical dev rules as a system message so Claude has them in context

cat << 'EOF'
SLOPWORKS_DEV_RULES: You are working in the Slopworks Unity game project.

CRITICAL RULES (always apply when writing C# code):

1. NEVER mutate ScriptableObjects at runtime. SOs are read-only static definitions.
   Per-instance state goes in ItemInstance struct, not the SO.

2. Factory simulation runs SERVER SIDE ONLY. Never run belt tick, machine tick,
   or power calculation logic in a code path that executes on clients.
   Check: if (!IsServerInitialized) return;

3. Belt items are NOT NetworkObjects. Store belt contents as SyncList<BeltItem>
   on the belt segment entity. Never spawn a NetworkObject per item.

4. Cross-scene communication uses ScriptableObject event bus (GameEventSO).
   Never use direct object references between scenes — Unity prohibits it and
   it causes NullReferenceExceptions on scene unload.

5. Three scenes, three concerns:
   - HomeBase scenes: factory grid, machines, conveyors, defenses
   - Building scenes: FPS level, fauna, MEP systems
   - Overworld scene: territory map, supply lines, threat display
   Supabase save/load happens at scene transition boundaries.

6. NetworkVariable vs RPC:
   - SyncVar/NetworkList: persistent state (inventory slots, machine status, craft progress)
   - ServerRpc: client-to-server commands (request pickup, configure recipe)
   - ClientRpc: server-to-client events (explosion sound, harvest VFX)
   Never use RPC for state that a late-joining player needs — use SyncVar.

7. build_version tagging: all game_worlds and game_sessions rows need
   build_version set from GameConfig.BuildVersion ("joe" or "kevin").

Reference docs: docs/reference/ in this repo.
EOF
