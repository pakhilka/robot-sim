# robot-control-loop Specification

## Purpose
TBD - created by archiving change document-robot-runtime-contracts. Update Purpose after archive.
## Requirements
### Requirement: Fixed-tick orchestration order
The system SHALL execute robot runtime orchestration in fixed-tick order: collect sensor payload, run controller/brain decision, then apply motor command to robot body.

#### Scenario: Regular fixed tick
- **WHEN** `RobotBrain.FixedUpdate` executes with valid body and controller references
- **THEN** sensor data is collected before controller decision, and motor command is applied after decision in the same tick

### Requirement: Null-safe tick guard
The system SHALL skip robot runtime tick processing when required runtime references are missing.

#### Scenario: Missing robot body
- **WHEN** `RobotBrain` has no assigned `RobotBody`
- **THEN** the tick exits without collecting sensor data or applying motor commands

### Requirement: Sensor payload composition
The system SHALL provide controller input as `SensorDataDTO` containing front distance, current speed, tick index, fixed delta time, and raw per-sensor payload map.

#### Scenario: Laser sensor is registered
- **WHEN** `SensorManager.Collect` runs with `LaserDistanceSensor` registered
- **THEN** `SensorDataDTO.distanceFront` is populated from sensor value and `allSensorsData` includes the laser sensor entry

#### Scenario: Body reference is missing in collect call
- **WHEN** `SensorManager.Collect` is called with `body = null`
- **THEN** `SensorDataDTO.currentSpeed` is set to `0`

### Requirement: Motor command normalization and differential motion
The system SHALL normalize motor inputs to `[-1, 1]`, scale by configured max speed, and apply differential-drive translation/rotation in physics tick.

#### Scenario: Normalized motor command application
- **WHEN** controller returns motor values outside `[-1, 1]`
- **THEN** `RobotBody` clamps values before applying Rigidbody movement

#### Scenario: Opposite-sign motors trigger pivot behavior
- **WHEN** left and right motor speeds have opposite signs
- **THEN** `RobotBody` sets linear velocity to zero and applies yaw rotation without forward drift

