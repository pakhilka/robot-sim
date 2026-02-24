## Why

Current map schema is wall-cell based (`W/S/F`) and produces thick wall blocks that consume most of the level area. This makes maps harder to read and limits layout expressiveness such as compact corridors, dead ends, and room-like spaces.

## What Changes

- **BREAKING**: Replace wall-cell map semantics with road-tile semantics where `#` means void and road symbols define connectivity.
- Add support for road glyphs: `S`, `F`, `─`, `│`, `┌`, `┐`, `└`, `┘`, `├`, `┤`, `┬`, `┴`, `X`.
- Generate thin border walls on road edges where movement is not allowed, including map perimeter and road-to-void boundaries.
- Treat open edge facing void as a valid dead end (with terminal border wall).
- Keep `X` as a 4-way tile to support open room regions (adjacent `X` tiles form room interiors with walls on the outer boundary).
- Extend validation to enforce symbol set, rectangular map, single `S`/`F`, and mutual edge consistency between neighboring road tiles.

## Capabilities

### New Capabilities
- `road-tile-connectivity`: Defines directional connectivity model for road glyph symbols and thin-wall boundary generation from tile edges.

### Modified Capabilities
- `level-generation-from-map`: Update map schema, validation rules, and prefab spawning expectations from wall-cell semantics to road-tile semantics.

## Impact

- Affected core generation logic under `Assets/Scripts/Levels/Generation/**`.
- Affected level data contracts under `Assets/Scripts/Levels/Data/**`.
- Affected runtime spawn/orchestration paths that consume validated map grids.
- Existing request files using `W`/empty semantics must migrate to the new road schema.
