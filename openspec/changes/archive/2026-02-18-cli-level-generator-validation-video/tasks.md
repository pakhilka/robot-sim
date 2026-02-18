> Legend:
> `[FEATURE]` — planned/implemented functionality
> `[BUG]` — bugfix added after feature implementation

## 1. [FEATURE] Structure & Data Contracts

- [x] 1.1 Move `Brains/Components/Sensors/Services` into `Assets/Scripts/Robot/**` and update namespaces/usages
- [x] 1.2 Add DTOs for `LevelRunRequest`, `LevelRunResult`, and `FailureType` under `Assets/Scripts/**/Data/**`
- [x] 1.3 Add artifact path helpers (project root + artifacts folder naming) in a shared utility
- [x] 1.4 Move Data to `Assets/Scripts/Robot/Data/**` and update usages

## 2. [FEATURE] Level Generation

- [x] 2.1 Implement map validation (rectangular, min size, single S/F) and symbol mapping
- [x] 2.2 Implement `LevelGrid` coordinate mapping (row→X, col→Z, cell size 10)
- [x] 2.3 Implement Unity adapter to instantiate prefabs via `ILevelPrefabProvider`
- [x] 2.4 Add optional `GroundWithBounds` prefab support (single spawn + size configuration from grid)

## 3. [FEATURE] Attempt Control & Validation

- [x] 3.1 Implement `AttemptController` lifecycle (start/end, timing)
- [x] 3.2 Implement terminal conditions: finish reached, out-of-bounds, timeout, internal error
- [x] 3.3 Implement socket connection check before attempt start
- [x] 3.4 Ensure out-of-bounds check uses generated level bounds (camera-independent)
- [x] 3.5 Integrate optional `GroundWithBounds` perimeter trigger as secondary out-of-bounds signal
- [x] 3.6 Add BootstrapRunner boolean flag to skip socket pre-check for local development

## 4. [FEATURE] Artifacts & Result Reporting

- [x] 4.1 Create artifacts folder `artifacts/<name>-<dateTime>-<uuid>/`
- [x] 4.2 Copy `request.json` into artifacts folder
- [x] 4.3 Write `result.json` with `status`, `failureType`, `reason`, `durationSeconds`, `artifacts`

## 5. [FEATURE] Video Recording

- [x] 5.1 Define `IAttemptVideoRecorder` port and result contract for frame capture + ffmpeg
- [x] 5.2 Implement frame capture to `artifacts/<attempt>/frames/` (PNG per rendered frame)
- [x] 5.3 Encode `video.mp4` with `ffmpeg` and delete `frames/` on success
- [x] 5.4 Handle video recording failures as `status=fail` with `failureType=video_error`

## 6. [FEATURE] Bootstrap & CLI Orchestration

- [x] 6.1 Implement CLI JSON loading via `-request <path>`
- [x] 6.2 Orchestrate: load request → validate → generate level → run attempt → write artifacts
- [x] 6.3 Ensure human-wired `BootstrapRunner` entry point is documented in code comments
- [x] 6.4 Create per-request runtime attempt scene and unload it after attempt completion

## 7. [FEATURE] Editor Local Validation Input

- [x] 7.1 Add `BootstrapRunner` Inspector fields for local request input (`editorRequestPath`, `useEditorRequestFallback`)
- [x] 7.2 Implement request source precedence: CLI `-request` → `editorRequestPath`
- [x] 7.3 Add optional debug output folder in Inspector and write `last-request.json` + `last-result.json` for editor fallback runs
- [x] 7.4 Add friendly validation errors for invalid editor fallback path/JSON and keep `failureType=invalid_input`

## 8. [BUG] Runtime Scene Physics Bugfix

- [x] [BUG] 8.1 Fix runtime attempt scene physics so robot movement is simulated during attempt runs (temporary workaround: default physics simulation)

## 9. [BUG] ffmpeg Discovery Bugfix

- [x] [BUG] 9.1 Fix `ffmpeg` executable resolution for dev runs (env override + project-local tools folder + PATH/common locations)

## 10. [BUG] ffmpeg Encoding Compatibility Bugfix

- [x] [BUG] 10.1 Fix odd frame dimension encoding failure for `libx264`/`yuv420p` by normalizing output to even width/height

## 11. [BUG] Centered World Coordinates Bugfix

- [x] [BUG] 11.1 Center generated map around world origin so `(0,0,0)` is map center
- [x] [BUG] 11.2 Update bounds/finish checks and optional GroundWithBounds placement for centered coordinates
