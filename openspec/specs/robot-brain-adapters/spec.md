# robot-brain-adapters Specification

## Purpose
TBD - created by archiving change document-robot-runtime-contracts. Update Purpose after archive.
## Requirements
### Requirement: Brain selection by configured type
The system SHALL create the active brain implementation from configured `BrainType` via `BrainSelector`.

#### Scenario: Local brain selection
- **WHEN** robot is configured with `BrainType.LocalMock`
- **THEN** `BrainSelector` returns `LocalMockBrain`

#### Scenario: Wokwi brain selection
- **WHEN** robot is configured with `BrainType.WokwiTcp`
- **THEN** `BrainSelector` returns `WokwiTcpBrain` with a TCP client service configured for host and port

### Requirement: Local brain ready-status command output
`LocalMockBrain` SHALL return `BrainStatusDTO.Ready` on each tick and provide command output based on current sensor input and configuration.

#### Scenario: Local brain emits ready step result
- **WHEN** `LocalMockBrain.Tick` is called with valid sensor payload
- **THEN** returned `BrainStepResultDTO.status` is `Ready` and command values come from configured drive/turn commands

### Requirement: Wokwi brain safe behavior before ready
`WokwiTcpBrain` SHALL attempt connect/reconnect when disconnected or in error state. Until ready-handshake is confirmed, returned motor command SHALL remain safe-stop.

#### Scenario: Adapter not ready
- **WHEN** Wokwi adapter is in `Disconnected`, `Connecting`, or `Error` state
- **THEN** returned motor command is stop `(0, 0)`

#### Scenario: Ready handshake received
- **WHEN** adapter receives configured ready token from TCP backend
- **THEN** adapter transitions to `BrainStatusDTO.Ready`

### Requirement: Controller behavior for non-ready brain results
`RobotController` SHALL bypass alarm-state machine behavior when brain status is not `Ready` and SHALL return brain command directly.

#### Scenario: Brain not ready in controller tick
- **WHEN** controller receives `BrainStepResultDTO` with status other than `Ready`
- **THEN** controller resets turn-state internals and returns the brain-provided command

### Requirement: Controller alarm-driven command stabilization
When brain status is `Ready`, `RobotController` SHALL use alarm-driven stabilization: enter turn-right on alarm, enforce minimum turn duration, and return to forward command only after configured clear-alarm tick count.

#### Scenario: Alarm toggles near threshold
- **WHEN** alarm signal appears intermittently across adjacent ticks
- **THEN** controller preserves turn-right until both minimum turn time and clear-tick thresholds are satisfied

