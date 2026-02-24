## ADDED Requirements

### Requirement: Road glyph directional openings
The system SHALL interpret each road glyph as a set of open directions (N/E/S/W):
- `─` = E/W
- `│` = N/S
- `┌` = E/S
- `┐` = W/S
- `└` = N/E
- `┘` = N/W
- `├` = N/E/S
- `┤` = N/W/S
- `┬` = E/S/W
- `┴` = N/E/W
- `X` = N/E/S/W
- `S` and `F` = inferred from valid neighboring road connectivity

#### Scenario: Turn glyph connectivity
- **WHEN** a tile is `┌`
- **THEN** movement is allowed only to the east and south neighboring tiles

### Requirement: Mutual edge agreement for passable neighbors
The system SHALL allow passable adjacency only when two neighboring road tiles expose compatible opposite edges. A one-sided opening SHALL be treated as a blocked edge for movement.

#### Scenario: Mismatched neighbor edges
- **WHEN** tile A opens east but tile B (east neighbor) does not open west
- **THEN** adjacency between A and B is blocked for navigation

### Requirement: Thin boundary wall derivation
The system SHALL derive thin boundary walls from road edges:
- No wall on an edge only when two neighboring road tiles are mutually open toward each other.
- A wall on all other edges, including road-to-void and map-perimeter edges.

#### Scenario: Road next to void
- **WHEN** a road tile edge faces `#`
- **THEN** a thin boundary wall is generated on that edge

### Requirement: Room construction with X tiles
The system SHALL support room-like open areas by allowing contiguous `X` tiles to connect internally without interior walls while still generating thin walls on the outer boundary.

#### Scenario: 2x2 X room
- **WHEN** four `X` tiles form a 2x2 block surrounded by void
- **THEN** internal shared edges have no walls and only the room perimeter has thin walls
