# Scene structure — detailed reference

## Full scene hierarchy

```
Scenes/
  Core/
    Core_Network.unity       — NetworkManager, FishNet config, FishySteamworks transport
    Core_GameManager.unity   — GameManager, session state, threat meter, wave controller
  HomeBase/
    HomeBase_Terrain.unity   — ground heightmap, resource nodes at edges
    HomeBase_Grid.unity      — factory grid, belt network, machine simulation tick loop
    HomeBase_UI.unity        — HUD, build menu, inventory UI, hotbar
    HomeBase_Lighting.unity  — directional light, ambient, baked GI
  Buildings/
    Building_Template.unity  — base scene: navmesh, lighting rig, MEP template
    [BuildingName].unity     — one scene per reclaimed building
  Overworld/
    Overworld_Map.unity      — territory tiles, building icons, supply lines
    Overworld_UI.unity       — overworld HUD, dossier panel, threat display
```

## Additive loading order

1. `Core_Network.unity` — loaded first, never unloaded
2. `Core_GameManager.unity` — loaded second, never unloaded
3. Active world scene (HomeBase / Building / Overworld) — loaded/unloaded on transitions
4. Active UI scene — loaded with its paired world scene

Always load Core scenes first before any world scene. The NetworkManager must exist before any NetworkObject spawns.

## Scene manager pattern

```csharp
public class SceneLoader : NetworkBehaviour {
    // Server initiates — all clients follow
    public void TransitionToHomeBase() {
        if (!IsServerInitialized) return;
        NetworkManager.SceneManager.LoadScene("HomeBase_Terrain", LoadSceneMode.Additive);
        NetworkManager.SceneManager.LoadScene("HomeBase_Grid", LoadSceneMode.Additive);
        NetworkManager.SceneManager.LoadScene("HomeBase_UI", LoadSceneMode.Additive);
    }

    public void TransitionToBuilding(string buildingSceneName) {
        if (!IsServerInitialized) return;
        // Unload Home Base scenes first, then load building scene
    }
}
```

## Cross-scene event bus setup

Event assets live in `Assets/_Slopworks/ScriptableObjects/Events/`. Managers in any scene hold serialized references to these assets.

```
Events/
  PlayerDied.asset
  BuildingClaimed.asset
  WaveStarted.asset
  WaveEnded.asset
  SceneTransitionRequested.asset
  ThreatLevelChanged.asset
  BuildingConnectedToNetwork.asset
  PowerGridUpdated.asset
```

Any MonoBehaviour in any scene can hold a serialized reference to these assets and raise or subscribe to them. No FindObjectOfType, no singletons, no scene dependencies.

## Scene ownership during parallel development

To avoid Unity merge conflicts, assign scene ownership:

| Scene | Primary owner |
|-------|--------------|
| Core/ | Joe (networking setup) |
| HomeBase_Grid.unity | Joe (automation, belts) |
| HomeBase_Terrain.unity | Kevin (terrain, resources) |
| Buildings/ | Kevin (BIM pipeline) |
| Overworld/ | Joe (map, territory) |
| UI scenes | whoever needs it, coordinate |

When both need the same scene: use prefabs for your work, commit the prefab, have the owner place it in the scene.

## Prefab strategy

Keep scenes thin — most gameplay objects should be prefabs placed in scenes, not objects created directly in the scene hierarchy. This reduces scene file size and merge conflicts.

```
Prefabs/
  Player/
    PlayerCharacter.prefab      — NetworkObject, player controller
  Machines/
    ConveyorBelt.prefab
    Furnace.prefab
    Splitter.prefab
  Buildings/
    [BuildingType].prefab
  UI/
    HUD.prefab
    BuildMenu.prefab
  FX/
    ExplosionVFX.prefab
```
