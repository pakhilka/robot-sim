# 5.2 Invalid Input Validation Matrix

Prepared malformed-schema requests under `Builds/requests/` and mapped expected
`invalid_input` reasons from `LevelMapValidator`:

| Request file | Expected validator reason |
| --- | --- |
| `request-invalid-unknown-symbol.json` | `Unknown map symbol 'W' at row 0, col 2.` |
| `request-invalid-non-rectangular.json` | `Map must be rectangular (all rows same length).` |
| `request-invalid-missing-finish.json` | `Map must contain exactly one F (Finish).` |
| `request-invalid-inconsistent-edges.json` | `Inconsistent connectivity between (...)` |

Result expectation:

- Runtime attempt startup must fail before simulation with
  `error.type = invalid_input`.
- Error `message` should contain one of the friendly reasons above.
