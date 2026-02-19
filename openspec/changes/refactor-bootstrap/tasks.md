## 1. Orchestration Contracts and Runtime Data

- [x] 1.1 Add shared runtime data contracts under `Assets/Scripts/**/Data/**` for request source, run-attempt state, binding handles, and execution snapshots
- [x] 1.2 Define service-facing contracts for run orchestration handoffs so each stage consumes/returns explicit data
- [x] 1.3 Keep contracts compatible with existing `LevelRunRequestDTO`, `LevelRunResultDTO`, and `FailureType`

## 2. Request and Preparation Services

- [x] 2.1 Implement `RequestLoadingService` using current CLI/editor fallback behavior and return request + source + resolved path
- [x] 2.2 Implement `RequestValidationService` for business validation (`name`, `map`, limits, and `socketAddress` format)
- [x] 2.3 Implement `LevelPreparationService` that builds `LevelGrid` from request and returns preparation failure reasons
- [x] 2.4 Implement `ConnectionProbeService` wrapper for pre-start socket connectivity checks

## 3. Runtime Scene and Binding Services

- [x] 3.1 Implement `RuntimeSceneService` for create/spawn/unload runtime scene operations
- [x] 3.2 Implement `AttemptRuntimeBindingService` to bind robot transform, terminal evaluator, and perimeter trigger subscriptions
- [x] 3.3 Implement `AttemptTeardownService` for idempotent detach/stop/unload/clear cleanup

## 4. Execution, Video, and Artifacts Services

- [x] 4.1 Implement `AttemptExecutionService` for attempt start/tick/evaluate/terminal completion lifecycle
- [x] 4.2 Implement `AttemptVideoService` for start/capture/stop and map failures to `FailureType.VideoError`
- [x] 4.3 Refine `AttemptArtifactsService` responsibilities to artifact layout, request copy, result write, and editor debug JSON
- [x] 4.4 Implement `AttemptResultFactory` to build final `LevelRunResultDTO` from attempt state and artifact paths

## 5. Use Case and Bootstrap Facade Refactor

- [x] 5.1 Implement `RunAttemptUseCase` that orchestrates `load -> prepare -> setup runtime -> execute -> finalize -> teardown`
- [ ] 5.2 Refactor `BootstrapRunner` into a thin `MonoBehaviour` facade delegating `Awake/Start/Update` to use case/services
- [ ] 5.3 Ensure runtime behavior parity for success/failure paths and preserve current JSON contract outputs

## 6. Verification and Task Tracking

- [ ] 6.1 Mark completed tasks in this file as implementation progresses
- [ ] 6.2 Run compile/smoke verification workflow and confirm no new Unity Console errors
- [ ] 6.3 Prepare handoff summary with pending work and next `bd ready` item
