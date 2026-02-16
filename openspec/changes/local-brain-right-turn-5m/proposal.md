## Why

Local-brain behavior currently stops in front of obstacles, which is not aligned with the desired classroom scenario.
We need deterministic obstacle-avoidance behavior for local validation runs: when an obstacle is close, the robot should turn right instead of stopping.
After enabling right-turn, we observed oscillation in narrow corridors (`turn -> forward -> turn` every few ticks), which causes the robot to drift into the right wall.
We need controller-level stabilization and reliable in-place turning.

## What Changes

### 1) Local brain decision rule update
- Update local brain logic to use a right-turn rule based on front distance.
- New rule: when `distanceFront <= 5.0` meters, local brain returns a right-turn motor command.
- When `distanceFront > 5.0` meters, local brain keeps forward drive behavior.

### 2) Brain config extension
- Add/adjust config values for local decision threshold and turn-right command so behavior stays explicit and tunable.

### 3) Controller-level anti-jitter state machine
- Move obstacle-response stabilization to `RobotController` so behavior is shared for local and TCP brains.
- Enter turn-right state when alarm is active.
- Exit turn-right only after:
  - minimum turn duration has passed;
  - alarm is clear for N consecutive ticks.

### 4) RobotBody pivot execution
- When motors have opposite signs (turn-in-place command), `RobotBody` SHALL zero linear velocity and apply yaw rotation only.
- This prevents forward drift into walls during right-turn recovery.

### 5) Scope
- Local-brain threshold rule remains unchanged (`distanceFront <= 5.0`).
- No changes to CLI request schema, level generation, attempt lifecycle, or artifact contracts.

## Capabilities

### Modified Capabilities
- `local-brain-decision`: local brain chooses `turn_right` when front distance is 5 meters or less.
- `obstacle-response-controller`: controller stabilizes alarm-driven turn/forward transitions.
- `robotbody-pivot`: body executes opposite-motor commands as in-place turns without linear drift.

## Impact

- Code area: `Assets/Scripts/Robot/**` (brains/config/controller/body).
- No scene/prefab/ProjectSettings/Packages changes required.
- Verification: compile without errors + manual smoke run for boundary values and narrow-corridor turn stability.
