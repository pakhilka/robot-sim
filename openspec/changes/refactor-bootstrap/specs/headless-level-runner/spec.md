## ADDED Requirements

### Requirement: Thin BootstrapRunner facade
The system SHALL keep `BootstrapRunner` as a thin `MonoBehaviour` facade that delegates run orchestration to `RunAttemptUseCase` and delegates frame/tick processing to dedicated services.

#### Scenario: Bootstrap lifecycle delegates orchestration
- **WHEN** Unity invokes `BootstrapRunner` lifecycle (`Awake`, `Start`, `Update`)
- **THEN** orchestration and runtime execution behavior is handled through dedicated services rather than embedded end-to-end logic in the facade

### Requirement: Deterministic run-attempt orchestration pipeline
The system SHALL orchestrate a run attempt with a deterministic pipeline: request load, request validation, level preparation, connection probe, runtime setup, execution, result finalization, and teardown.

#### Scenario: Successful run follows pipeline order
- **WHEN** a valid request is processed and runtime completes with success
- **THEN** the system executes the orchestration stages in the defined order and produces a final result

#### Scenario: Startup failure still terminates through orchestration
- **WHEN** any startup stage fails before attempt completion
- **THEN** the orchestration finalizes failure state and completes through teardown without leaving active runtime state

### Requirement: Runtime binding before execution
The system SHALL establish runtime bindings before execution starts, including robot transform access, terminal condition evaluation wiring, and perimeter trigger subscriptions.

#### Scenario: Binding succeeds before attempt execution
- **WHEN** runtime scene setup succeeds and required components are available
- **THEN** runtime bindings are created before attempt execution begins

#### Scenario: Binding failure aborts execution
- **WHEN** required runtime binding dependencies are missing
- **THEN** the system finalizes the run as failure and does not start execution ticks

### Requirement: Unified teardown and failure mapping
The system SHALL use a single teardown path to detach runtime events, stop video capture when needed, unload runtime scene state, and clear in-memory runtime references. Video lifecycle failures SHALL be mapped to `failureType = "video_error"`.

#### Scenario: Teardown after terminal completion
- **WHEN** an attempt reaches pass or fail terminal state
- **THEN** teardown executes once and runtime state is cleaned safely

#### Scenario: Video stop failure mapping
- **WHEN** video stop/encode fails during finalization
- **THEN** the final result uses `status = "fail"` and `failureType = "video_error"`
