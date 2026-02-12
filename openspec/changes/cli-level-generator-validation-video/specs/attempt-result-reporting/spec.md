## ADDED Requirements

### Requirement: Artifacts folder creation
The system SHALL create an artifacts folder in the project root with the format:
`artifacts/<name>-<dateTime>-<uuid>/`.
`<dateTime>` SHALL use UTC in `yyyyMMdd-HHmmss` format, and `<uuid>` SHALL be an RFC 4122 UUID.

#### Scenario: Artifacts folder naming
- **WHEN** a request with `name = "level-01"` starts
- **THEN** the system creates `artifacts/level-01-<dateTime>-<uuid>/` in the project root

### Requirement: Request copy
The system SHALL copy the original input JSON into the artifacts folder as `request.json` without modification.

#### Scenario: Request copy
- **WHEN** the attempt starts from a valid request
- **THEN** `artifacts/<name>-<dateTime>-<uuid>/request.json` exists and matches the input JSON

### Requirement: Result JSON content
The system SHALL write `result.json` in the artifacts folder with these fields:
- `name` (string)
- `status` (string: `pass`, `fail`)
- `failureType` (string; required when `status = "fail"`, omitted or null when `status = "pass"`)
- `reason` (string, friendly explanation)
- `durationSeconds` (number, elapsed time between attempt start and end)
- `artifacts` (object with `request`, `result`, `video` paths relative to project root)

#### Scenario: Result JSON after completion
- **WHEN** the attempt ends for any reason
- **THEN** `result.json` is written with the required fields and artifact paths

### Requirement: Failure types
When `status = "fail"`, the system SHALL set `failureType` to one of:
`invalid_input`, `connection`, `timeout`, `out_of_bounds`, `error`, `video_error`.

#### Scenario: Timeout failure type
- **WHEN** the attempt ends due to time limit exceeded
- **THEN** `failureType = "timeout"`

### Requirement: Friendly reason
The `reason` field SHALL be human-readable and explain why the attempt passed or failed. For `failureType = "error"` or `failureType = "video_error"`, it SHALL include a concise error message.

#### Scenario: Timeout reason
- **WHEN** the attempt ends due to time limit exceeded
- **THEN** `reason` mentions the time limit and that the level was not completed in time

### Requirement: Result on startup failure
If the attempt cannot start (invalid input, map validation error, or connection failure), the system SHALL still create the artifacts folder and write `request.json` and `result.json` with `status = "fail"` and an appropriate `failureType`.

#### Scenario: Map validation error
- **WHEN** the map is invalid and the attempt cannot start
- **THEN** the system writes `result.json` with `status = "fail"` and `failureType = "invalid_input"` and a friendly reason
