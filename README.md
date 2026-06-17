# 2D 横版传送门解密原型

Unity 2D PC prototype scaffold for a side-scrolling puzzle platformer built around four-direction portal aiming.

## Current Scope

- Player movement: run, jump, fall, aim in four cardinal directions.
- Portal gun: primary and secondary portals, raycast placement, placement validation.
- Portal transfer: connected portal teleport with velocity remapping and cooldown.
- Puzzle primitives: pressure switch, puzzle door, weighted box.
- Level flow: restart current level and advance through build-index scenes.
- Debug overlay: aim direction, hit point, surface normal, and placement rejection reason.

## Controls

- Move: `A/D` or left/right arrows.
- Jump: `Space`.
- Aim: `W/A/S/D` or arrow keys.
- Fire primary portal: `Q` or left mouse.
- Fire secondary portal: `E` or right mouse.
- Restart level: `R`.

## Unity Setup

1. Open this folder in Unity as a 2D project, or copy `Assets/` into an existing Unity 2D project.
2. Open `Assets/Scenes/Level_01_Tutorial.unity`.
3. Press Play. The scene contains a runtime bootstrap object that creates the greybox player, portal templates, camera, level geometry, exit, and debug overlay.
4. Optional: when Unity licensing/editor execution is healthy, run `SidePortal > Build Playable Vertical Slice` to generate prefab-based scene assets from the editor tool.
5. Configure `PortalGun` masks if you build custom scenes:
   - `Portal Surface Mask`: layers that can receive portals.
   - `Placement Blocking Mask`: solid geometry and puzzle objects that must block portal placement.
   - `Portal Overlap Mask`: portal layer, used to reject overlapping portals.
6. See `Documentation/PrototypeSetup.md` for scene and level design details.

## First Playable Slice

`Level_01_Tutorial` is currently a runtime-generated greybox scene because local Unity batchmode execution was blocked by the licensing client. The gameplay content is still versioned in code and the scene is included in Build Settings.

Expected first-room solution:

1. Aim left and fire one portal onto the start back wall.
2. Aim right and fire the other portal onto the far exit wall.
3. Walk into the start-side portal to cross the wide pit.
4. Touch the green exit marker to complete the level.

## MVP Level Plan

- Level 1: movement, jump, four-direction aiming, place two portals.
- Level 2: cross a blocked space with portals.
- Level 3: use falling velocity to launch from another portal.
- Level 4: weighted box and pressure switch opens a door.
- Level 5: optional combined challenge.
