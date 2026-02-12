## Why

Integrate new functionality to support robotics lesson creators and automated evaluation:
- Provide a tool for lesson creators to generate levels/scenes.
- Enable integration into an automated testing pipeline that validates students' code.
- Record a video of each attempt and produce an attempt report with a friendly explanation and a visual recording of the run.

## What Changes

### 1) Code structure refactor (no behavior changes)

Before:
```
Assets/Scripts/
Brains/
Components/
Sensors/
Services/
Levels/
Data/
Bootstrap/
```

After:
```
Assets/Scripts/
Robot/ (Brains, Components, Sensors, Services)
Levels/
Data/
Bootstrap/
```

### 2) Internal modules/services (implementation)

- Levels: level generation service that builds a level from the input `map` schema (array of arrays) using Prefabs.
- Bootstrap: a single Unity entry point to receive the input JSON request, generate the level, run the simulation, and collect outputs (JSON report + video).
- Start/finish/walls and other symbols are defined by a map symbol mapping (e.g., `W`=Wall, `S`=Start, `F`=Finish, ` `=Empty).
- Human-created scene: an empty scene with an empty object and a script that calls the Bootstrap service (outside AI scope).

### 3) Data contracts (DTOs)

- Input JSON for running a level (example):
```json
{
  "name": "level-01",
  "socketAddress": "127.0.0.1:9999",
  "levelCompletionLimitSeconds": 120,
  "startRotationDegrees": 0,
  "map": [
    [" ", " ", "W", " ", " "],
    [" ", "W", "F", "W", " "],
    [" ", "W", " ", "W", " "],
    [" ", "W", " ", "W", " "],
    [" ", "W", "S", "W", " "]
  ]
}
```

- Output JSON for the level attempt result (final structure in specs):
- Status (pass/fail/timeout/error)
- Reason (friendly explanation)
- Duration
- Artifact links/paths (e.g., path to the video file)

### 4) Video artifact

- Record the attempt video.
- Save the video to the project root using the filename format: `<name>-<dateTime>-<uuid>`.

## Capabilities

### New Capabilities

- `headless-level-runner`: run a level in headless/CLI mode from input JSON, connect to `socketAddress`, and finish the attempt based on the time limit.
- `level-generation-from-map`: generate a level from a 2D `map` schema using Prefabs and the start parameters (`startRotationDegrees`).
- `attempt-result-reporting`: produce a JSON attempt report (pass/fail/timeout/error) plus a friendly explanation of the reason.
- `attempt-video-recording`: record the attempt video and save it using the `<name>-<dateTime>-<uuid>` naming format.

### Modified Capabilities

None.

## Impact

- Code changes are limited to `Assets/Scripts/**` only.
- New/changed areas include `Assets/Scripts/Robot/**`, `Assets/Scripts/Levels/**`, `Assets/Scripts/Bootstrap/**`, and `Assets/Scripts/Data/**`.
- External contract: input JSON (LevelRunRequest) triggers the attempt and produces output JSON (LevelRunResult) plus a video artifact.
- Risks/considerations: headless/CLI mode platform compatibility, video recording performance impact, and forbidden zones (scenes/ProjectSettings/Packages).
- Verification: build/compile with no errors; manual smoke run (Play → no Console errors → basic behavior check).
