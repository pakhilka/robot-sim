## 1. Specs & Planning

- [x] 1.1 Confirm and lock threshold rule: `distanceFront <= 5.0` => turn right
- [x] 1.2 Define/confirm right-turn motor command source in `BrainConfig`

## 2. Local Brain Implementation

- [x] 2.1 Update `LocalMockBrain` decision logic to return right-turn command when `distanceFront <= 5.0`
- [x] 2.2 Keep forward-drive behavior for `distanceFront > 5.0`
- [x] 2.3 Ensure threshold and right-turn command are config-driven in `BrainConfig`
- [x] 2.4 Add local-brain logs: start driving, received `distanceFront`, and right-turn trigger events

## 3. Validation

- [x] 3.1 Build/compile with no errors
- [x] 3.2 Manual smoke checks for boundary values: `4.9`, `5.0`, `5.1`

## 4. Controller Stabilization

- [x] 4.1 Add controller-level anti-jitter state machine in `RobotController`
- [x] 4.2 Add config fields for stabilization (`ControllerMinTurnDurationSeconds`, `ControllerAlarmClearTicksRequired`)
- [x] 4.3 Use alarm token (with local-brain command fallback) to drive controller state

## 5. RobotBody Pivot Execution

- [x] 5.1 Ensure opposite-sign motor commands run as in-place turns (zero linear velocity while turning)

## 6. Stability Validation

- [x] 6.1 Manual smoke check: narrow corridor/U-corner without oscillation into right wall
