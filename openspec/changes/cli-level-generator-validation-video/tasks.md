## 1. Structure & Data Contracts

- [ ] 1.1 Move `Brains/Components/Sensors/Services` into `Assets/Scripts/Robot/**` and update namespaces/usages
- [ ] 1.2 Add DTOs for `LevelRunRequest`, `LevelRunResult`, and `FailureType` under `Assets/Scripts/**/Data/**`
- [ ] 1.3 Add artifact path helpers (project root + artifacts folder naming) in a shared utility

## 2. Level Generation

- [ ] 2.1 Implement map validation (rectangular, min size, single S/F) and symbol mapping
- [ ] 2.2 Implement `LevelGrid` coordinate mapping (row→X, col→Z, cell size 10)
- [ ] 2.3 Implement Unity adapter to instantiate prefabs via `ILevelPrefabProvider`

## 3. Attempt Control & Validation

- [ ] 3.1 Implement `AttemptController` lifecycle (start/end, timing)
- [ ] 3.2 Implement terminal conditions: finish reached, out-of-bounds, timeout, internal error
- [ ] 3.3 Implement socket connection check before attempt start

## 4. Artifacts & Result Reporting

- [ ] 4.1 Create artifacts folder `artifacts/<name>-<dateTime>-<uuid>/`
- [ ] 4.2 Copy `request.json` into artifacts folder
- [ ] 4.3 Write `result.json` with `status`, `failureType`, `reason`, `durationSeconds`, `artifacts`

## 5. Video Recording

- [ ] 5.1 Define `IAttemptVideoRecorder` port and result contract for frame capture + ffmpeg
- [ ] 5.2 Implement frame capture to `artifacts/<attempt>/frames/` (PNG per rendered frame)
- [ ] 5.3 Encode `video.mp4` with `ffmpeg` and delete `frames/` on success
- [ ] 5.4 Handle video recording failures as `status=fail` with `failureType=video_error`

## 6. Bootstrap & CLI Orchestration

- [ ] 6.1 Implement CLI JSON loading via `-request <path>`
- [ ] 6.2 Orchestrate: load request → validate → generate level → run attempt → write artifacts
- [ ] 6.3 Ensure human-wired `BootstrapRunner` entry point is documented in code comments
