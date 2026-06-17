# Prototype Setup Notes

## Core Scene Objects

- `LevelManager`: one per scene. Handles `R` restart and level exit progression.
- `Player`: requires `Rigidbody2D`, `Collider2D`, `PlayerController`, and `PortalGun`.
- `PortalGun`: assign primary and secondary portal prefabs, a fire origin transform, and layer masks.
- `Portal`: prefab root should have a trigger `Collider2D`. The component uses its local `right` direction as the exit normal.

## Portal Placement Contract

The prototype allows wall placement from four aim directions, but placement is still rejected when:

- The ray misses a portal surface.
- The surface normal does not oppose the aim direction closely enough.
- The candidate portal space overlaps blocking geometry.
- The candidate portal overlaps another portal.
- The exit clearance area is obstructed.

This is the minimum needed to support "any wall" placement without accepting obviously unsafe corner and thin-wall placements.

## Recommended Layers

- `Solid`: walls, floors, ceilings, door bodies.
- `PortalSurface`: surfaces that may receive portals. In the first prototype this can be the same physical wall tilemap layer if the mask is configured accordingly.
- `Portal`: portal trigger colliders.
- `Player`: player body.
- `PuzzleObject`: weighted boxes and other moving puzzle objects.

## Level Design Constraints

- Keep early rooms axis-aligned. Slopes are out of scope for the first vertical slice.
- Give each portal placement surface at least one full portal height of clear space.
- Avoid one-tile-thick walls unless both sides are intentionally reachable.
- Add a visible restart path or rely on `R` while prototyping.
- Validate each level with both intended play and obvious exploit attempts: ceiling skip, floor momentum skip, corner insertion, and overlapping portals.
