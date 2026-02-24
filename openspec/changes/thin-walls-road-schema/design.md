## Context

Current level generation is based on wall cells (`W`) and empty cells, which leads to thick wall geometry and low map expressiveness. The change introduces a road-tile schema where passability is expressed by directional glyphs and `#` marks void. This impacts validation, map-to-grid parsing, and prefab spawning in Unity adapters under `Assets/Scripts/Levels/**`.

Constraints:
- Keep clean architecture boundaries: pure parsing/validation in core C# services, Unity API usage only in adapter components.
- Keep `S`/`F` semantics and centered world coordinate mapping unchanged.
- Existing request examples may break unless migrated.

## Goals / Non-Goals

**Goals:**
- Define deterministic directional connectivity for road glyphs.
- Validate map symbols and edge consistency before runtime spawn.
- Replace thick wall-cell generation with thin edge-wall generation.
- Preserve existing grid sizing, centering, and attempt bounds behavior.

**Non-Goals:**
- Reworking robot control logic or sensor models.
- Introducing procedural generation algorithms.
- Preserving backward compatibility with `W` schema in this change.

## Decisions

### Decision 1: Use explicit road glyph contract
- Decision: Represent traversable topology with explicit glyphs (`─│┌┐└┘├┤┬┴X`) and `#` for void.
- Rationale: Human-readable maps with deterministic semantics and no ambiguous "empty means maybe road".
- Alternative considered: Auto-infer road type from neighbors. Rejected because authoring and validation become harder to reason about.

### Decision 2: Derive walls from tile-edge connectivity
- Decision: Generate thin walls per cell edge, not per wall cell.
- Rationale: Produces compact geometry and naturally supports corridors, dead ends, and rooms (`X` clusters).
- Alternative considered: Keep `W` wall blocks and add overlays. Rejected due to geometry bloat and duplicate semantics.

### Decision 3: Strict validation on unknown symbols and mismatched edges
- Decision: Reject unknown symbols and enforce mutual neighbor compatibility before scene generation.
- Rationale: Fails fast with clear input errors and prevents malformed road graphs.
- Alternative considered: Lenient fallback to void. Rejected because silent correction hides map authoring mistakes.

### Decision 4: Infer `S` and `F` connectivity from valid neighboring roads
- Decision: Do not encode fixed direction sets for `S` and `F`; infer openings from compatible neighbors.
- Rationale: Keeps start/finish authoring flexible and avoids introducing extra start/finish orientation symbols.
- Alternative considered: Force directional start/finish symbols. Rejected as unnecessary map complexity.

## Risks / Trade-offs

- [Risk] Existing `W`-based maps stop working immediately. -> Mitigation: Document breaking change and provide migrated sample requests.
- [Risk] Unicode glyph entry can be awkward in some editors. -> Mitigation: Keep strict symbol table in docs and optionally support ASCII aliases in a follow-up.
- [Risk] Incorrect edge-wall transform/orientation in Unity can produce visual gaps. -> Mitigation: Centralize edge-to-transform mapping and validate with fixed test maps (corridor, T, room, dead end).

## Migration Plan

1. Add new map symbol model and edge-direction metadata in level generation core.
2. Update validator to enforce symbol set, single `S/F`, rectangular map, and mutual edge consistency.
3. Update grid factory/spawner to emit road tiles and thin edge walls.
4. Migrate request fixtures under `Builds/requests/**` to road schema.
5. Run manual Unity smoke maps: straight corridor, dead end, T-junction, closed room (`X` cluster), and mixed route from `S` to `F`.
6. If rollout must be reverted, restore previous `W` mapping in validator/factory and revert migrated fixtures.

## Open Questions

- Should we add an optional compatibility mode for legacy `W` maps behind a flag?
- Do we need ASCII fallbacks (`-`, `|`, etc.) in the same parser for easier CLI authoring?
- Should rooms built with `X` allow optional interior props/markers in a future change?
