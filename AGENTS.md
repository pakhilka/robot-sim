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

Command execution policy:
- The agent may propose commands, but must ask for explicit confirmation before running any shell commands (including all git commands).

## 6) Session Handoff (mandatory)
At the end of each session:
- summary of changes
- pending tasks
- verification steps (commands; user runs them)
- next recommended `bd ready` task
