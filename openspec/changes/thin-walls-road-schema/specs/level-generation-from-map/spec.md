## MODIFIED Requirements

### Requirement: Map schema and symbol mapping
The system SHALL accept `map` as a rectangular 2D array of strings. The symbol mapping SHALL be:
- `#` = Void (non-passable, no road tile)
- `S` = Start road tile
- `F` = Finish road tile
- `─`, `│`, `┌`, `┐`, `└`, `┘`, `├`, `┤`, `┬`, `┴`, `X` = Road tiles
Any symbol outside this set SHALL be rejected as invalid input.

#### Scenario: Unknown symbol
- **WHEN** a cell contains a symbol outside the allowed map schema
- **THEN** the system produces an attempt result with `status = "fail"` and `failureType = "invalid_input"` and a friendly reason

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

### Requirement: Grid placement and centered coordinates
The system SHALL build a grid with width equal to the number of columns and height equal to the number of rows. The system SHALL map column indices to +Z and row indices to +X with a cell size of 10.0 world units. The generated map SHALL be centered around world origin `(0, 0, 0)` (map center at origin). Cell centers SHALL be placed using centered coordinates:
- `x = -((rowCount * 10) / 2) + (rowIndex * 10 + 5)`
- `z = -((columnCount * 10) / 2) + (colIndex * 10 + 5)`
The system SHALL instantiate road-compatible prefabs for all non-void cells and derive thin boundary walls from tile-edge connectivity.

#### Scenario: Centered grid with road schema
- **WHEN** the map has width 3 and height 2
- **THEN** the grid spans X in `[-10, 10]` and Z in `[-15, 15]` with centered cell centers and road cells spawned from the road schema

## ADDED Requirements

### Requirement: Dead-end handling against void
The system SHALL treat an open road edge facing void (`#`) as a valid dead end and SHALL close that edge with a thin boundary wall.

#### Scenario: Dead-end in corridor
- **WHEN** a road tile has no compatible neighbor on one of its open edges
- **THEN** the edge is rendered as a dead end with a thin boundary wall
