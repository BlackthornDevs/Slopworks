# Slopworks -- Game Design Document

**Date:** 2026-02-27
**Team:** Kevin (kamditis) + Brother (jamditis)
**Engine:** Unity
**Platform:** Desktop
**Players:** 1-4 players (solo + co-op)

---

## Elevator Pitch

A post-apocalyptic base-building survival game where you reclaim real buildings by restoring their mechanical systems. Explore and clear hostile structures in first-person, switch to isometric view to design Satisfactory-style automation networks connecting your growing territory. Defend your factory base from escalating fauna waves. Play solo or drop-in/drop-out co-op.

**What makes it unique:** Every reclaimable building is modeled from real BIM data -- real duct layouts, real pipe routing, real mechanical rooms. No other game can offer that level of environmental authenticity.

---

## World Structure

Three separate loaded spaces, not seamless:

### Home Base

Your factory and fortress. Its own loaded map. Satisfactory-style freeform building across the entire area.

- **Foundations first:** Lay foundation tiles on terrain. Everything snaps to the foundation grid.
- **Vertical stacking:** Stack foundations with spacing for multi-story factories. No structural integrity physics -- things float, no collapse.
- **Batch placement:** Zoop-style placement for laying multiple foundations/walls quickly.
- **Machines on grid:** Crafting stations, conveyors, splitters, mergers, storage bins all snap to foundations.
- **Walls optional:** Enclose areas for organization or defense. Doorways, windows, ramps, walkways. Purely functional/aesthetic, no room bonuses.
- **Defenses integrated:** Turrets and traps snap to foundations and walls. Power lines route from generators. Defensive perimeter grows organically with the factory.

The production layout IS the base design. Your factory shape becomes your defense layout.

### Reclaimed Buildings

Separate maps generated from real BIM building models. You travel to them from the overworld. Each building is a self-contained level with its own fauna, hazards, and MEP systems to restore.

### Overworld / Network Map

Isometric view showing your territory, supply lines, connected buildings, threat levels, and the unexplored frontier. Used for scouting, logistics planning, and managing your network.

---

## Camera System

Toggleable between two modes:

- **First-person:** Exploration, combat, building interiors, close-up base building and machine placement. Used during building breaches and base defense waves.
- **Isometric:** Network/overworld management, base layout overview, automation design, scouting the frontier.

---

## Core Gameplay Loop

```
SCOUT    -- Isometric overworld. Identify building types and threat levels.
BREACH   -- First-person. Enter an unexplored building.
CLEAR    -- FPS combat against hostile fauna nested inside.
RESTORE  -- Fix MEP systems to bring the building online.
CONNECT  -- Build supply lines (pipes, conveyors, power cables) to your network.
AUTOMATE -- Optimize production at building and hub level.
DEFEND   -- Protect home base and supply lines from fauna waves.
EXPAND   -- Scout next building, repeat with escalating difficulty.
```

---

## Automation System (Distributed Production Network)

Every reclaimed building is a production node in a supply chain. Buildings process raw materials into ingots, parts, and components. Those outputs ship to your home base OR to other buildings that need them as inputs for higher-tier production. The whole network is a distributed factory.

### Building Nodes (Managed from Overworld)

Each reclaimed building becomes a configurable production facility. You manage them from the isometric overworld UI -- set recipes, assign output destinations, monitor throughput. You go inside buildings to clear and restore them, not to lay conveyors.

**Building types and example outputs:**
- Power plant: generates electricity (required by other buildings)
- Foundry: smelts raw scrap into iron/copper/steel ingots
- Warehouse: sorts and distributes raw salvage to other buildings
- Machine shop: turns ingots into mechanical components
- Water treatment: supplies clean water (required by some processes)
- Electronics lab: produces circuit boards from copper ingots + components
- Hospital: produces medical supplies from chemicals + water
- Office building: generates research points

**Per-building configuration (overworld UI):**
- Set production recipe (what to make)
- Set output destinations (which buildings/hub receive the product)
- Set output ratios (e.g., 60% to Machine Shop, 40% to Hub)
- View throughput rates and bottleneck warnings
- Upgrade production capacity

### Supply Lines (Inter-Building Logistics)

Supply lines connect buildings and carry specific products between them:

- Each supply line carries one product type
- Throughput rate based on line capacity and distance
- Longer supply lines are more vulnerable to fauna attacks
- Can be upgraded for higher throughput
- Configurable from overworld: click a line to see what it carries, reroute, or upgrade

**Example supply chain:**
```
Warehouse (raw scrap) ---> Foundry (smelts into ingots)
                              |
                              +--> Machine Shop (ingots -> components)
                              |       |
                              |       +--> Home Base (components -> weapons/armor)
                              |       +--> Electronics Lab (components + copper -> circuits)
                              |                |
                              |                +--> Home Base (circuits -> turrets/advanced gear)
                              |
                              +--> Home Base (ingots -> plates -> building materials)
```

### Home Base Factory (Satisfactory-Style Building)

Your home base is one node among many -- it can smelt, process, and assemble just like any building. But it's also where you live, defend, and do personal crafting. The key difference: inside your home base, you get full Satisfactory-style freeform factory building.

- Foundations, machines, conveyors, splitters, mergers on a snap grid
- Place smelters, assemblers, workbenches wherever you want
- Route conveyors between machines for multi-step crafting
- Receives intermediate products from the network (ingots, components, circuits)
- Also receives raw materials if you want to process locally
- Final assembly for weapons, tools, armor, turrets, advanced MEP parts
- Research/tech tree unlocks new machines and recipes
- You COULD do everything at the hub, or distribute it across your network

---

## Defense System

### Defensive Structures

- **Auto-turrets:** Snap to foundations/walls. Require power + ammo supply. Primary ranged defense.
- **Spike walls / barbed wire:** Passive damage to enemies on contact. Slows advances.
- **Reinforced gates:** Controlled choke points. Upgradeable.
- **Landmines:** Crafted at fab shop. One-time-use area denial.
- **Spotlights:** Reveal incoming enemies at range. Require power.

### Power Dependency

- Turrets, spotlights, and automated defenses need electricity from generators.
- Lose power = defenses go dark. Generators are critical infrastructure.
- Creates tactical decisions: where to place generators, how to route power, what to prioritize when under strain.

### Threat Meter

A global threat level that drives wave intensity:

**Threat rises when you:**
- Connect new buildings to your network
- Increase resource throughput
- Expand territory into the frontier

**Higher threat means:**
- More frequent attack waves
- Tougher fauna types
- Multi-direction assaults
- Boss-tier creature spawns

**Baseline pressure:** Waves still come on a schedule even at low threat. You cannot turtle indefinitely. But you control the escalation pace by choosing when and how aggressively to expand.

---

## Combat

### Phase 1 (Launch)

- Hostile fauna have nested in abandoned buildings and roam the frontier
- First-person combat during building breaches and base defense waves
- Fauna types scale with distance from hub and building difficulty

### Future Updates (Architecture Prepped)

- Environmental hazards: collapsing structures, toxic leaks, electrical dangers
- Rival factions: other survivor groups competing for buildings, diplomacy or conflict

---

## Multiplayer

- **1-4 players.** Solo or co-op with up to 3 friends.
- **Drop-in/drop-out,** Satisfactory style. Persistent world, shared progress.
- **No restarts,** no roguelike resets. Your world is yours.
- Split up across buildings and base, or squad up for a breach together.

---

## BIM Pipeline (Kevin's Secret Weapon)

Real Revit/Navisworks building models exported into Unity as explorable levels:

- Thousands of real building models available as source material
- Real duct layouts, pipe routing, mechanical rooms, electrical panels
- MEP system restoration is grounded in actual building systems
- Point cloud data can generate "ruined" building variants (damaged, overgrown, collapsed sections)
- Every building feels different because they ARE different real buildings
- No other indie game can replicate this level of environmental authenticity

---

## Brother's Contributions (jamditis)

Leveraging journalism, AI, voice, and mapping expertise:

- **AI-generated building dossiers:** Each building has procedural history and narrative. Who built it, what happened, what's inside.
- **Voice interface:** AudioBash technology adapted for in-game voice commands or NPC comms.
- **Territory mapping:** GeoSprite / interactive mapping experience for the overworld and network visualization.
- **Automation scripting:** Python/TypeScript background for tooling, procedural generation, and backend systems.

---

## Key Influences

| Game | What We Take | What We Change |
|------|-------------|----------------|
| **Satisfactory** | Factory building, conveyor automation, foundation grid, co-op | Post-apocalyptic setting, combat, real buildings |
| **Riftbreaker** | Base defense waves, turrets, action combat | Persistent progression (no restarts), expansion-driven threat |
| **Stardew Valley** | Relaxing progression, always something to do | Industrial scale, combat layer |
| **Halo** | FPS combat feel, co-op campaign | Survival context, base building |
| **Diablo** | Loot progression, enemy scaling | First-person perspective, factory systems |
| **V Rising** | Territory expansion, conquest feel | Satisfactory-style building instead of room-based |

---

## Technical Notes

- **Engine:** Unity
- **Separate scenes:** Home base, each building, and overworld are independent loaded maps
- **No seamless streaming required** between home base and buildings
- **Grid snapping system** for foundation-based building
- **BIM import pipeline** needed: Revit/Navisworks -> Unity-compatible format
- **Networking:** Steam-based co-op (peer-to-peer or dedicated, TBD)

---

## Open Questions for Future Sessions

- Art style / visual direction (realistic, stylized, low-poly?)
- Specific fauna designs and behavior patterns
- Tech tree structure and progression curve
- Loot / equipment system depth
- Audio and music direction
- ~~Name finalization~~ -- **Slopworks**. Locked.
- Repository structure and project setup
