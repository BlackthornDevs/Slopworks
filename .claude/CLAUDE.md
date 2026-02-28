# Slopworks — Claude rules

Unity + FishNet co-op factory/survival game. Two-person team: Joe (jamditis) + Kevin (kamditis).

## Hard rules

**Never mutate ScriptableObjects at runtime.** SOs are read-only static definitions. All per-instance state goes in `ItemInstance` / `ItemSlot` structs.

**Never spawn a NetworkObject per belt item.** Belt contents are a `SyncList<BeltItem>` on the segment entity. One NetworkObject per belt segment.

**Factory simulation runs server-side only.** Clients display state synced from server via SyncVar/SyncList. Never replicate simulation logic to clients.

**Never use direct cross-scene references.** Scenes communicate via `GameEventSO` ScriptableObject event bus only.

**Never use RPCs for persistent state.** Late joiners don't receive RPCs. Use SyncVar/SyncList for anything a new client needs on join.

**No direct LLM API calls.** Use `claude -p` or `gemini -p` via subprocess if LLM calls are needed.

## Key patterns before writing C# code

Load the `slopworks-patterns` skill. It covers the ItemDefinitionSO/ItemInstance split, NetworkVariable vs RPC decision tree, belt sync, server authority, and cross-scene communication.

## Architecture questions

Load the `slopworks-architecture` skill. It covers the three-scene structure, which systems belong where, the FishNet + FishySteamworks stack, and Supabase integration points.

## Project structure

```
Assets/_Slopworks/Scripts/
  Automation/    — belt, machine, grid
  Combat/        — weapons, damage, health
  Network/       — FishNet + Supabase
  Player/        — character, camera
  World/         — gen, BIM import
  UI/            — HUD, menus
  Core/          — game manager, scene loader, save
```

## Branches

Joe works on `joe/main`, Kevin on `kevin/main`. Short-lived feature branches off your own main. Merge frequently — don't let branches run more than a day or two.

## Writing style

Sentence case everywhere. No emojis in source code, log messages, or comments.
