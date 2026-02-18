## Context

RobotSim follows Clean Architecture / Ports & Adapters, and code changes are constrained to `Assets/Scripts/**`. Scenes, `ProjectSettings/**`, and `Packages/**` are normally forbidden, but package additions are permitted for this change (with explicit approval). The feature must run a level headlessly from CLI by reading a JSON file path, generate a level from a 2D map, validate attempt outcomes, and produce artifacts (JSON + video). CI runs on Linux self-hosted runners with no physical display; the Unity Player must run under Xvfb for rendering and frame capture.

## Goals / Non-Goals

**Goals:**
- Accept a LevelRunRequest JSON via a CLI argument (path to file).
- Generate a level from a 2D `map` (`S`, `F`, `W`, `Empty`) using Prefabs.
- Validate an attempt: time limit, out-of-bounds, presence of start and finish, and completion success/failure.
- Produce an artifacts folder: `artifacts/<name>-<dateTime>-<uuid>/` with:
  - `request.json` (copy of input)
  - `result.json`
  - `video.mp4`
- Record a video of the attempt and save it to the artifacts folder using runtime frame capture + ffmpeg encoding.
- Refactor structure: move `Brains/Components/Sensors/Services` under `Assets/Scripts/Robot/**`.

**Non-Goals:**
- Scene edits by the agent (human will wire the Bootstrap entry point).
- New editor tooling or UI.
- Support for map symbols beyond `S`, `F`, `W`, and empty.

## Decisions

- **Entry point**: `BootstrapRunner` (MonoBehaviour) in `Assets/Scripts/Bootstrap` reads the CLI argument path, loads JSON, and runs the attempt. Rationale: works in batchmode and in standard Unity builds. Alternative: separate external CLI binary.
- **JSON parsing**: use `Newtonsoft.Json` (installed) because `string[][]` map requires robust parsing. Alternative: change contract to array-of-strings (not preferred).
- **Level generation**: core builds a `LevelGrid` and spawn instructions; Unity adapter instantiates Prefabs via an `ILevelPrefabProvider` MonoBehaviour set in the scene by a human. Grid mapping: row index → +X, column index → +Z, cell size 10.0, cell center `(rowIndex * 10 + 5, 0, colIndex * 10 + 5)`. Alternative: `Resources.Load` if a Resources layout exists.
- **Validation loop**: `AttemptController` ticks from Unity `Update/FixedUpdate`, tracking elapsed time, bounds from `LevelGrid`, and completion. On any terminal condition it produces a result and stops the attempt.
- **Artifacts**: `AttemptArtifactsWriter` writes to `artifacts/<name>-<dateTime>-<uuid>/` in the project root (`Application.dataPath/..`). It copies `request.json`, writes `result.json` (with `status=pass|fail` and `failureType` when failed), and receives the video output path.
- **Video capture**: runtime frame capture to `artifacts/<attempt>/frames/` (PNG, one per rendered frame for maximum smoothness), then `ffmpeg` encodes `video.mp4` into the artifacts folder. `ffmpeg` must be available on PATH; missing/failed encoding results in `status=fail` with `failureType=video_error`. On successful encode, the `frames/` folder is deleted. Alternative: a Unity recording package (not used due to CI constraints).
- **Refactor**: move existing `Brains/Components/Sensors/Services` into `Assets/Scripts/Robot/**` and update namespaces/usages.

## Risks / Trade-offs

- **Video performance impact** → Mitigation: configurable resolution and optional disable flag.
- **Frame capture disk/CPU cost** → Mitigation: PNG compression, cleanup of `frames/` after encode, and CI guidance to ensure sufficient disk.
- **ffmpeg dependency** → Mitigation: explicit failure with `failureType=video_error` if not found on PATH.
- **Missing or incorrect Prefab wiring** → Mitigation: explicit validation errors and `status=error` in `result.json`.
- **Package selection/installation** → Mitigation: choose a recording package before implementation; keep video recorder behind a port so it can be swapped.
- **Headless/CLI platform differences** → Mitigation: keep Unity API calls on main thread and provide clear failure reasons in `result.json`.

## Migration Plan

- Move folders into `Assets/Scripts/Robot/**`, update namespaces/usages.
- Add DTOs under `Assets/Scripts/Data/**`.
- Implement Levels generation and validation services.
- Implement Bootstrap orchestration and result writer.
- Implement video recorder adapter for frame capture + ffmpeg, and ensure CI uses Xvfb.
- Human wires `BootstrapRunner` and Prefab provider in the scene.
- Rollback: revert commits and restore original folder layout.

## Open Questions

- What capture resolution should be used for frames?
- Exact prefab mapping and scene wiring steps (to be defined by the human).
