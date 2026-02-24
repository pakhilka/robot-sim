## 1. Road Tile Contract

- [x] 1.1 Add road symbol classification and direction metadata in `Assets/Scripts/Levels/Data/**` for `#`, road glyphs, `S`, and `F`
- [x] 1.2 Add helper logic to resolve directional openings for each traversable symbol and infer openings for `S`/`F` from valid neighbors

## 2. Map Validation

- [x] 2.1 Update `LevelMapValidator` to accept only the new symbol set and reject unknown symbols as `invalid_input`
- [x] 2.2 Keep rectangular and single `S`/`F` checks, adapted to the new schema
- [x] 2.3 Add mutual edge-consistency validation between adjacent traversable tiles

## 3. Grid & Thin Wall Generation

- [x] 3.1 Update level grid construction to use road/void semantics instead of wall/empty semantics
- [x] 3.2 Generate thin boundary edges from tile connectivity for road-to-void, perimeter, and mismatched edges
- [x] 3.3 Ensure dead ends are rendered with terminal thin walls
- [x] 3.4 Keep centered coordinate mapping and map-derived world bounds behavior unchanged

## 4. Unity Adapter Integration

- [x] 4.1 Update level prefab spawning flow to instantiate traversable tiles from the new schema
- [x] 4.2 Integrate thin edge wall spawn/orientation in Unity adapters without leaking Unity API into core logic
- [x] 4.3 Move `LevelPrefabSpawner` from `Levels/Components` to `Levels/Adapters` as a non-MonoBehaviour Unity adapter refactor
- [x] 4.4 Raise thin boundary walls by half wall height so they sit on top of floor instead of being centered through it
- [x] 4.5 Align scene/prefab assets for thin-wall rendering (`Bootstrap.unity`, `Wall.prefab`, `Road.prefab`)

## 5. Verification

- [x] 5.1 Validate in editor with manual smoke maps: corridor, dead end, T-junction, and `X`-room perimeter
- [ ] 5.2 Confirm attempt startup still fails fast with friendly `invalid_input` reasons for malformed schemas
- [x] 5.3 Update existing request fixtures to the new road schema in `Builds/requests/request-fail-out-of-bounds.json`, `Builds/requests/request-fail-timeout.json`, `Builds/requests/request-pass-straight.json`, and `Builds/requests/request-pass-with-two-turns.json`
- [x] 5.4 Validate provided spiral map schema and add new fixture `Builds/requests/request-pass-spiral-room.json`
