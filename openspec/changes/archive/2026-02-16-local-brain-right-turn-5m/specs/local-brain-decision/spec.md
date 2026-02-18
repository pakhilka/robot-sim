## ADDED Requirements

### Requirement: Local brain right-turn threshold
The system SHALL make local-brain obstacle decisions using front distance in meters from the local sensor payload. If `distanceFront <= 5.0`, the local brain SHALL output a right-turn motor command.

#### Scenario: Distance equal to threshold
- **WHEN** `distanceFront` is exactly `5.0`
- **THEN** local brain returns a right-turn command

#### Scenario: Distance below threshold
- **WHEN** `distanceFront` is below `5.0`
- **THEN** local brain returns a right-turn command

### Requirement: Local brain forward behavior above threshold
If `distanceFront > 5.0`, local brain SHALL return the configured forward-drive command.

#### Scenario: Distance above threshold
- **WHEN** `distanceFront` is greater than `5.0`
- **THEN** local brain returns forward-drive command

### Requirement: Right-turn command is configuration-driven
The right-turn command values SHALL come from brain configuration, so threshold response can be tuned without changing decision flow.

#### Scenario: Custom right-turn command
- **WHEN** configuration defines non-default right-turn motor values
- **THEN** local brain uses those configured values for `distanceFront <= 5.0`

### Requirement: Controller-level anti-jitter for obstacle response
The system SHALL stabilize alarm-driven turn behavior in `RobotController`. After alarm is detected, the controller SHALL keep turn-right active for at least a configured minimum duration and SHALL switch back to forward only after alarm is clear for a configured number of consecutive ticks.

#### Scenario: Alarm toggles around threshold
- **WHEN** sensor/brain alarm rapidly toggles near threshold
- **THEN** controller keeps turn-right active until minimum turn time and clear-tick criteria are satisfied

#### Scenario: Alarm clears and stays clear
- **WHEN** alarm is no longer present for required consecutive ticks after minimum turn time
- **THEN** controller returns to forward command

### Requirement: Opposite-motor commands execute as pivot turns
`RobotBody` SHALL execute opposite-sign motor commands as in-place turns by zeroing linear velocity while applying yaw rotation.

#### Scenario: Right turn near wall
- **WHEN** left/right motor commands have opposite signs during obstacle recovery
- **THEN** robot turns in place without forward drift into the wall
