# 5.1 Manual Smoke Map Set

Prepared smoke requests for editor validation under `Builds/requests/`:

- `request-pass-smoke-corridor.json`
- `request-pass-smoke-dead-end.json`
- `request-pass-smoke-t-junction.json`
- `request-pass-smoke-x-room.json` (corridor + `2x2` room made from `X`)

Validation intent for each map:

- corridor: thin walls render on both sides of a straight road
- dead end: terminal wall closes the dead-end side
- T-junction: branch walls exist with one open split side
- X-room: perimeter around a connected `2x2` room is enclosed by thin walls
