## ADDED Requirements

### Requirement: Map schema and symbol mapping
The system SHALL accept `map` as a rectangular 2D array of strings. The symbol mapping SHALL be:
- `W` = Wall
- `S` = Start
- `F` = Finish
- Any other value = Empty

#### Scenario: Unknown symbol
- **WHEN** a cell contains a symbol other than `W`, `S`, or `F`
- **THEN** the system treats the cell as Empty

### Requirement: Map validation for start and finish
The system SHALL require exactly one Start (`S`) and exactly one Finish (`F`) in the map. If the map has zero or multiple `S` or `F` cells, the system SHALL report an error and SHALL NOT start the attempt.

#### Scenario: Missing or multiple start/finish
- **WHEN** the map contains zero or multiple `S` or `F` cells
- **THEN** the system produces an attempt result with `status = "fail"` and `failureType = "invalid_input"` and a friendly reason

### Requirement: Rectangular map validation
The system SHALL require all map rows to have the same length and the map to be at least 1x1. If the map is non-rectangular or empty, the system SHALL report an error and SHALL NOT start the attempt.

#### Scenario: Non-rectangular map
- **WHEN** the map contains rows with different lengths
- **THEN** the system produces an attempt result with `status = "fail"` and `failureType = "invalid_input"` and a friendly reason

### Requirement: Grid placement and coordinates
The system SHALL build a grid with width equal to the number of columns and height equal to the number of rows. The system SHALL map column indices to +Z and row indices to +X with a cell size of 10.0 world units, placing cell centers at `(rowIndex * 10 + 5, 0, colIndex * 10 + 5)`. The system SHALL instantiate Prefabs for `W`, `S`, and `F` cells at their respective positions.

#### Scenario: Grid coordinates
- **WHEN** the map has width 3 and height 2
- **THEN** the grid spans X in `[0, 20]` and Z in `[0, 30]` with cell centers at `(rowIndex * 10 + 5, 0, colIndex * 10 + 5)`

### Requirement: Level bounds contract for attempt validation
The system SHALL provide level world bounds derived from generated grid dimensions and cell size for attempt validation:
- X bounds: `[0, rowCount * 10)`
- Z bounds: `[0, columnCount * 10)`
These bounds SHALL be the source for out-of-bounds checks in attempt control.

#### Scenario: Bounds derived from map
- **WHEN** the map has 5 rows and 4 columns
- **THEN** level bounds are X in `[0, 50)` and Z in `[0, 40)` for out-of-bounds checks

### Requirement: GroundWithBounds prefab generation
The system SHALL support an optional single `GroundWithBounds` prefab provided by `ILevelPrefabProvider`. If provided, the system SHALL instantiate it once per generated level and configure it to the map-derived world size.

#### Scenario: GroundWithBounds provided
- **WHEN** provider returns a valid `GroundWithBounds` prefab
- **THEN** the system spawns exactly one instance and configures floor/trigger perimeter to the level world size

### Requirement: Start rotation
The system SHALL spawn the robot at the Start cell center and apply `startRotationDegrees` as a yaw rotation around the Y axis.

#### Scenario: Start rotation applied
- **WHEN** `startRotationDegrees` is 90
- **THEN** the robot spawns at the Start cell center with a Y rotation of 90 degrees
