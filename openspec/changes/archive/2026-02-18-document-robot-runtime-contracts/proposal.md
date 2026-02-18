## Why

Robot runtime behavior is currently implemented across multiple classes (`RobotBrain`, `RobotController`, `SensorManager`, `RobotBody`, brain adapters) but is not captured as a single OpenSpec contract. We need an explicit source of truth for robot tick orchestration and brain-adapter behavior so future refactors preserve runtime semantics.

## What Changes

- Add a capability that defines the robot fixed-tick control loop contract: sensor collection, controller decision, and motor application order.
- Add a capability that defines brain-adapter contracts for LocalMock and Wokwi TCP modes, including status behavior and safe fallback commands.
- Document current runtime invariants (non-blocking orchestration in Unity tick, command clamping, and deterministic fallback behavior when brain is not ready).

## Capabilities

### New Capabilities
- `robot-control-loop`: Defines runtime tick orchestration between `RobotBrain`, `SensorManager`, `RobotController`, and `RobotBody`.
- `robot-brain-adapters`: Defines expected behavior of LocalMock and Wokwi TCP brain adapters and their controller integration.

### Modified Capabilities
- None.

## Impact

- New specs to be added under:
  - `openspec/changes/document-robot-runtime-contracts/specs/robot-control-loop/spec.md`
  - `openspec/changes/document-robot-runtime-contracts/specs/robot-brain-adapters/spec.md`
- Affected implementation references (documentation target, no runtime code change in this change):
  - `Assets/Scripts/Robot/Components/RobotBrain.cs`
  - `Assets/Scripts/Robot/Sensors/SensorManager.cs`
  - `Assets/Scripts/Robot/Brains/RobotController.cs`
  - `Assets/Scripts/Robot/Components/RobotBody.cs`
  - `Assets/Scripts/Robot/Brains/LocalMockBrain.cs`
  - `Assets/Scripts/Robot/Brains/WokwiTcpBrain.cs`
  - `Assets/Scripts/Robot/Brains/BrainSelector.cs`
