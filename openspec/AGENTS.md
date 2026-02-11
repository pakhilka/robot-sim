# OpenSpec + Beads Workflow for RobotSim

We operate in a cycle: **OpenSpec (What) → Beads (Plan) → Code (Implementation)**.

## 0) Project Architecture (non-negotiable unless documented)
RobotSim uses **Clean Architecture / Hexagonal (Ports & Adapters)** adapted for Unity:

- **Unity Adapters (MonoBehaviour)**: touch Unity API only (physics, raycasts, lifecycle).
- **Core (Pure C#)**: brains, controllers, services, decision logic. Avoid UnityEngine dependencies where possible.
- **Ports (Interfaces)**: `IRobotBrain`, `ISensor`, `ITcpClientService`, etc.
- **Adapters (Implementations)**: `WokwiTcpBrain`, `TcpClientService`, sensors, bodies, orchestrators.

### Tick Contract (FixedUpdate loop)
Preferred flow:
1. Collect sensor data
2. Brain/controller decision
3. Apply motor command via RobotBody

Rules:
- The core tick must **not block on I/O** (TCP must be non-blocking to the tick).
- Unity API calls happen **only** in Unity adapters on the main thread.

## 1) Repository Structure Rules
### Allowed modification zone (STRICT)
- `Assets/Scripts/**` is the only area the agent may modify by default.
- Everything else under `Assets/**` (art, settings, assets, input assets, etc.) must not be changed without explicit permission.

### Data placement (STRICT)
All DTOs, result objects, and low-level data contracts must live under:
- `Assets/Scripts/**/Data/**` (or the existing `RobotSim.Data/**` namespace structure)

Rule: do not scatter DTOs/data contracts across random folders. Keep them grouped under Data.
If the repo already has a specific layout (e.g., `RobotSim.Data.DTOs`, `RobotSim.Data.Results`, `RobotSim.Interfaces`), follow it consistently.

## 2) Guardrails (STRICT)
### Forbidden areas (default)
- `ProjectSettings/**`
- `Packages/**`
- `Assets/**/*.unity` (scenes)
- Prefabs and other binary/asset configs by default

If a task requires changes there, STOP and request explicit permission.

### Secrets
- Never place credentials in code, docs, issues, specs, or chat.
- Use environment variables, local-only config (excluded from git), or secure secret storage.

## 3) OpenSpec Process (mandatory for all changes)
### Intent Formation
User starts with:
`/openspec-proposal "Add X"`

OpenSpec creates `openspec/changes/<change-id>/` containing:
- `proposal.md` (business value and scope)
- `tasks.md` (high-level task list)
- `design.md` (optional)
- `specs/**/spec.md` (requirements and acceptance criteria)

**Agent goal**: iterate these files until they represent a signable contract.

**CRITICAL**: Do NOT proceed to implementation until the user explicitly says **"Go!"**.

### Task Transformation (to Beads)
After approval:
`/openspec-to-beads <change-id>`

The agent must:
1. Read the change files.
2. Create a Beads epic for the feature, referencing `openspec/changes/<change-id>/`.
3. Create Beads tasks for each item in `tasks.md` (prefer many small tasks).
4. Set dependencies where needed.

### Execution
Work loop:
- `bd ready`
- `bd show <task-id>`
- implement code changes
- `bd close <task-id>`
- `bd sync`

Pop-up fixes are allowed but discouraged; they must be tracked.

## 4) Quality Gates (current)
There are no automated tests yet.

Before marking work “done”:
- (a) project compiles with **no Unity Console errors**
- (d) perform a **manual smoke run** (performed by the human, not the agent):
  - open the simulation scene
  - press Play
  - confirm the robot moves and reacts to obstacles
  - confirm no Console errors during runtime

(When tests exist, update gates to include EditMode tests.)

## 5) Git + Collaboration
- Work on feature branches.
- One PR per feature.
- Code review is expected to preserve style and architectural boundaries.

### Branch naming (required)
- Features: `feature/<name>`
- Bug fixes: `bugfix/<name>`
- (Optional) Chores/infra: `chore/<name>`

Rules:
- lowercase + hyphens only
- short and descriptive

## 6) Command Execution Policy (Codex)
- The agent may propose commands.
- The agent must ask for explicit confirmation before running any shell commands.
- **This includes all git commands** (`status/add/commit/push/pull/rebase`, etc.).

## 7) Session Handoff (mandatory)
At the end of each session, provide:
- Summary of changes
- Pending tasks
- Verification steps (commands; user runs them)
- Next recommended `bd ready` task
