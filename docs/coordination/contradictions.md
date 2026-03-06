# Contradictions and open questions

When either agent finds a conflict between docs, or needs an architectural decision made, write it here. The lead (Kevin's Claude) resolves entries by moving them to `decisions.md`.

Format:
```
## C-NNN: Short title

**Found by:** [joe/kevin]
**Date:** YYYY-MM-DD
**Doc A says:** ...
**Doc B says:** ...
**Options:**
1. ...
2. ...
**Recommendation:** ...
```

---

## Resolved

- C-001 through C-003: VContainer vs no DI, Addressables vs SceneManager, Legacy vs New Input System. See `decisions.md` D-001 through D-003.
- C-004: Missing server authority guards. Fixed in J-007 — all combat classes converted to NetworkBehaviour with IsServerInitialized guards, WeaponBehaviour damage routed through ServerRpc.
- C-005: FaunaController violates D-004. Fixed in J-008 — FaunaAI extracted as plain C#, 23 EditMode tests.
- C-006: GameObject.Find in combat code. Fixed in J-009 — all references wired via SerializeField, zero Find calls remain.
- C-007: GetComponent per melee attack. Fixed in J-010 — cached on target acquisition.
- C-008: Dev_Test bootstrapper question. Moot — StructuralPlaytestSetup no longer exists. JoePlaytestSetup replaces it per D-012 bootstrapper refactoring.
- C-009: Turret ghost not cleaned up on tool switch. Fixed by Kevin — added `RegisterToolCleanup(Action)` to PlaytestToolController. Both KevinPlaytestSetup and JoePlaytestSetup register `DestroyTurretGhost` during `RegisterToolHandlers`. Commit `255f2a7`.

---

## Open

## C-010: Redefine Joe's task scope to focus on art/world-building

**Found by:** joe
**Date:** 2026-03-06
**Issue:** Joe's task list (tasks-joe.md) has been mostly C# code tasks (turret simulation, tower data model, bug fixes). Joe's actual strength and intended role is maps, terrain, textures, asset sourcing, lore/story, and visual polish. Kevin has been handling all mechanics, multiplayer, and systems code anyway -- when Joe's code tasks got blocked, Kevin completed them himself (J-018 through J-021).
**Current state:** All Joe code tasks are complete. No pending tasks.
**Options:**
1. Redefine Joe's task scope: Joe focuses on terrain/levels, sourcing and integrating free asset packs, lore/story writing, visual polish, UI art. Kevin continues mechanics, multiplayer, simulation. Code tasks only go to Joe if they're directly related to his visual/world work (e.g., terrain generator scripts).
2. Keep mixed scope: Kevin assigns whatever's needed, could be code or art.
**Recommendation:** Option 1. Joe already built the terrain generator and PlaytestEnvironment. His next natural tasks are: finding post-apocalyptic asset packs, building tower floor layouts, creating overworld terrain, writing the game's backstory and lore. These don't require deep systems knowledge and avoid the dependency bottleneck where Joe's code tasks get blocked on Kevin's deliverables.
