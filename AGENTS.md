# OpenSpec + Beads Workflow for RobotSim

We operate in a cycle: OpenSpec (What) → Beads (Plan) → Code (Implementation).

## 0) Project Architecture (non-negotiable unless documented)
RobotSim uses Clean Architecture / Hexagonal (Ports & Adapters) adapted for Unity:

- Unity Adapters (MonoBehaviour): touch Unity API only (physics, raycasts, lifecycle).
- Core (Pure C#): brains, controllers, services, decision logic. Avoid UnityEngine dependencies where possible.
- Ports (Interfaces): `IRobotBrain`, `ISensor`, `ITcpClientService`, etc.
- Adapters (Implementations): `WokwiTcpBrain`, `TcpClientService`, sensors, bodies, orchestrators.

### Tick Contract (FixedUpdate loop)
Preferred flow:
1) Collect sensor data
2) Brain/controller decision
3) Apply motor command via RobotBody

Rules:
- The core tick must not block on I/O.
- Unity API calls happen only in Unity adapters on the main thread.

## 1) Repository Structure Rules
### Allowed modification zone (STRICT)
- `Assets/Scripts/**` is the only area the agent may modify by default.
- Everything else under `Assets/**` must not be changed without explicit permission.

### Data placement (STRICT)
All DTOs and result/data contracts must live under:
- `Assets/Scripts/**/Data/**` (or follow existing `RobotSim.Data/**` namespaces)

Do not scatter DTOs across random folders.

## 2) Guardrails (STRICT)
Forbidden areas (default):
- `ProjectSettings/**`
- `Packages/**`
- `Assets/**/*.unity` (scenes)
- Prefabs and other binary/asset configs by default

Secrets:
- Never place credentials in code, docs, issues, specs, or chat.

## 3) OpenSpec Process (mandatory for all changes)
Approval gate:
- Do NOT proceed to implementation until the user explicitly says "Go!".
- STRICTLY FORBIDDEN to bypass the flow: OpenSpec (What) -> Beads (Plan) -> Code (Implementation).

Task execution gate (STRICT):
- Only one task may be executed at a time.
- After finishing a task, the agent must present the result to the user and wait for user validation before commit.
- The next task must not start until the previous task is committed, unless the user explicitly grants an exception.

OpenSpec command execution (CLI):
- OpenSpec is used via CLI in the terminal (not as chat slash-commands).
- Do NOT invent command names or suggest PATH hacks.
- If an OpenSpec command is unknown/missing, ask the human to run: `openspec --help` (human-run).
- The agent must ask for explicit confirmation before running any terminal commands.

Beads generation fallback:
- If there is no automated "to-beads", the agent MUST:
  1) Read `openspec/changes/<change-id>/tasks.md`
  2) Create a Beads epic referencing `openspec/changes/<change-id>/`
  3) Create many small Beads tasks via `bd create`
  4) Add dependencies via `bd dep add`
  5) Execute via `bd ready`

Beads tool mode (environment note):
- In this repository, `bd` DB mode is currently unreliable.
- Use `bd --no-db ...` for issue operations until DB mode is fixed.

## 4) Quality Gates (current)
No automated tests yet.
Before marking work "done":
- compile with no Unity Console errors
- human manual smoke run

## 5) Git + Collaboration
Branch naming (required):
- `feature/<name>`
- `bugfix/<name>`
- `infrastructure/<name>`

Epic workflow (required):
- Before starting the first task in an epic, create and switch to a `feature/<epic-name>` branch.
- Commit each completed task in that epic separately (one task = one commit).
- Every task commit MUST also include the corresponding checkbox update in `openspec/changes/<change-id>/tasks.md` in the same commit (`- [ ]` -> `- [x]`).
- Do not batch multiple epic tasks into one commit unless the user explicitly approves.
- Never commit directly to `main`. All commits must be made in a non-`main` working branch.

Command execution policy:
- The agent may propose commands, but must ask for explicit confirmation before running any shell commands (including all git commands).

## 6) Session Handoff (mandatory)
At the end of each session:
- summary of changes
- pending tasks
- verification steps (commands; user runs them)
- next recommended `bd ready` task
