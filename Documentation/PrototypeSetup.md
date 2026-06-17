# Prototype Setup Notes

## Core Scene Objects

- `LevelManager`: one per scene. Handles `R` restart and level exit progression.
- `Player`: requires `Rigidbody2D`, `Collider2D`, `PlayerController`, and `PortalGun`.
- `PortalGun`: assign primary and secondary portal prefabs, a fire origin transform, and layer masks.
- `Portal`: prefab root should have a trigger `Collider2D`. The component uses its local `right` direction as the exit normal.
- `PortalAnchor`: fixed valid placement point. Mouse shots must hit one of these anchors to create a portal.
- `GridLevelData`: ScriptableObject level source containing placed blocks, spawn, exit, and legal portal edges.
- `GridLevelBuilder`: runtime builder that generates the playable greybox scene from `GridLevelData`.
- `GridLevelSceneBootstrap`: scene entrypoint used by `Level_01_Tutorial`.

## First Playable Scene

Open `Assets/Scenes/Level_01_Tutorial.unity` and press Play. The scene reads `Assets/Data/Levels/Level_01_Tutorial.asset` and creates the first test room automatically:

- Start floor and back wall on the left.
- A wide pit that cannot be crossed by a normal jump.
- Exit floor and back wall on the right.
- A green exit trigger that shows completion text.
- Runtime-created portal templates assigned to the player portal gun.
- Semi-transparent portal anchors on the start wall, exit wall, and a ceiling test point.

## Grid Level Editor

Open `SidePortal > Grid Level Editor` to edit `GridLevelData` assets. The editor works with placed blocks, not individual cell painting:

- Place solid or obstacle blocks using `1x1`, `2x1`, `1x2`, or `2x2` presets.
- Delete blocks.
- Set player spawn and exit cells.
- Toggle portal markers on block edges.

Portal markers are rejected on edges that touch any other block. If a new block is placed against an existing marked edge, invalid portal markers are removed automatically.

## Portal Placement Contract

The prototype looks like free mouse shooting, but portal placement only succeeds when the ray hits a valid `PortalAnchor`. Placement is rejected when:

- The ray misses all portal anchors.
- The hit anchor is disabled.
- The hit anchor does not allow the requested portal type.
- The candidate portal space overlaps blocking geometry.
- The candidate portal overlaps another portal.
- The exit clearance area is obstructed.

This keeps level design controllable while still letting the player aim freely with the mouse.

## Recommended Layers

- `Solid`: walls, floors, ceilings, door bodies.
- `PortalAnchor`: fixed anchor points that may receive portals.
- `Portal`: portal trigger colliders.
- `Player`: player body.
- `PuzzleObject`: weighted boxes and other moving puzzle objects.

## Level Design Constraints

- Keep early rooms axis-aligned. Slopes are out of scope for the first vertical slice.
- Give each portal placement surface at least one full portal height of clear space.
- Avoid one-tile-thick walls unless both sides are intentionally reachable.
- Add a visible restart path or rely on `R` while prototyping.
- Validate each level with both intended play and obvious exploit attempts: ceiling skip, floor momentum skip, corner insertion, and overlapping portals.
