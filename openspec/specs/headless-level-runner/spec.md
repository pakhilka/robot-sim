# headless-level-runner Specification

## Purpose
TBD - created by archiving change cli-level-generator-validation-video. Update Purpose after archive.
## Requirements
### Requirement: CLI request path
The system SHALL accept a CLI argument `-request <path>` that points to a JSON file. The system SHALL read and parse the file before starting any simulation. If the argument is missing, the file does not exist, or the JSON is invalid, the system SHALL produce an attempt result with `status = "fail"` and `failureType = "invalid_input"` and SHALL NOT start the attempt.

#### Scenario: Valid request path
- **WHEN** the process is started with `-request` pointing to a readable JSON file
- **THEN** the system loads the request and continues to level generation

#### Scenario: Missing or invalid request path
- **WHEN** the process starts without `-request`, or the file path is unreadable, or the JSON cannot be parsed
- **THEN** the system writes `result.json` with `status = "fail"` and `failureType = "invalid_input"` and does not start the attempt

### Requirement: Editor request fallback for local validation
The system SHALL support optional request input fields on `BootstrapRunner` for local validation/testing in Unity Editor. This fallback SHALL be used only when CLI argument `-request <path>` is not provided.

Fallback sources and precedence:
- CLI request path (`-request`) has highest priority.
- If CLI request path is absent, `editorRequestPath` file input is used when non-empty.
- If editor fallback is enabled but `editorRequestPath` is empty or invalid, the system SHALL return `status = "fail"` and `failureType = "invalid_input"`.

#### Scenario: CLI request takes precedence
- **WHEN** both `-request` and editor fallback fields are set
- **THEN** the system uses only the CLI request file

#### Scenario: Editor fallback path is used
- **WHEN** no CLI request is passed and `editorRequestPath` points to a valid JSON file
- **THEN** the system loads request from `editorRequestPath` and continues to level generation

#### Scenario: Invalid editor fallback input
- **WHEN** no CLI request is passed and `editorRequestPath` input is empty, unreadable, or invalid JSON
- **THEN** the system writes `result.json` with `status = "fail"` and `failureType = "invalid_input"` and does not start the attempt

### Requirement: Optional editor debug JSON output
When running through editor fallback input, the system SHALL support an optional debug output folder configured in `BootstrapRunner` Inspector. If configured, the resolved request and resulting output SHALL be written for local validation.

#### Scenario: Editor debug output enabled
- **WHEN** editor fallback run completes and debug output folder is configured
- **THEN** the system writes `last-request.json` and `last-result.json` into the configured folder

### Requirement: Required request fields
The system SHALL require these fields in the JSON request: `name`, `socketAddress`, `levelCompletionLimitSeconds`, `startRotationDegrees`, and `map`. If any field is missing or invalid, the system SHALL produce an attempt result with `status = "fail"` and `failureType = "invalid_input"`.

#### Scenario: Missing required field
- **WHEN** the request JSON omits any required field
- **THEN** the system writes `result.json` with `status = "fail"` and `failureType = "invalid_input"` and a friendly reason indicating which field is invalid or missing

### Requirement: Configurable socket connection pre-check
The system SHALL attempt to connect to `socketAddress` before starting the attempt by default. `BootstrapRunner` SHALL expose a boolean flag to skip socket pre-check for local development flows (for example, when using LocalMock brain). When this flag is enabled, the system SHALL continue attempt startup without connection requirement.

#### Scenario: Connection failure
- **WHEN** socket pre-check is enabled and the system cannot connect to `socketAddress`
- **THEN** the system writes `result.json` with `status = "fail"` and `failureType = "connection"` and a friendly reason indicating the connection failure

#### Scenario: Socket pre-check skipped by bootstrap flag
- **WHEN** `BootstrapRunner` has socket pre-check skip flag enabled
- **THEN** the system skips socket pre-check and continues attempt startup without connection requirement

### Requirement: Attempt lifecycle and termination
The system SHALL start the attempt after level generation and robot spawn are complete. The system SHALL terminate the attempt on the first terminal condition and set the attempt status as follows:
- Finish reached → `status = "pass"`
- Robot leaves level bounds → `status = "fail"`, `failureType = "out_of_bounds"`
- Time limit exceeded → `status = "fail"`, `failureType = "timeout"`
- Internal error during the attempt → `status = "fail"`, `failureType = "error"`

#### Scenario: Finish reached
- **WHEN** the robot reaches the finish area during the attempt
- **THEN** the system ends the attempt with `status = "pass"`

#### Scenario: Out-of-bounds
- **WHEN** the robot leaves the defined level bounds
- **THEN** the system ends the attempt with `status = "fail"` and `failureType = "out_of_bounds"`

#### Scenario: Time limit exceeded
- **WHEN** elapsed attempt time exceeds `levelCompletionLimitSeconds`
- **THEN** the system ends the attempt with `status = "fail"` and `failureType = "timeout"`

### Requirement: Out-of-bounds uses level bounds, not camera
The system SHALL evaluate out-of-bounds using generated level world bounds derived from the map grid and cell size. Camera viewport/frustum position SHALL NOT affect out-of-bounds detection.

#### Scenario: Camera-independent out-of-bounds
- **WHEN** the camera is moved or the run is executed in headless mode
- **THEN** out-of-bounds detection is still based only on generated level bounds

### Requirement: GroundWithBounds trigger integration
If a `GroundWithBounds` prefab is configured and instantiated for the attempt, perimeter trigger hits MAY be used as an additional out-of-bounds signal. The final decision SHALL still remain consistent with level bounds.

#### Scenario: Trigger-assisted out-of-bounds
- **WHEN** the robot enters a perimeter trigger from `GroundWithBounds`
- **THEN** the attempt is finalized as out-of-bounds without relying on camera visibility

### Requirement: Runtime attempt scene isolation
For each incoming run request, the system SHALL create a dedicated runtime attempt scene, instantiate generated level objects and robot in that scene, and unload that scene after attempt completion and artifact write.

#### Scenario: Sequential requests in one process
- **WHEN** two run requests are processed sequentially by the same process
- **THEN** each request runs in its own runtime attempt scene, and the previous attempt scene is unloaded before the next attempt starts

### Requirement: Runtime attempt scene physics simulation
The system SHALL ensure active physics simulation for robot and level objects spawned into runtime attempt scenes, so movement and collisions are processed during the attempt.

#### Scenario: Robot spawned in runtime attempt scene
- **WHEN** the robot is spawned in a runtime attempt scene
- **THEN** its Rigidbody movement is simulated and applied without requiring manual hierarchy moves to the Bootstrap scene

