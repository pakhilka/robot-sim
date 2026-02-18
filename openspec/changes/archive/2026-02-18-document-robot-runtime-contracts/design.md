## Context

Robot runtime logic is split across Unity adapters (`RobotBrain`, `RobotBody`, `LaserDistanceSensor`) and core-like orchestration classes (`RobotController`, `SensorManager`, `IRobotBrain` implementations). Existing specs document local decision details, but there is no single contract for full control-loop ordering and adapter-level status/fallback behavior.

## Goals / Non-Goals

**Goals:**
- Define a normative runtime contract for robot FixedUpdate orchestration.
- Define normative adapter contracts for LocalMock and Wokwi TCP brains.
- Keep behavior documentation aligned with current implementation.

**Non-Goals:**
- Change `BrainConfig` defaults or alter threshold values.
- Refactor runtime architecture or move code between Unity/Core layers.
- Introduce new sensors, motor models, or networking protocols.

## Decisions

1. Add new capabilities instead of editing `local-brain-decision`.
- Rationale: `local-brain-decision` stays focused on obstacle rule tuning; new capabilities capture system-wide loop/adapter contracts.
- Alternative considered: expand local-brain spec. Rejected due to mixed concerns and reduced maintainability.

2. Specify control-loop behavior from observable runtime boundaries.
- Rationale: requirements are anchored to observable outcomes (order, payload, fallback, motor behavior) rather than private implementation details.
- Alternative considered: specify class internals and field-level mechanics. Rejected as brittle and over-constraining.

3. Encode safe-stop behavior for non-ready brain states.
- Rationale: Wokwi adapter and controller interplay relies on deterministic fallback command behavior when connection/handshake is incomplete.
- Alternative considered: leave as implicit adapter detail. Rejected due to operational risk during connectivity regressions.

## Risks / Trade-offs

- [Risk] Some requirements overlap with existing local-brain behavior docs.  
  Mitigation: keep this change focused on loop/adapter contracts and avoid threshold-policy duplication.
- [Risk] Current implementation uses Unity `Time.fixedDeltaTime` directly in orchestration code.  
  Mitigation: document current behavior as baseline and defer time-abstraction improvements to a later change.
- [Trade-off] Documentation-only scope does not improve runtime architecture today.  
  Mitigation: use resulting specs as guardrails for future refactors.
