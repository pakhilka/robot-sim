## 1. Runtime Contract Validation

- [ ] 1.1 Re-verify `RobotBrain.FixedUpdate` execution order (sensors -> controller -> body command).
- [ ] 1.2 Re-verify null-guard behavior when body/controller references are missing.
- [ ] 1.3 Re-verify `SensorDataDTO` payload composition from `SensorManager`.

## 2. Brain Adapter Contract Validation

- [ ] 2.1 Re-verify `BrainSelector` mapping for `LocalMock` and `WokwiTcp` types.
- [ ] 2.2 Re-verify LocalMock step result contract (`Ready` status + config-driven commands).
- [ ] 2.3 Re-verify Wokwi safe-stop behavior before ready handshake and reconnect flow.
- [ ] 2.4 Re-verify `RobotController` non-ready bypass and ready-state alarm stabilization behavior.

## 3. Spec Lifecycle Completion

- [ ] 3.1 Run `openspec status --change document-robot-runtime-contracts` and confirm all artifacts complete.
- [ ] 3.2 Run `openspec verify --change document-robot-runtime-contracts` and resolve any requirement mismatches.
- [ ] 3.3 Prepare sync/archive follow-up once review confirms this as source of truth.
