# Supabase schema — reference

Supabase is used for persistent world state and lobby discovery only. In-session live state stays in FishNet.

## Tables

### game_sessions

Lobby discovery. One row per active or recent session.

```sql
create table game_sessions (
  id           uuid primary key default gen_random_uuid(),
  host_user_id uuid references auth.users,
  status       text not null default 'lobby',  -- 'lobby' | 'active' | 'ended'
  world_seed   bigint not null,
  connection_info jsonb,  -- { "steamLobbyId": "..." }
  max_players  int not null default 4,
  created_at   timestamptz default now(),
  updated_at   timestamptz default now()
);
```

`connection_info.steamLobbyId` is how joining clients find the FishNet session.

### session_players

Tracks who is in each session for reconnection handling.

```sql
create table session_players (
  id           uuid primary key default gen_random_uuid(),
  session_id   uuid references game_sessions,
  user_id      uuid references auth.users,
  steam_id     text,
  status       text not null default 'connected',  -- 'connected' | 'disconnected'
  joined_at    timestamptz default now(),
  left_at      timestamptz
);
```

### world_state

Persistent world — what has been built, claimed, or modified.

```sql
create table world_state (
  id           uuid primary key default gen_random_uuid(),
  session_id   uuid references game_sessions,
  world_seed   bigint not null,
  placed_buildings jsonb,  -- array of { buildingType, chunkId, x, y, rotation }
  claimed_territory jsonb, -- array of { chunkId, x, y, claimedAt }
  resource_nodes jsonb,    -- array of { nodeId, purity, position }
  threat_level int default 0,
  updated_at   timestamptz default now()
);
```

### player_saves

Per-player persistent state. Serializes `definitionId` strings, never SO references.

```sql
create table player_saves (
  id               uuid primary key default gen_random_uuid(),
  user_id          uuid references auth.users,
  session_id       uuid references game_sessions,
  save_version     int not null default 1,
  inventory        jsonb,  -- array of { definitionId, count, instanceId, durability, quality }
  hotbar           jsonb,
  discovered_recipes jsonb,  -- array of recipeId strings
  player_stats     jsonb,  -- health, stamina, position (last logout)
  updated_at       timestamptz default now()
);
```

## Autosave strategy

Write to Supabase on:
- Fixed interval (every 5 minutes during active play)
- Player disconnect (`OnClientDisconnect` callback)
- Session end (`StopServer`)
- Building successfully claimed (important milestone, worth an immediate save)

Use upsert (INSERT ... ON CONFLICT DO UPDATE) for `world_state` and `player_saves` — one row per session/user, always overwrite.

## Config file

`Assets/StreamingAssets/supabase-config.json` (gitignored — copy from `supabase-config.template.json`):

```json
{
  "url": "https://[project].supabase.co",
  "anonKey": "..."
}
```

The anon key is safe to ship in the game client. Row Level Security on Supabase tables handles access control. Never ship the service role key.

## Client pattern in Unity

```csharp
// SaveSystem.cs (in Core scene, server-only operations)
public class SaveSystem : NetworkBehaviour {
    private Supabase.Client _client;

    public override void OnStartServer() {
        var config = LoadConfig();  // from StreamingAssets
        _client = new Supabase.Client(config.url, config.anonKey);
    }

    public async Task SaveWorldState(WorldStateData data) {
        if (!IsServerInitialized) return;
        await _client
            .From<WorldState>()
            .Upsert(data.ToRow());
    }
}
```

The Supabase C# SDK: `https://github.com/supabase-community/supabase-csharp`
