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
