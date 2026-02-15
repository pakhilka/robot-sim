## Why

Local-brain behavior currently stops in front of obstacles, which is not aligned with the desired classroom scenario.
We need deterministic obstacle-avoidance behavior for local validation runs: when an obstacle is close, the robot should turn right instead of stopping.

## What Changes

### 1) Local brain decision rule update
- Update local brain logic to use a right-turn rule based on front distance.
- New rule: when `distanceFront <= 5.0` meters, local brain returns a right-turn motor command.
- When `distanceFront > 5.0` meters, local brain keeps forward drive behavior.

### 2) Brain config extension
- Add/adjust config values for local decision threshold and turn-right command so behavior stays explicit and tunable.

### 3) Scope
- Only local brain behavior is changed.
- No changes to CLI request schema, level generation, attempt lifecycle, or artifact contracts.

## Capabilities

### Modified Capabilities
- `local-brain-decision`: local brain chooses `turn_right` when front distance is 5 meters or less.

## Impact

- Code area: `Assets/Scripts/Robot/**` (brains/config/DTO usage).
- No scene/prefab/ProjectSettings/Packages changes required.
- Verification: compile without errors + manual smoke run with local brain to confirm threshold behavior at 5m boundary.
