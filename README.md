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
2. Create scenes for the vertical slice under `Assets/Scenes`.
3. Add a `Player` GameObject with `Rigidbody2D`, `Collider2D`, `PlayerController`, and `PortalGun`.
4. Add two portal prefabs with `Collider2D` set to trigger and the `Portal` component.
5. Configure `PortalGun` masks:
   - `Portal Surface Mask`: layers that can receive portals.
   - `Placement Blocking Mask`: solid geometry and puzzle objects that must block portal placement.
   - `Portal Overlap Mask`: portal layer, used to reject overlapping portals.
6. See `Documentation/PrototypeSetup.md` for scene and level design details.

## MVP Level Plan

- Level 1: movement, jump, four-direction aiming, place two portals.
- Level 2: cross a blocked space with portals.
- Level 3: use falling velocity to launch from another portal.
- Level 4: weighted box and pressure switch opens a door.
- Level 5: optional combined challenge.
