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

---

## Open

## C-004: Missing server authority guards in combat systems

**Found by:** kevin (code review of J-003 through J-006 on master)
**Date:** 2026-02-28
**CLAUDE.md says:** "Factory simulation runs server-side only. Belt tick, machine tick, power calculation, and crafting progress run under `if (!IsServerInitialized) return;`. Clients display state synced via SyncVar/SyncList -- they never simulate."
**Joe's code does:** `EnemySpawner`, `WaveControllerBehaviour`, and `FaunaController` all spawn/destroy NetworkObjects without `IsServerInitialized` guards. `WeaponBehaviour` calls `healthComponent.TakeDamage()` directly on the client instead of through a `ServerRpc`.
**Impact:** Clients will crash or duplicate spawns in multiplayer. Damage can be applied by any client without server validation.
**Fix required:** Add `if (!IsServerInitialized) return;` guards to all spawning, wave, and AI tick methods. Route damage through a `ServerRpc`.

## C-005: FaunaController violates D-004 pattern

**Found by:** kevin (code review of J-005 on master)
**Date:** 2026-02-28
**decisions.md D-004 says:** Pure C# simulation classes + thin MonoBehaviour wrappers. Simulation logic must be testable in EditMode without MonoBehaviour dependencies.
**Joe's code does:** `FaunaController` is a single MonoBehaviour mixing simulation logic (threat evaluation, attack timing, state transitions, pack coordination) with Unity lifecycle. Not testable in EditMode.
**Impact:** AI logic is untestable without PlayMode. Bugs will be harder to catch and reproduce.
**Fix required:** Extract simulation logic into `FaunaAI` (pure C#), keep `FaunaController` as thin wrapper.

## C-006: GameObject.Find and FindAnyObjectByType in combat code

**Found by:** kevin (code review of J-003/J-004 on master)
**Date:** 2026-02-28
**CLAUDE.md says:** "Never use direct cross-scene references. Unity doesn't allow them, and the three-scene structure means objects in one scene can be unloaded at any time."
**Joe's code does:** `PlayerHUD` uses `FindAnyObjectByType<HealthComponent>()`. `WeaponBehaviour` uses `GameObject.Find` patterns. These assume single-scene and break in multi-scene.
**Impact:** Will return null or wrong objects when scenes load/unload independently.
**Fix required:** Use dependency injection or `GameEventSO` event bus to wire references. Player should receive its own `HealthComponent` reference through its spawn setup, not global search.

## C-007: GetComponent called per melee attack

**Found by:** kevin (code review of J-005 on master)
**Date:** 2026-02-28
**CLAUDE.md says:** "Never cache GetComponent, FindObjectOfType, or FindObjectsOfType results inside Update or FixedUpdate. Cache in Awake or Start."
**Joe's code does:** `FaunaController` calls `GetComponent<HealthComponent>()` on the target every melee attack instead of caching.
**Impact:** Unnecessary allocation per attack. At scale with many fauna this adds up.
**Fix required:** Cache the target's `HealthComponent` when target is acquired, clear on target change.

## C-008: Dev_Test scene should use StructuralPlaytestSetup instead of a separate bootstrapper

**Found by:** joe
**Date:** 2026-03-02
**D-009 says:** "Each developer has one playtest scene that grows each phase. Kevin's scene is StructuralPlaytest (building + automation + inventory + crafting + combat). Joe's scene is Dev_Test (combat focus)."
**Problem:** Dev_Test currently has a separate bootstrapper (DevTestPlaytestSetup) with only combat features. This means Joe's scene is missing factory automation, turrets, building exploration, supply chain, and the build page — all features that exist in Kevin's StructuralPlaytestSetup. Joe cannot test his turret work (J-013 through J-015) in his own scene. The two scenes are out of sync, and Dev_Test falls behind master as Kevin adds features.
**Options:**
1. **Dev_Test uses StructuralPlaytestSetup directly.** Swap the component. Both scenes use one bootstrapper. Zero code duplication. Scenes start identical and diverge only when dev-specific features are added. Joe's turrets, tower features, etc. would be tested by adding them to StructuralPlaytestSetup (which already has turrets).
2. **Fork StructuralPlaytestSetup into DevTestPlaytestSetup.** Copy ~1500 lines. Two independent copies to maintain. Every change Kevin makes must be manually mirrored in Joe's copy, or the scenes drift apart.
3. **Keep Dev_Test combat-only per current D-009.** Joe tests turrets only through Kevin's scene. Dev_Test stays lightweight but can't verify Joe's factory-integrated features.
**Recommendation:** Option 1. One bootstrapper, two scenes. The "combat focus" distinction in D-009 was written before Joe built turrets (factory-integrated combat). Now that Joe's features touch factory automation, keeping a separate combat-only bootstrapper means Joe can never test his own work end-to-end. Using StructuralPlaytestSetup directly keeps both scenes in sync with zero maintenance cost.
