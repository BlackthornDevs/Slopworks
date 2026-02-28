# Player complaints analysis — factory + base defense games

Sourced from Steam reviews, Reddit, and developer postmortems on Satisfactory, Factorio, The Riftbreaker, They Are Billions, Mindustry, Dyson Sphere Program, V Rising, and Foundry.

---

## Belt and conveyor UX

**The universal friction pattern:** flow direction is not obvious at placement time. Players must predict the resulting direction before placing, then demolish and rebuild if wrong. There is no drag-to-reverse, no in-place rotation after placement, and no visual feedback that a belt is mis-aimed until items pile up.

**Throughput readability is the most-requested missing feature across the genre.** Players cannot tell at a glance whether a belt is at capacity, starved, or partially utilized. Finding bottlenecks requires manually tracing production lines from output back to input. Satisfactory's community built third-party calculators specifically because the game doesn't show throughput.

**Silent failure modes are the most frustrating category.** A splitter placed on an existing belt can silently break flow — no error, no warning. Pipes in Satisfactory fail silently based on network topology that the player cannot observe. Players describe tracing broken production lines as "the most unfun part" of these games.

**Specific Slopworks implications:**
- Belt placement must show directional preview before committing — arrow or flow indicator
- Belt snapping errors must produce visible feedback, not silent failures
- Build a throughput overlay from the start, not as a later feature
- The supply line UI on the overworld map (abstract, not physical) avoids many of these problems — lean into the abstraction

---

## First-person factory building

**Information blackout:** in first-person, the structure being built blocks line of sight to adjacent infrastructure. Players cannot see how a new building connects to the surrounding factory without physically stepping back. Ports, belt connections, and alignment are hardest to verify at the moment of placement.

**Motion sickness is a documented player loss vector.** Players who cannot play in first-person switch to isometric factory games (Dyson Sphere Program, Factorio) instead. The camera mode *transition* is itself a sickness risk if it involves sudden FOV change, camera position jump, or orientation mismatch.

**What players actually want from an overhead view is a planning mode, not a gameplay camera.** The specific request across multiple games: "I need to see what is jammed and where without physically walking through the factory." This is different from the isometric camera — it's a map/inspect mode.

**Specific Slopworks implications:**
- The FPS ↔ isometric toggle is the game's answer to the information blackout problem — verify it solves the "what's jammed" use case specifically
- Camera transition must not involve sudden FOV jump — cross-fade or animate smoothly, no instantaneous cut
- Consider a "factory map" mode distinct from the gameplay isometric camera — minimap or separate panel showing item flow on the grid
- Accessibility: document that players who get motion sick in FPS can use isometric for all factory work

---

## Wave defense failure modes

**Final wave difficulty spikes** are the genre's most consistent complaint. They Are Billions' final wave attacks from every perimeter point simultaneously — correct strategy for all previous waves is wrong strategy for the final wave. Players describe 3-4 hour sessions ending in an unavoidable wipe with no feedback.

**Predictable enemy behavior makes defense trivial, then boring.** The Riftbreaker's enemies walk in straight lines to the nearest building — no pathfinding variety, no flanking, no siege units. Players build one correct turret configuration and never have to think again.

**Difficulty spikes tied to progression events** (not timer) are the worst pattern. Completing a mission in The Riftbreaker immediately spawns enemies resistant to the turrets players built to handle earlier waves. Players feel punished for progressing.

**No communication about incoming wave composition** forces players to learn by dying. Mindustry players spent 3.5 hours on 44 waves only to lose to a guardian unit with unknown resistances. Players need enough information to make defensive decisions before the wave arrives.

**Specific Slopworks implications:**
- Threat meter scaling off expansion pace (not a fixed timer) is sound design — players control escalation rate
- Wave composition needs visible preview: incoming enemy type, direction, ETA
- Avoid the final-wave trap — difficulty should escalate continuously, not jump at a story beat
- Require players to adjust defensive layouts between waves, not just reinforce the same one
- Supply line attacks (fauna targeting lines between buildings) create the flanking pressure that keeps defense interesting

---

## Co-op multiplayer pitfalls

**Desync presentation matters as much as desync prevention.** Satisfactory's visual desync (players teleporting, belts disappearing visually but remaining physically) is worse than Factorio's deterministic crash, because it produces an inconsistent game state where both players think they're playing correctly. FishNet's server-authoritative model handles this better — the server is always correct, clients correct to server state.

**The "silent desync" is the hardest to debug.** Players report belts appearing deleted on one client while physically present on the server — blocking construction silently. This requires disconnect/reconnect to resolve. Server authority guard on all state mutations prevents this: clients never remove or modify state locally, they request changes via ServerRpc.

**Host dependency is the primary co-op complaint for casual groups.** If the host's machine degrades (late-game factory load), all clients suffer. FishNet's dedicated server support is the answer — but setting it up must be one-click, not technical.

**Griefing in co-op:** Satisfactory has no permissions system and declined to add public sessions because of griefing. V Rising's PvP is documented as a griefer's paradise on official servers. For Slopworks co-op (1-4 friends, no public sessions), griefing is less of a concern — but inventory griefing (dismantling other players' builds) needs consideration.

**Specific Slopworks implications:**
- `if (!IsServerInitialized) return;` on all simulation logic prevents client-side state divergence — this is already in the hard rules
- SyncVar/SyncList for all persistent state means late joiners get the correct state automatically — no desync on join
- No public session browser is fine — Satisfactory made the same call and it's defensible
- Consider a build lock: structures placed by a player require that player's permission to demolish in co-op

---

## Performance at scale

**Belt and conveyor simulation is the primary perf killer in factory games.** Every belt segment simulated as a separate entity creates per-object overhead that scales with factory size. Factorio's belt optimization (treating adjacent belts as a single entity, integer distances) produced the largest single performance gain in the game's history. Satisfactory's Unreal-based per-actor overhead never fully recovered.

**The lesson from both games:** belt simulation needs to be a specialized, batched system from day one. Standard entity simulation applied to belt items does not scale. This is exactly what the `SyncList<BeltItem>` on a per-segment NetworkObject does — one NetworkObject per segment, not per item.

**Pathfinding under wave pressure is a consistent CPU spike.** Biters/fauna that get stuck and repeatedly pathfind around obstacles cause sustained CPU load. Server-only AI (no client-side pathfinding) and NavMesh path caching both help.

**Autosave freezes** are a common complaint in late-game saves. This is a Unity `JsonUtility` / file I/O problem — write saves asynchronously using a background thread or coroutine, never on the main thread.

**Specific Slopworks implications:**
- The `SyncList<BeltItem>` belt pattern with integer ushort distances is already the right call — do not deviate
- Never spawn a NetworkObject per belt item — this is already a hard rule
- NavMesh baking must account for dynamic obstacles (placed walls, turrets) — consider partial rebake on build
- Save system must be async from day one — `File.WriteAllText` on main thread will freeze at scale

---

## New player experience

**Tutorial vs. freeplay access** creates a consistent drop-off: players skip the tutorial, hit a complexity wall, feel stupid, and quit. Factorio's forums have a dedicated "am I too stupid for this game?" thread pattern. The fix is not a longer tutorial — it's making the first session succeed without requiring prior knowledge.

**Spaghetti pressure** is the main reason players quit in the 5-20 hour range. Players build organically until the factory is unextendable, then face tear-down-or-quit. Neither game teaches layout patterns (main bus, modular city blocks) — players must find guides. Slopworks' overworld abstraction (supply lines, not physical belts, between buildings) reduces spaghetti pressure significantly.

**No undo** multiplied by **mistakes are expensive** creates player anxiety. The specific quote from Satisfactory players: "I cannot think while the game is running. This is some kind of pressure I can't stand." A demolish-and-refund model (materials returned when dismantling) reduces the cost of mistakes and makes experimentation feel safe.

**The "complexity wall" pattern:** games have one major mechanic that players hit and either understand or quit. Factorio's are train signaling and circuit networks. Satisfactory's are trains and power. For Slopworks, the likely wall is the supply line configuration UI on the overworld — setting output ratios, choosing destinations, understanding throughput. This UI needs to be extremely clear.

**Specific Slopworks implications:**
- Demolish-and-refund should be the default — no reason to punish repositioning
- The overworld supply line UI is the highest-risk complexity wall — design it carefully, test it with new players early
- The threat meter (expansion pace → wave intensity) is a better onboarding pressure system than a fixed timer — let new players build at their own pace before escalating
- FPS building at home base (small scale) before unlocking the supply network reduces the initial complexity load

---

## What Slopworks gets right by design

Several documented complaints in the genre are non-issues for Slopworks by virtue of design choices already made:

| Genre problem | Slopworks answer |
|---------------|-----------------|
| Physical conveyor spaghetti between buildings | Supply lines are abstract (overworld connections, not physical belts) |
| Final-wave difficulty spike | Threat meter = player-controlled escalation pace |
| Host dependency desync | FishNet server-authoritative with SyncVar/SyncList |
| First-person information blackout | FPS ↔ isometric toggle built into core design |
| Belt item NetworkObject spam | `SyncList<BeltItem>` per segment, not per item |
| Single-thread simulation ceiling | Factory sim server-only, decoupled from render frame rate |

The overworld abstraction (buildings as production nodes connected by supply lines, not by physical conveyors) is the single biggest genre differentiation from a player experience standpoint. It eliminates the most-complained-about problems: throughput readability, belt spaghetti, and belt placement UX.
