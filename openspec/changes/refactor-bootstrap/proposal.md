## Why

`BootstrapRunner` currently owns too many responsibilities (request loading/validation, level preparation, runtime scene lifecycle, attempt execution, video, artifacts, teardown). This makes changes risky, error handling inconsistent, and runtime cleanup behavior hard to reason about.

## What Changes

- Decompose current Bootstrap orchestration into focused services:
  - `RunAttemptUseCase`
  - `RequestLoadingService`
  - `RequestValidationService`
  - `LevelPreparationService`
  - `ConnectionProbeService`
  - `RuntimeSceneService`
  - `AttemptRuntimeBindingService`
  - `AttemptExecutionService`
  - `AttemptVideoService`
  - `AttemptArtifactsService`
  - `AttemptResultFactory`
  - `AttemptTeardownService`
  - thin facade `BootstrapRunner`
- Enforce a clear orchestration pipeline: `load -> prepare -> setup runtime -> execute -> finalize -> teardown`.
- Move final result assembly into a dedicated factory and centralize safe teardown for both success and failure paths.
- Preserve external contracts (`LevelRunRequestDTO`, `LevelRunResultDTO`) and existing failure type semantics.

## Capabilities

### New Capabilities
- None.

### Modified Capabilities
- `headless-level-runner`: clarify runtime orchestration contract (thin `BootstrapRunner`, explicit use-case pipeline, single teardown path).

## Impact

- Primary change area: `Assets/Scripts/Bootstrap/**`.
- Existing classes are reused behind new service boundaries.
- JSON request/response contract remains unchanged.
- Main risks: incomplete runtime cleanup and behavior drift on error paths.
- Mitigation: unified `AttemptTeardownService` and centralized `AttemptResultFactory`.
