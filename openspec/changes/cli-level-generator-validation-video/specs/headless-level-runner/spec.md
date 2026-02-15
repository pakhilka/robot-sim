## ADDED Requirements

### Requirement: CLI request path
The system SHALL accept a CLI argument `-request <path>` that points to a JSON file. The system SHALL read and parse the file before starting any simulation. If the argument is missing, the file does not exist, or the JSON is invalid, the system SHALL produce an attempt result with `status = "fail"` and `failureType = "invalid_input"` and SHALL NOT start the attempt.

#### Scenario: Valid request path
- **WHEN** the process is started with `-request` pointing to a readable JSON file
- **THEN** the system loads the request and continues to level generation

#### Scenario: Missing or invalid request path
- **WHEN** the process starts without `-request`, or the file path is unreadable, or the JSON cannot be parsed
- **THEN** the system writes `result.json` with `status = "fail"` and `failureType = "invalid_input"` and does not start the attempt

### Requirement: Required request fields
The system SHALL require these fields in the JSON request: `name`, `socketAddress`, `levelCompletionLimitSeconds`, `startRotationDegrees`, and `map`. If any field is missing or invalid, the system SHALL produce an attempt result with `status = "fail"` and `failureType = "invalid_input"`.

#### Scenario: Missing required field
- **WHEN** the request JSON omits any required field
- **THEN** the system writes `result.json` with `status = "fail"` and `failureType = "invalid_input"` and a friendly reason indicating which field is invalid or missing

### Requirement: Socket connection
The system SHALL attempt to connect to `socketAddress` before starting the attempt. If the connection cannot be established, the system SHALL produce an attempt result with `status = "fail"` and `failureType = "connection"`.

#### Scenario: Connection failure
- **WHEN** the system cannot connect to `socketAddress`
- **THEN** the system writes `result.json` with `status = "fail"` and `failureType = "connection"` and a friendly reason indicating the connection failure

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
