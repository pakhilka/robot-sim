## Context

`Assets/Scripts/Bootstrap/Components/BootstrapRunner.cs` currently implements an end-to-end orchestration path in one MonoBehaviour and exceeds maintainable scope (request input, validation, level preparation, socket check, runtime scene lifecycle, event subscriptions, attempt lifecycle, video recording, artifact IO, result assembly, teardown).

The project follows Clean Architecture / Ports & Adapters in Unity:
- Unity adapters own Unity API calls and MonoBehaviour lifecycle.
- Core services and use-cases should remain cohesive and explicit.
- Runtime tick must stay non-blocking and must not perform IO in the critical loop.

This change refactors orchestration while preserving existing external contracts:
- `LevelRunRequestDTO` input contract.
- `LevelRunResultDTO` output contract.
- Existing `FailureType` semantics.

## Goals / Non-Goals

**Goals:**
- Make `BootstrapRunner` a thin Unity facade that delegates orchestration.
- Introduce explicit orchestration flow in `RunAttemptUseCase`.
- Split responsibilities into dedicated services with clear ownership.
- Centralize result assembly and safe teardown.
- Preserve current runtime behavior and output format.

**Non-Goals:**
- Changing request/result JSON schema.
- Reworking level generation semantics.
- Replacing existing video recorder implementation details.
- Modifying scenes, prefabs, or package configuration.

## Decisions

### 1) Introduce `RunAttemptUseCase` as the orchestration root

Decision:
- Add `RunAttemptUseCase` as the only coordinator of a full run attempt.
- Execution order is explicit and linear:
  1. `RequestLoadingService`
  2. `RequestValidationService`
  3. `LevelPreparationService`
  4. `ConnectionProbeService`
  5. `AttemptArtifactsService` (layout + request copy)
  6. `RuntimeSceneService`
  7. `AttemptRuntimeBindingService`
  8. `AttemptExecutionService` + `AttemptVideoService`
  9. `AttemptResultFactory`
  10. `AttemptTeardownService`

Rationale:
- Eliminates hidden side effects in MonoBehaviour callbacks.
- Enables deterministic failure handling and easier extension points.

Alternatives considered:
- Keep orchestration in `BootstrapRunner` and only extract helpers. Rejected: still leaves lifecycle coupling and scattered failure paths.

### 2) Keep Unity API usage in Unity-facing services

Decision:
- `BootstrapRunner`, `RuntimeSceneService`, perimeter trigger subscription points, and video frame capture remain Unity adapter territory.
- Use-case and non-Unity services only coordinate outcomes and contracts.

Rationale:
- Matches existing architecture constraints and keeps main-thread Unity calls explicit.

Alternatives considered:
- Move scene/object operations into generic core services. Rejected: would leak Unity concerns into core and reduce portability.

### 3) Introduce explicit runtime binding and state handoff

Decision:
- `AttemptRuntimeBindingService` produces a runtime binding object containing references/subscriptions needed by execution.
- `AttemptExecutionService` consumes the runtime binding and owns attempt lifecycle tick/evaluation.

Rationale:
- Separates one-time wiring from per-frame behavior.
- Makes teardown deterministic (everything bound has a matching detach/dispose).

Alternatives considered:
- Let execution service discover references lazily every tick. Rejected: slower, less deterministic, and harder to fail fast.

### 4) Centralize teardown and result assembly

Decision:
- `AttemptResultFactory` becomes the single place that produces final `LevelRunResultDTO`.
- `AttemptTeardownService` becomes the single cleanup path (detach events, stop video if needed, unload scene, clear runtime state).

Rationale:
- Prevents duplicated cleanup code and inconsistent failure mapping.
- Guarantees one predictable terminal flow for success and failures.

Alternatives considered:
- Keep local `try/finally`-style cleanup per service. Rejected: hard to ensure consistency across all branches.

### 5) Keep existing concrete implementations where already valid

Decision:
- Reuse existing classes as implementation details where applicable:
  - `CommandLineRequestLoader` -> backing implementation for `RequestLoadingService`
  - `SocketConnectionProbe` -> backing implementation for `ConnectionProbeService`
  - `RuntimeAttemptSceneService` -> backing implementation for `RuntimeSceneService`
  - `AttemptArtifactsService` -> kept for artifact layout/copy/write responsibilities
  - `PngFrameCaptureVideoRecorder` + `IAttemptVideoRecorder` -> used by `AttemptVideoService`

Rationale:
- Reduces migration risk and avoids unnecessary rewrites.

Alternatives considered:
- Full class replacement. Rejected: high regression risk with low value.

## Risks / Trade-offs

- [Service graph complexity] -> Keep data contracts small and explicit for each handoff.
- [Behavior drift during extraction] -> Preserve existing logic paths first, then simplify incrementally.
- [Missed cleanup in edge failures] -> Route all terminal paths through `AttemptTeardownService` and keep it idempotent.
- [Tick-phase regressions] -> Keep per-frame execution non-blocking and avoid IO in `Update` loop.

## Migration Plan

1. Add orchestration contracts and data models for runtime handoffs.
2. Implement service wrappers around current logic.
3. Introduce `RunAttemptUseCase` and wire it into `BootstrapRunner`.
4. Move per-frame handling (`Update`) to delegated execution/video services.
5. Remove duplicated orchestration logic from `BootstrapRunner`.
6. Validate compile and smoke run.

Rollback strategy:
- Revert to previous `BootstrapRunner` implementation from git history if runtime regression is detected.

## Open Questions

- Should socket probe remain synchronous with timeout (current behavior) or become coroutine-based async?
- Should teardown attempt video stop even when capture never started (strict idempotency mode)?
- Do we need separate result reason templates for binding/setup failures vs execution failures?
