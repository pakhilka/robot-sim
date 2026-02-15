## 1. Specs & Planning

- [x] 1.1 Confirm and lock threshold rule: `distanceFront <= 5.0` => turn right
- [x] 1.2 Define/confirm right-turn motor command source in `BrainConfig`

## 2. Local Brain Implementation

- [x] 2.1 Update `LocalMockBrain` decision logic to return right-turn command when `distanceFront <= 5.0`
- [x] 2.2 Keep forward-drive behavior for `distanceFront > 5.0`
- [x] 2.3 Ensure threshold and right-turn command are config-driven in `BrainConfig`
- [x] 2.4 Add local-brain logs: start driving, received `distanceFront`, and right-turn trigger events

## 3. Validation

- [x] 3.1 Build/compile with no errors
- [ ] 3.2 Manual smoke checks for boundary values: `4.9`, `5.0`, `5.1`
